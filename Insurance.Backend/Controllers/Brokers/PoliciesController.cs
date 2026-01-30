using Insurance.Application.Common.DTOs.PolicyDTOs;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers.Brokers;

[ApiController]
[Route("api/brokers/policies")]
public sealed class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _service;
        public PoliciesController(IPolicyService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] Guid? clientId,
            [FromQuery] Guid? brokerId,
            [FromQuery] PolicyStatus? status,
            [FromQuery] DateTime? startDateUtc,
            [FromQuery] DateTime? endDateUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
            => Ok(await _service.SearchAsync(clientId, brokerId, status, startDateUtc, endDateUtc, page, pageSize, ct));

        [HttpGet("{policyId:guid}")]
        public async Task<IActionResult> GetById(Guid policyId, CancellationToken ct = default)
            => Ok(await _service.GetByIdAsync(policyId, ct));

        [HttpPost]
        public async Task<IActionResult> CreateDraft([FromBody] CreatePolicyRequestDTO request, CancellationToken ct = default)
        {
            var id = await _service.CreateDraftAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { policyId = id }, new { id });
        }

        [HttpPost("{policyId:guid}/activate")]
        public async Task<IActionResult> Activate(Guid policyId, CancellationToken ct = default)
        {
            await _service.ActivateAsync(policyId, ct);
            return NoContent();
        }

        [HttpPost("{policyId:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid policyId, [FromBody] CancelPolicyRequestDTO request, CancellationToken ct = default)
        {
            await _service.CancelAsync(policyId, request, ct);
            return NoContent();
        }
    }
