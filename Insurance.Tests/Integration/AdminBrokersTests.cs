using FluentAssertions;
using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;
using Insurance.Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace Insurance.Tests.Integration;

[TestFixture]
public sealed class AdminBrokersTests
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
        public async Task List_ReturnsSeededBroker()
        {
            var resp = await _client.GetAsync("/api/admin/brokers?page=1&pageSize=50");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var paged = await resp.Content.ReadFromJsonAsync<PagedResult<BrokerResponseDTO>>(TestJson.Options);
            paged.Should().NotBeNull();
            paged!.Items.Should().NotBeEmpty();
            paged.Items.Should().Contain(x => x.BrokerCode == "BRK-001");
        }

        [Test]
        public async Task Create_GetById_Deactivate_Activate_Works()
        {
            var create = new CreateBrokerRequestDTO
            {
                BrokerCode = "BRK-999",
                Name = "Test Broker",
                Email = "tb@local.com",
                Phone = "123",
                Status = BrokerStatus.Active,
                CommissionPercentage = 0.05m
            };

            var createResp = await _client.PostAsJsonAsync("/api/admin/brokers", create, TestJson.Options);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResp.Content.ReadFromJsonAsync<Dictionary<string, string>>(TestJson.Options);
            created.Should().NotBeNull();
            created!.Should().ContainKey("id");
            var id = created["id"];

            var getResp = await _client.GetAsync($"/api/admin/brokers/{id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var broker = await getResp.Content.ReadFromJsonAsync<BrokerResponseDTO>(TestJson.Options);
            broker.Should().NotBeNull();
            broker!.BrokerCode.Should().Be("BRK-999");
            broker.Name.Should().Be("Test Broker");

            var deactResp = await _client.PostAsync($"/api/admin/brokers/{id}/deactivate", null);
            deactResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var actResp = await _client.PostAsync($"/api/admin/brokers/{id}/activate", null);
            actResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task Create_DuplicateCode_ReturnsConflict()
        {
            var create = new CreateBrokerRequestDTO
            {
                BrokerCode = "BRK-DUP",
                Name = "Broker 1",
                Email = "b1@local.com",
                Status = BrokerStatus.Active
            };

            var r1 = await _client.PostAsJsonAsync("/api/admin/brokers", create, TestJson.Options);
            r1.StatusCode.Should().Be(HttpStatusCode.Created);

            var r2 = await _client.PostAsJsonAsync("/api/admin/brokers", create, TestJson.Options);
            r2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
