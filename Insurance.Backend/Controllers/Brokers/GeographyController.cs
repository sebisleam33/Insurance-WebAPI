using Insurance.Application.Geography;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers.Brokers;

[ApiController]
[Route("api/brokers")]
public class GeographyController : ControllerBase
{
    private readonly IGeographyService _service;

    public GeographyController(IGeographyService service) => _service = service;

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries(CancellationToken ct = default)
        => Ok(await _service.GetCountriesAsync(ct));

    [HttpGet("countries/{countryId:int}/counties")]
    public async Task<IActionResult> GetCounties(int countryId, CancellationToken ct = default)
        => Ok(await _service.GetCountiesByCountryAsync(countryId, ct));

    [HttpGet("counties/{countyId:int}/cities")]
    public async Task<IActionResult> GetCities(int countyId, CancellationToken ct = default)
        => Ok(await _service.GetCitiesByCountyAsync(countyId, ct));
}
