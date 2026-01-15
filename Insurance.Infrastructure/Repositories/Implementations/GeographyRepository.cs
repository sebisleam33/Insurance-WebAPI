using Insurance.Domain.Entities.Geography;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations
{
    public sealed class GeographyRepository : IGeographyRepository
    {
        private readonly AppDbContext _db;
        public GeographyRepository(AppDbContext db) => _db = db;

        public Task<bool> CityExistsAsync(int cityId, CancellationToken ct = default)
            => _db.Cities.AnyAsync(x => x.Id == cityId);

        public async Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken ct = default)
            => await _db.Countries.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

        public async Task<IReadOnlyList<County>> GetCountiesByCountryAsync(int countryId, CancellationToken ct = default)
            => await _db.Counties.AsNoTracking()
            .Where(x => x.CountryId ==  countryId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        public async Task<IReadOnlyList<City>> GetCitiesByCountyAsync(int countyId, CancellationToken ct = default)
            => await _db.Cities.AsNoTracking()
            .Where(x => x.CountyId == countyId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }
}
