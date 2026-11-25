using FluentAssertions;
using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;
using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Application.Common.DTOs.ClientDTOs;
using Insurance.Application.Common.DTOs.GeographyDTOs;
using Insurance.Application.Common.DTOs.PolicyDTOs;
using Insurance.Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace Insurance.Tests.Integration;

[TestFixture]
public sealed class PoliciesFlowTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();

            var token = TestJwtFactory.CreateAdminToken(
                    key: _factory.JwtKey,
                    issuer: _factory.JwtIssuer,
                    audience: _factory.JwtAudience);

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task FullFlow_CreateDraft_Activate_Cancel_Works()
        {
            var brokersResp = await _client.GetAsync("/api/admin/brokers?page=1&pageSize=50");
            brokersResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var brokersPaged = await brokersResp.Content.ReadFromJsonAsync<PagedResult<BrokerResponseDTO>>(TestJson.Options);
            brokersPaged!.Items.Should().NotBeEmpty();
            var brokerId = brokersPaged.Items.First().Id;

            var currencies = await _client.GetFromJsonAsync<CurrencyResponseDTO[]>("/api/admin/currencies", TestJson.Options);
            currencies!.Should().Contain(x => x.Code == "RON");
            var ronId = currencies.First(x => x.Code == "RON").Id;

            var createClient = new CreateClientRequestDTO
            {
                Type = ClientType.Individual,
                Name = "Policy Flow Client",
                IdentificationNumber = $"CNP-{Guid.NewGuid():N}",
                Email = "c@local.com",
                Phone = "0700",
                PrimaryAddress = "addr"
            };

            var clientResp = await _client.PostAsJsonAsync("/api/brokers/clients", createClient, TestJson.Options);
            clientResp.StatusCode.Should().Be(HttpStatusCode.Created);

            var clientBody = await clientResp.Content.ReadFromJsonAsync<Dictionary<string, string>>(TestJson.Options);
            var clientId = Guid.Parse(clientBody!["id"]);

            var countries = await _client.GetFromJsonAsync<List<CountryResponseDTO>>("/api/brokers/countries", TestJson.Options);
            countries!.Should().NotBeEmpty();
            var countryId = countries[0].Id;

            var counties = await _client.GetFromJsonAsync<List<CountyResponseDTO>>($"/api/brokers/countries/{countryId}/counties", TestJson.Options);
            counties!.Should().NotBeEmpty();
            var countyId = counties[0].Id;

            var cities = await _client.GetFromJsonAsync<List<CityResponseDTO>>($"/api/brokers/counties/{countyId}/cities", TestJson.Options);
            cities!.Should().NotBeEmpty();
            var cityId = cities[0].Id;

            var createBuilding = new CreateBuildingRequestDTO
            {
                Street = "Street",
                Number = "10",
                CityId = cityId,
                ConstructionYear = 2005,
                BuildingType = BuildingType.Residential,
                Floors = 2,
                SurfaceArea = 100,
                InsuredValue = 300000,
                IsFloodZone = false,
                EarthquakeRiskZone = 0
            };

            var buildingResp = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", createBuilding, TestJson.Options);
            buildingResp.StatusCode.Should().Be(HttpStatusCode.Created);

            var buildingBody = await buildingResp.Content.ReadFromJsonAsync<Dictionary<string, string>>(TestJson.Options);
            var buildingId = Guid.Parse(buildingBody!["id"]);

            var createPolicy = new CreatePolicyRequestDTO
            {
                ClientId = clientId,
                BuildingId = buildingId,
                BrokerId = brokerId,
                CurrencyId = ronId,
                BasePremium = 1000m,
                StartDateUtc = DateTime.UtcNow.Date.AddDays(3),
                EndDateUtc = DateTime.UtcNow.Date.AddYears(1).AddDays(3)
            };

            var policyResp = await _client.PostAsJsonAsync("/api/brokers/policies", createPolicy, TestJson.Options);
            policyResp.StatusCode.Should().Be(HttpStatusCode.Created);

            var policyBody = await policyResp.Content.ReadFromJsonAsync<Dictionary<string, string>>(TestJson.Options);
            var policyId = Guid.Parse(policyBody!["id"]);

            var policyDetailsResp = await _client.GetAsync($"/api/brokers/policies/{policyId}");
            policyDetailsResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var details = await policyDetailsResp.Content.ReadFromJsonAsync<PolicyDetailsResponseDTO>(TestJson.Options);
            details.Should().NotBeNull();
            details!.Id.Should().Be(policyId);
            details.Status.Should().Be(PolicyStatus.Draft);
            details.BasePremium.Should().Be(1000m);
            details.FinalPremium.Should().BeGreaterThan(0);
            details.CityId.Should().Be(cityId);
            details.CurrencyId.Should().Be(ronId);

            var actResp = await _client.PostAsync($"/api/brokers/policies/{policyId}/activate", null);
            actResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var cancel = new CancelPolicyRequestDTO
            {
                CancelledAtUtc = DateTime.UtcNow,
                Reason = "Test cancel"
            };

            var cancelResp = await _client.PostAsJsonAsync($"/api/brokers/policies/{policyId}/cancel", cancel, TestJson.Options);
            cancelResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var afterCancel = await _client.GetFromJsonAsync<PolicyDetailsResponseDTO>($"/api/brokers/policies/{policyId}", TestJson.Options);
            afterCancel!.Status.Should().Be(PolicyStatus.Cancelled);
            afterCancel.CancellationReason.Should().Be("Test cancel");
            afterCancel.CancelledAtUtc.Should().NotBeNull();
        }

        [Test]
        public async Task CreatePolicy_WithInactiveBroker_ReturnsBadRequest()
        {
            var code = $"BRK-{Guid.NewGuid():N}".Substring(0, 10).ToUpperInvariant();

            var brokerResp = await _client.PostAsJsonAsync("/api/admin/brokers", new CreateBrokerRequestDTO
            {
                BrokerCode = code,
                Name = "Inactive Broker",
                Email = "ib@local.com",
                Status = BrokerStatus.Active
            }, TestJson.Options);

            brokerResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var brokerBody = await brokerResp.Content.ReadFromJsonAsync<Dictionary<string, string>>(TestJson.Options);
            var brokerId = Guid.Parse(brokerBody!["id"]);

            var deactivate = await _client.PostAsync($"/api/admin/brokers/{brokerId}/deactivate", null);
            deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var currencies = await _client.GetFromJsonAsync<CurrencyResponseDTO[]>("/api/admin/currencies", TestJson.Options);
            var ronId = currencies!.First(x => x.Code == "RON").Id;

            var clientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new CreateClientRequestDTO
            {
                Type = ClientType.Individual,
                Name = "Client",
                IdentificationNumber = $"CNP-{Guid.NewGuid():N}",
            }, TestJson.Options);

            clientResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var clientBody = await clientResp.Content.ReadFromJsonAsync<Dictionary<string, string>>(TestJson.Options);
            var clientId = Guid.Parse(clientBody!["id"]);

            var countries = await _client.GetFromJsonAsync<List<CountryResponseDTO>>("/api/brokers/countries", TestJson.Options);
            var countryId = countries![0].Id;

            var counties = await _client.GetFromJsonAsync<List<CountyResponseDTO>>($"/api/brokers/countries/{countryId}/counties", TestJson.Options);
            var countyId = counties![0].Id;

            var cities = await _client.GetFromJsonAsync<List<CityResponseDTO>>($"/api/brokers/counties/{countyId}/cities", TestJson.Options);
            var cityId = cities![0].Id;

            var buildingResp = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", new CreateBuildingRequestDTO
            {
                Street = "S",
                Number = "1",
                CityId = cityId,
                ConstructionYear = 2000,
                BuildingType = BuildingType.Residential,
                Floors = 1,
                SurfaceArea = 50,
                InsuredValue = 100000
            }, TestJson.Options);

            buildingResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var buildingBody = await buildingResp.Content.ReadFromJsonAsync<Dictionary<string, string>>(TestJson.Options);
            var buildingId = Guid.Parse(buildingBody!["id"]);

            var badPolicy = new CreatePolicyRequestDTO
            {
                ClientId = clientId,
                BuildingId = buildingId,
                BrokerId = brokerId,
                CurrencyId = ronId,
                BasePremium = 1000m,
                StartDateUtc = DateTime.UtcNow.Date.AddDays(2),
                EndDateUtc = DateTime.UtcNow.Date.AddDays(30)
            };

            var policyResp = await _client.PostAsJsonAsync("/api/brokers/policies", badPolicy, TestJson.Options);
            policyResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
