using Insurance.Application.Buildings;
using Insurance.Application.Buildings.Models;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers.Brokers;

[ApiController]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _service;

    public BuildingsController(IBuildingService service) => _service = service;

    [HttpGet("api/brokers/clients/{clientId:guid}/buildings")]
    public async Task<IActionResult> GetByClient(Guid clientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _service.GetByClientAsync(clientId, page, pageSize, ct));

    [HttpGet("api/brokers/buildings/{buildingId:guid}")]
    public async Task<IActionResult> GetById(Guid buildingId, CancellationToken ct = default)
        => Ok(await _service.GetByIdAsync(buildingId, ct));

    [HttpPost("api/brokers/clients/{clientId:guid}/buildings")]
    public async Task<IActionResult> Create(Guid clientId, [FromBody] CreateBuildingRequest request, CancellationToken ct = default)
    {
        var id = await _service.CreateAsync(clientId, request, ct);
        return CreatedAtAction(nameof(GetById), new { buildingId = id }, new { id });
    }

    [HttpPut("api/brokers/buildings/{buildingId:guid}")]
    public async Task<IActionResult> Update(Guid buildingId, [FromBody] UpdateBuildingRequest request, CancellationToken ct = default)
    {
        await _service.UpdateAsync(buildingId, request, ct);
        return NoContent();
    }
}
