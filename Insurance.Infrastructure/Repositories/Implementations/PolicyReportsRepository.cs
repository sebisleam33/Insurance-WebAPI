using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Reports;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

public sealed class PolicyReportsRepository : IPolicyReportsRepository
    {
        private readonly AppDbContext _db;
        public PolicyReportsRepository(AppDbContext db) => _db = db;

        private IQueryable<Policy> BaseQuery(PolicyReportQuery q)
        {
            var query = _db.Policies.AsNoTracking()
                .Where(p => p.StartDateUtc >= q.FromUtc && p.StartDateUtc <= q.ToUtc);

            if (q.Status.HasValue)
            {
                query = query.Where(p => p.Status == q.Status.Value);
            }

            if (q.CurrencyId.HasValue)
            {
                query = query.Where(p => p.CurrencyId == q.CurrencyId.Value);
            }

            if (q.BuildingType.HasValue)
            {
                query = query.Where(p => p.Building.BuildingType == q.BuildingType.Value);
            }

            return query;
        }

        private sealed record FlatRow(
            int CurrencyId,
            string CurrencyCode,
            decimal FinalPremium,
            decimal FinalPremiumInBaseCurrency,

            int CountryId,
            string CountryName,

            int CountyId,
            string CountyName,

            int CityId,
            string CityName,

            Guid BrokerId,
            string BrokerCode,
            string BrokerName
        );

        private async Task<List<FlatRow>> LoadFlatAsync(PolicyReportQuery query, CancellationToken ct)
        {
            return await BaseQuery(query)
                .Select(p => new FlatRow(
                    p.CurrencyId,
                    p.Currency.Code,
                    p.FinalPremium,
                    p.FinalPremium * p.Currency.ExchangeRateToBase,

                    p.Building.City.County.CountryId,
                    p.Building.City.County.Country.Name,

                    p.Building.City.CountyId,
                    p.Building.City.County.Name,

                    p.Building.CityId,
                    p.Building.City.Name,

                    p.BrokerId,
                    p.Broker.BrokerCode,
                    p.Broker.Name
                ))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PoliciesByCountryReportRow>> PoliciesByCountryAsync(
            PolicyReportQuery query,
            CancellationToken ct = default)
        {
            var flat = await LoadFlatAsync(query, ct);

            return flat
                .GroupBy(x => new { x.CountryId, x.CountryName, x.CurrencyId, x.CurrencyCode })
                .Select(g => new PoliciesByCountryReportRow
                {
                    CountryId = g.Key.CountryId,
                    CountryName = g.Key.CountryName,
                    CurrencyId = g.Key.CurrencyId,
                    CurrencyCode = g.Key.CurrencyCode,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremiumInBaseCurrency)
                })
                .OrderBy(r => r.CountryName)
                .ThenBy(r => r.CurrencyCode)
                .ToList();
        }

        public async Task<IReadOnlyList<PoliciesByCountyReportRow>> PoliciesByCountyAsync(
            PolicyReportQuery query,
            CancellationToken ct = default)
        {
            var flat = await LoadFlatAsync(query, ct);

            return flat
                .GroupBy(x => new
                {
                    x.CountryId,
                    x.CountryName,
                    x.CountyId,
                    x.CountyName,
                    x.CurrencyId,
                    x.CurrencyCode
                })
                .Select(g => new PoliciesByCountyReportRow
                {
                    CountryId = g.Key.CountryId,
                    CountryName = g.Key.CountryName,
                    CountyId = g.Key.CountyId,
                    CountyName = g.Key.CountyName,
                    CurrencyId = g.Key.CurrencyId,
                    CurrencyCode = g.Key.CurrencyCode,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremiumInBaseCurrency)
                })
                .OrderBy(r => r.CountryName)
                .ThenBy(r => r.CountyName)
                .ThenBy(r => r.CurrencyCode)
                .ToList();
        }

        public async Task<IReadOnlyList<PoliciesByCityReportRow>> PoliciesByCityAsync(
            PolicyReportQuery query,
            CancellationToken ct = default)
        {
            var flat = await LoadFlatAsync(query, ct);

            return flat
                .GroupBy(x => new
                {
                    x.CountryId,
                    x.CountryName,
                    x.CountyId,
                    x.CountyName,
                    x.CityId,
                    x.CityName,
                    x.CurrencyId,
                    x.CurrencyCode
                })
                .Select(g => new PoliciesByCityReportRow
                {
                    CountryId = g.Key.CountryId,
                    CountryName = g.Key.CountryName,
                    CountyId = g.Key.CountyId,
                    CountyName = g.Key.CountyName,
                    CityId = g.Key.CityId,
                    CityName = g.Key.CityName,
                    CurrencyId = g.Key.CurrencyId,
                    CurrencyCode = g.Key.CurrencyCode,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremiumInBaseCurrency)
                })
                .OrderBy(r => r.CountryName)
                .ThenBy(r => r.CountyName)
                .ThenBy(r => r.CityName)
                .ThenBy(r => r.CurrencyCode)
                .ToList();
        }

        public async Task<IReadOnlyList<PoliciesByBrokerReportRow>> PoliciesByBrokerAsync(
            PolicyReportQuery query,
            CancellationToken ct = default)
        {
            var flat = await LoadFlatAsync(query, ct);

            return flat
                .GroupBy(x => new { x.BrokerId, x.BrokerCode, x.BrokerName, x.CurrencyId, x.CurrencyCode })
                .Select(g => new PoliciesByBrokerReportRow
                {
                    BrokerId = g.Key.BrokerId,
                    BrokerCode = g.Key.BrokerCode,
                    BrokerName = g.Key.BrokerName,
                    CurrencyId = g.Key.CurrencyId,
                    CurrencyCode = g.Key.CurrencyCode,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremiumInBaseCurrency)
                })
                .OrderBy(r => r.BrokerCode)
                .ThenBy(r => r.CurrencyCode)
                .ToList();
        }
    }
