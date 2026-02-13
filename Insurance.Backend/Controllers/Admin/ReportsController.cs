using Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
public sealed class ReportsController : ControllerBase
    {
        private readonly IPolicyReportsService _reports;
        public ReportsController(IPolicyReportsService reports) => _reports = reports;

        [HttpGet("policies-by-country")]
        public async Task<IActionResult> PoliciesByCountry(
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            [FromQuery] PolicyStatus? status,
            [FromQuery] int? currencyId,
            [FromQuery] BuildingType? buildingType,
            CancellationToken ct = default)
            => Ok(await _reports.PoliciesByCountryAsync(new PolicyReportQueryDTO
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Status = status,
                CurrencyId = currencyId,
                BuildingType = buildingType
            }, ct));

        [HttpGet("policies-by-county")]
        public async Task<IActionResult> PoliciesByCounty(
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            [FromQuery] PolicyStatus? status,
            [FromQuery] int? currencyId,
            [FromQuery] BuildingType? buildingType,
            CancellationToken ct = default)
            => Ok(await _reports.PoliciesByCountyAsync(new PolicyReportQueryDTO
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Status = status,
                CurrencyId = currencyId,
                BuildingType = buildingType
            }, ct));

        [HttpGet("policies-by-city")]
        public async Task<IActionResult> PoliciesByCity(
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            [FromQuery] PolicyStatus? status,
            [FromQuery] int? currencyId,
            [FromQuery] BuildingType? buildingType,
            CancellationToken ct = default)
            => Ok(await _reports.PoliciesByCityAsync(new PolicyReportQueryDTO
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Status = status,
                CurrencyId = currencyId,
                BuildingType = buildingType
            }, ct));

        [HttpGet("policies-by-broker")]
        public async Task<IActionResult> PoliciesByBroker(
            [FromQuery] DateTime fromUtc,
            [FromQuery] DateTime toUtc,
            [FromQuery] PolicyStatus? status,
            [FromQuery] int? currencyId,
            [FromQuery] BuildingType? buildingType,
            CancellationToken ct = default)
            => Ok(await _reports.PoliciesByBrokerAsync(new PolicyReportQueryDTO
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Status = status,
                CurrencyId = currencyId,
                BuildingType = buildingType
            }, ct));
    }
