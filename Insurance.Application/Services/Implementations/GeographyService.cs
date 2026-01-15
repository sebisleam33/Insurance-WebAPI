using AutoMapper;
using Insurance.Application.Common.DTOs.GeographyDTOs;
using Insurance.Application.Services.Interfaces;
using Insurance.Infrastructure.Repositories.Interfaces;

namespace Insurance.Application.Services.Implementations;

public sealed class GeographyService : IGeographyService
{
    private readonly IGeographyRepository _geo;
    private readonly IMapper _mapper;

    public GeographyService(IGeographyRepository geo, IMapper mapper)
    {
        _geo = geo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CountryResponseDTO>> GetCountriesAsync(CancellationToken ct = default)
        => (await _geo.GetCountriesAsync(ct)).Select(x => _mapper.Map<CountryResponseDTO>(x)).ToList();

    public async Task<IReadOnlyList<CountyResponseDTO>> GetCountiesByCountryAsync(int countryId, CancellationToken ct = default)
        => (await _geo.GetCountiesByCountryAsync(countryId, ct)).Select(x => _mapper.Map<CountyResponseDTO>(x)).ToList();

    public async Task<IReadOnlyList<CityResponseDTO>> GetCitiesByCountyAsync(int countyId, CancellationToken ct = default)
        => (await _geo.GetCitiesByCountyAsync(countyId, ct)).Select(x => _mapper.Map<CityResponseDTO>(x)).ToList();
}
