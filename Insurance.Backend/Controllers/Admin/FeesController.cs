using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;
using Insurance.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/fees")]
public sealed class FeesController : ControllerBase
    {
        private readonly IFeeConfigurationAdminService _service;
        public FeesController(IFeeConfigurationAdminService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await _service.ListAsync(page, pageSize, ct));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
            => Ok(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeeConfigurationRequestDTO request, CancellationToken ct = default)
        {
            var id = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFeeConfigurationRequestDTO request, CancellationToken ct = default)
        {
            await _service.UpdateAsync(id, request, ct);
            return NoContent();
        }
    }
