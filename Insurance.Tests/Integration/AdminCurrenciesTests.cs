using FluentAssertions;
using Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;
using System.Net;
using System.Net.Http.Json;

namespace Insurance.Tests.Integration;

[TestFixture]
public sealed class AdminCurrenciesTests
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
        public async Task GetAll_ReturnsSeededCurrencies()
        {
            var resp = await _client.GetAsync("/api/admin/currencies");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var currencies = await resp.Content.ReadFromJsonAsync<CurrencyResponseDTO[]>(TestJson.Options);
            currencies.Should().NotBeNull();
            currencies!.Should().Contain(x => x.Code == "RON");
            currencies.Should().Contain(x => x.Code == "EUR");
        }

        [Test]
        public async Task Create_ThenUpdate_ThenGetById_Works()
        {
            var create = new CreateCurrencyRequestDTO
            {
                Code = "USD",
                Name = "US Dollar",
                ExchangeRateToBase = 4.6m,
                IsActive = true
            };

            var createResp = await _client.PostAsJsonAsync("/api/admin/currencies", create, TestJson.Options);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResp.Content.ReadFromJsonAsync<Dictionary<string, int>>(TestJson.Options);
            created.Should().NotBeNull();
            created!.Should().ContainKey("id");
            var id = created["id"];

            var update = new UpdateCurrencyRequestDTO
            {
                Name = "US Dollar Updated",
                ExchangeRateToBase = 4.7m,
                IsActive = true
            };

            var updateResp = await _client.PutAsJsonAsync($"/api/admin/currencies/{id}", update, TestJson.Options);
            updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var byIdResp = await _client.GetAsync($"/api/admin/currencies/{id}");
            byIdResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await byIdResp.Content.ReadFromJsonAsync<CurrencyResponseDTO>(TestJson.Options);
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.Code.Should().Be("USD");
            dto.Name.Should().Be("US Dollar Updated");
            dto.ExchangeRateToBase.Should().Be(4.7m);
        }

        [Test]
        public async Task Create_DuplicateCode_ReturnsConflict()
        {
            var create = new CreateCurrencyRequestDTO
            {
                Code = "USD",
                Name = "US Dollar",
                ExchangeRateToBase = 4.6m,
                IsActive = true
            };

            var r1 = await _client.PostAsJsonAsync("/api/admin/currencies", create, TestJson.Options);
            r1.StatusCode.Should().Be(HttpStatusCode.Created);

            var r2 = await _client.PostAsJsonAsync("/api/admin/currencies", create, TestJson.Options);
            r2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
