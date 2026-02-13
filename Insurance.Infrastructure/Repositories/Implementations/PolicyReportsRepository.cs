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

        public async Task<IReadOnlyList<PoliciesByCountryReportRow>> PoliciesByCountryAsync(PolicyReportQuery query, CancellationToken ct = default)
        {
            var q = BaseQuery(query);

            return await q
                .GroupBy(p => new
                {
                    p.Building.City.County.CountryId,
                    CountryName = p.Building.City.County.Country.Name,
                    p.CurrencyId,
                    CurrencyCode = p.Currency.Code,
                    ExchangeRateToBase = p.Currency.ExchangeRateToBase
                })
                .Select(g => new PoliciesByCountryReportRow
                {
                    CountryId = g.Key.CountryId,
                    CountryName = g.Key.CountryName,
                    CurrencyId = g.Key.CurrencyId,
                    CurrencyCode = g.Key.CurrencyCode,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremium * x.Currency.ExchangeRateToBase)
                })
                .OrderBy(r => r.CountryName)
                .ThenBy(r => r.CurrencyCode)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PoliciesByCountyReportRow>> PoliciesByCountyAsync(PolicyReportQuery query, CancellationToken ct = default)
        {
            var q = BaseQuery(query);

            return await q
                .GroupBy(p => new
                {
                    p.Building.City.County.CountryId,
                    CountryName = p.Building.City.County.Country.Name,
                    CountyId = p.Building.City.CountyId,
                    CountyName = p.Building.City.County.Name,
                    p.CurrencyId,
                    CurrencyCode = p.Currency.Code
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
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremium * x.Currency.ExchangeRateToBase)
                })
                .OrderBy(r => r.CountryName)
                .ThenBy(r => r.CountyName)
                .ThenBy(r => r.CurrencyCode)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PoliciesByCityReportRow>> PoliciesByCityAsync(PolicyReportQuery query, CancellationToken ct = default)
        {
            var q = BaseQuery(query);

            return await q
                .GroupBy(p => new
                {
                    p.Building.City.County.CountryId,
                    CountryName = p.Building.City.County.Country.Name,
                    CountyId = p.Building.City.CountyId,
                    CountyName = p.Building.City.County.Name,
                    CityId = p.Building.CityId,
                    CityName = p.Building.City.Name,
                    p.CurrencyId,
                    CurrencyCode = p.Currency.Code
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
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremium * x.Currency.ExchangeRateToBase)
                })
                .OrderBy(r => r.CountryName)
                .ThenBy(r => r.CountyName)
                .ThenBy(r => r.CityName)
                .ThenBy(r => r.CurrencyCode)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PoliciesByBrokerReportRow>> PoliciesByBrokerAsync(PolicyReportQuery query, CancellationToken ct = default)
        {
            var q = BaseQuery(query);

            return await q
                .GroupBy(p => new
                {
                    p.BrokerId,
                    BrokerCode = p.Broker.BrokerCode,
                    BrokerName = p.Broker.Name,
                    p.CurrencyId,
                    CurrencyCode = p.Currency.Code
                })
                .Select(g => new PoliciesByBrokerReportRow
                {
                    BrokerId = g.Key.BrokerId,
                    BrokerCode = g.Key.BrokerCode,
                    BrokerName = g.Key.BrokerName,
                    CurrencyId = g.Key.CurrencyId,
                    CurrencyCode = g.Key.CurrencyCode,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremium * x.Currency.ExchangeRateToBase)
                })
                .OrderBy(r => r.BrokerCode)
                .ThenBy(r => r.CurrencyCode)
                .ToListAsync(ct);
        }
    }
