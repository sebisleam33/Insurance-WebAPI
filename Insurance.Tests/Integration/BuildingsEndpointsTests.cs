using FluentAssertions;
using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Application.Common.DTOs.ClientDTOs;
using Insurance.Application.Common.DTOs.GeographyDTOs;
using System.Net;
using System.Net.Http.Json;

namespace Insurance.Tests.Integration;

    [TestFixture]
    public class BuildingsEndpointsTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

    [Test]
    public async Task CreateClient_CreateBuilding_GetBuilding_Returns200()
    {
        var postClient = await _client.PostAsJsonAsync("/api/brokers/clients", new CreateClientRequestDTO
        {
            Name = "Owner",
            IdentificationNumber = "OWN-1"
        });

        postClient.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdClient = await postClient.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        createdClient.Should().NotBeNull();
        var clientId = createdClient!["id"];

        var countries = await _client.GetFromJsonAsync<List<CountryResponseDTO>>("/api/brokers/countries");
        countries.Should().NotBeNull().And.NotBeEmpty();
        var countryId = countries![0].Id;

        var counties = await _client.GetFromJsonAsync<List<CountyResponseDTO>>($"/api/brokers/countries/{countryId}/counties");
        counties.Should().NotBeNull().And.NotBeEmpty();
        var countyId = counties![0].Id;

        var cities = await _client.GetFromJsonAsync<List<CityResponseDTO>>($"/api/brokers/counties/{countyId}/cities");
        cities.Should().NotBeNull().And.NotBeEmpty();
        var cityId = cities![0].Id;

        var postBuilding = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", new CreateBuildingRequestDTO
        {
            Street = "Main",
            Number = "1",
            CityId = cityId,
            ConstructionYear = 2000,
            SurfaceArea = 100,
            InsuredValue = 100000
        });

        postBuilding.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBuilding = await postBuilding.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        createdBuilding.Should().NotBeNull();
        var buildingId = createdBuilding!["id"];

        var get = await _client.GetAsync($"/api/brokers/buildings/{buildingId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
        public async Task CreateBuilding_InvalidCity_Returns400()
        {
            var postClient = await _client.PostAsJsonAsync("/api/brokers/clients", new CreateClientRequestDTO
            {
                Name = "Owner",
                IdentificationNumber = "OWN-2"
            });
            var clientId = (await postClient.Content.ReadFromJsonAsync<Dictionary<string, string>>())!["id"];

            var postBuilding = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", new CreateBuildingRequestDTO
            {
                Street = "Main",
                Number = "1",
                CityId = 999999,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 100000
            });

            postBuilding.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
