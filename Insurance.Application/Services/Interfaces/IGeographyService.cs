using Insurance.Application.Common.DTOs.GeographyDTOs;

namespace Insurance.Application.Services.Interfaces;

    public interface IGeographyService
    {
        Task<IReadOnlyList<CountryResponseDTO>> GetCountriesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CountyResponseDTO>> GetCountiesByCountryAsync(int countryId, CancellationToken ct = default);
        Task<IReadOnlyList<CityResponseDTO>> GetCitiesByCountyAsync(int countyId, CancellationToken ct = default);
    }
