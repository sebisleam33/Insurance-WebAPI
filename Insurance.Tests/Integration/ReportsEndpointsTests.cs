using FluentAssertions;
using Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;
using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;
using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Insurance.Tests.Integration;

[TestFixture]
public sealed class ReportsEndpointsTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

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
        public async Task PoliciesByBroker_ReturnsAggregates_AndStatusFilterChangesResults()
        {
            await SeedPoliciesForReportsAsync(eurRateToBase: 5m, ronRateToBase: 1m);

            var from = "2026-01-01T00:00:00Z";
            var to = "2026-01-31T23:59:59Z";

            var resp = await _client.GetAsync($"/api/admin/reports/policies-by-broker?fromUtc={from}&toUtc={to}");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var rows = await ReadAsAsync<List<PoliciesByBrokerRowDTO>>(resp);

            var brkAEur = rows.Single(r => r.BrokerCode == "BRK-A" && r.CurrencyCode == "EUR");
            brkAEur.PolicyCount.Should().Be(2);
            brkAEur.TotalFinalPremium.Should().Be(150m);
            brkAEur.TotalFinalPremiumInBaseCurrency.Should().Be(750m); // 150 * 5

            var resp2 = await _client.GetAsync($"/api/admin/reports/policies-by-broker?fromUtc={from}&toUtc={to}&status=Active");
            resp2.StatusCode.Should().Be(HttpStatusCode.OK);

            var rows2 = await ReadAsAsync<List<PoliciesByBrokerRowDTO>>(resp2);

            var brkAEurActive = rows2.Single(r => r.BrokerCode == "BRK-A" && r.CurrencyCode == "EUR");
            brkAEurActive.PolicyCount.Should().Be(1);
            brkAEurActive.TotalFinalPremium.Should().Be(100m);
            brkAEurActive.TotalFinalPremiumInBaseCurrency.Should().Be(500m); // 100 * 5
        }

        [Test]
        public async Task PoliciesByCity_ReturnsAggregates_AndCurrencyFilterWorks()
        {
            var (eurId, _, _) = await SeedPoliciesForReportsAsync(eurRateToBase: 5m, ronRateToBase: 1m);

            var from = "2026-01-01T00:00:00Z";
            var to = "2026-01-31T23:59:59Z";

            var resp = await _client.GetAsync($"/api/admin/reports/policies-by-city?fromUtc={from}&toUtc={to}&currencyId={eurId}");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var rows = await ReadAsAsync<List<PoliciesByCityRowDTO>>(resp);

            rows.Should().NotBeEmpty();
            rows.Should().OnlyContain(r => r.CurrencyId == eurId);

            var bucEur = rows.Single(r => r.CityName == "Bucharest" && r.CurrencyCode == "EUR");
            bucEur.PolicyCount.Should().Be(2);
            bucEur.TotalFinalPremium.Should().Be(150m);
            bucEur.TotalFinalPremiumInBaseCurrency.Should().Be(750m); // 150 * 5
        }

        private static async Task<T> ReadAsAsync<T>(HttpResponseMessage resp)
        {
            var json = await resp.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<T>(json, JsonOpts);
            obj.Should().NotBeNull("response should deserialize to expected DTO type");
            return obj!;
        }
        private async Task<(int eurId, int ronId, int cityId)> SeedPoliciesForReportsAsync(decimal eurRateToBase, decimal ronRateToBase)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await ResetTransactionalDataAsync(db);

            var cityId = await EnsureBucharestCityAsync(db);

            var eur = await EnsureCurrencyAsync(db, code: "EUR", nameFallback: "Euro", exchangeRateToBase: eurRateToBase);
            var ron = await EnsureCurrencyAsync(db, code: "RON", nameFallback: "Romanian Leu", exchangeRateToBase: ronRateToBase);

            var brokerA = new Broker
            {
                Id = Guid.NewGuid(),
                BrokerCode = "BRK-A",
                Name = "Broker A",
                Status = BrokerStatus.Active
            };
            var brokerB = new Broker
            {
                Id = Guid.NewGuid(),
                BrokerCode = "BRK-B",
                Name = "Broker B",
                Status = BrokerStatus.Active
            };
            db.Brokers.AddRange(brokerA, brokerB);

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = $"CNP-{Guid.NewGuid():N}".Substring(0, 12)
            };
            db.Clients.Add(client);

            var building = new Building
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                CityId = cityId,
                Street = "Main",
                Number = "1",
                ConstructionYear = 2000,
                BuildingType = BuildingType.Office,
                Floors = 1,
                SurfaceArea = 50m,
                InsuredValue = 100000m
            };
            db.Buildings.Add(building);

            var now = DateTime.UtcNow;

            db.Policies.AddRange(
                new Policy
                {
                    Id = Guid.NewGuid(),
                    PolicyNumber = $"POL-1-{Guid.NewGuid():N}",
                    ClientId = client.Id,
                    BuildingId = building.Id,
                    BrokerId = brokerA.Id,
                    CurrencyId = eur.Id,
                    Status = PolicyStatus.Active,
                    StartDateUtc = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    EndDateUtc = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    BasePremium = 80m,
                    FinalPremium = 100m,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new Policy
                {
                    Id = Guid.NewGuid(),
                    PolicyNumber = $"POL-2-{Guid.NewGuid():N}",
                    ClientId = client.Id,
                    BuildingId = building.Id,
                    BrokerId = brokerA.Id,
                    CurrencyId = eur.Id,
                    Status = PolicyStatus.Cancelled,
                    StartDateUtc = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                    EndDateUtc = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    BasePremium = 40m,
                    FinalPremium = 50m,
                    CancelledAtUtc = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc),
                    CancellationReason = "x",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new Policy
                {
                    Id = Guid.NewGuid(),
                    PolicyNumber = $"POL-3-{Guid.NewGuid():N}",
                    ClientId = client.Id,
                    BuildingId = building.Id,
                    BrokerId = brokerB.Id,
                    CurrencyId = ron.Id,
                    Status = PolicyStatus.Active,
                    StartDateUtc = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                    EndDateUtc = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    BasePremium = 20m,
                    FinalPremium = 25m,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });

            await db.SaveChangesAsync();

            return (eur.Id, ron.Id, cityId);
        }

        private static async Task ResetTransactionalDataAsync(AppDbContext db)
        {
            db.Policies.RemoveRange(db.Policies);
            db.Buildings.RemoveRange(db.Buildings);
            db.ClientIdentifierChanges.RemoveRange(db.ClientIdentifierChanges);
            db.Clients.RemoveRange(db.Clients);
            db.Brokers.RemoveRange(db.Brokers);

            await db.SaveChangesAsync();
        }

        private static async Task<int> EnsureBucharestCityAsync(AppDbContext db)
        {
            var existingCity = await db.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name == "Bucharest");

            if (existingCity is not null)
            {
                return existingCity.Id;
            }

            var country = await db.Countries.FirstOrDefaultAsync(c => c.Name == "Romania")
                          ?? (await db.Countries.AddAsync(new Country { Name = "Romania" })).Entity;

            await db.SaveChangesAsync();

            var county = await db.Counties.FirstOrDefaultAsync(c => c.Name == "Bucharest" && c.CountryId == country.Id)
                         ?? (await db.Counties.AddAsync(new County { Name = "Bucharest", CountryId = country.Id })).Entity;

            await db.SaveChangesAsync();

            var city = new City { Name = "Bucharest", CountyId = county.Id };
            db.Cities.Add(city);
            await db.SaveChangesAsync();

            return city.Id;
        }

        private static async Task<Currency> EnsureCurrencyAsync(AppDbContext db, string code, string nameFallback, decimal exchangeRateToBase)
        {
            var cur = await db.Currencies.FirstOrDefaultAsync(c => c.Code == code);

            if (cur is null)
            {
                cur = new Currency
                {
                    Code = code,
                    Name = nameFallback,
                    IsActive = true,
                    ExchangeRateToBase = exchangeRateToBase
                };
                db.Currencies.Add(cur);
            }
            else
            {
                cur.ExchangeRateToBase = exchangeRateToBase;
                cur.IsActive = true;
                if (string.IsNullOrWhiteSpace(cur.Name))
                    cur.Name = nameFallback;
            }

            await db.SaveChangesAsync();
            return cur;
        }
    }
