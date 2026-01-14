using Insurance.Application.Abstractions;
using Insurance.Application.Geography.Models;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Application.Geography;

    public sealed class GeographyService : IGeographyService
    {
        private readonly IAppDbContext _db;

        public GeographyService(IAppDbContext db) => _db = db;

        public async Task<IReadOnlyList<CountryResponse>> GetCountriesAsync (CancellationToken ct = default) =>
    
            await _db.Countries.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CountryResponse { Id = c.Id, Name = c.Name })
                .ToListAsync(ct);
    
        public async Task<IReadOnlyList<CountyResponse>> GetCountiesByCountryAsync (int countryId, CancellationToken ct = default) =>
            await _db.Counties.AsNoTracking()
            .Where(x => x.CountryId == countryId)
            .OrderBy(c => c.Name)
            .Select (c => new CountyResponse {Id = c.Id, Name = c.Name, CountryId = c.CountryId})
            .ToListAsync(ct);

        public async Task<IReadOnlyList<CityResponse>> GetCitiesByCountyAsync (int countyId, CancellationToken ct = default) =>
            await _db.Cities.AsNoTracking()
            .Where(x => x.CountyId == countyId)
            .OrderBy(c => c.Name)
            .Select(c => new CityResponse { Id = c.Id, CountyId = c.CountyId, Name = c.Name})
            .ToListAsync(ct);
    }
