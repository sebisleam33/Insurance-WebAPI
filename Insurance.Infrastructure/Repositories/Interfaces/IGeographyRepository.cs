using Insurance.Domain.Entities.Geography;

namespace Insurance.Infrastructure.Repositories.Interfaces;

public interface IGeographyRepository
    {
        Task<bool> CountryExistsAsync(int countryId, CancellationToken ct = default);
        Task<bool> CountyExistsAsync(int countyId, CancellationToken ct = default);
        Task<bool> CityExistsAsync(int cityId, CancellationToken ct = default);

        Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<County>> GetCountiesByCountryAsync(int countryId, CancellationToken ct = default);
        Task<IReadOnlyList<City>> GetCitiesByCountyAsync(int countyId, CancellationToken ct = default);
    }
