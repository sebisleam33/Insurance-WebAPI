using Insurance.Application.Geography.Models;

namespace Insurance.Application.Geography;

    public interface IGeographyService
    {
        Task<IReadOnlyList<CountryResponse>> GetCountriesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CountyResponse>> GetCountiesByCountryAsync(int countryId, CancellationToken ct = default);
        Task<IReadOnlyList<CityResponse>> GetCitiesByCountyAsync(int countyId, CancellationToken ct = default);
    }
