using Insurance.Application.Common.DTOs.ClientDTOs;
using Insurance.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers.Brokers;

[ApiController]
[Route("api/brokers/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _service;

    public ClientsController(IClientService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] string? identifier, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _service.SearchAsync(name, identifier, page, pageSize, ct));

    [HttpGet("{clientId:guid}")]
    public async Task<IActionResult> GetById(Guid clientId, CancellationToken ct = default)
        => Ok(await _service.GetByIdAsync(clientId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequestDTO request, CancellationToken ct = default)
    {
        var id = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { clientId = id }, new { id });
    }

    [HttpPut("{clientId:guid}")]
    public async Task<IActionResult> Update(Guid clientId, [FromBody] UpdateClientRequestDTO request, CancellationToken ct = default)
    {
        await _service.UpdateAsync(clientId, request, ct);
        return NoContent();
    }
}
