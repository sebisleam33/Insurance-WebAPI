using Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;
using Insurance.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/currencies")]
public sealed class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyAdminService _service;
        public CurrenciesController(ICurrencyAdminService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
            => Ok(await _service.GetAllAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
            => Ok(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCurrencyRequestDTO request, CancellationToken ct = default)
        {
            var id = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCurrencyRequestDTO request, CancellationToken ct = default)
        {
            await _service.UpdateAsync(id, request, ct);
            return NoContent();
        }
    }
