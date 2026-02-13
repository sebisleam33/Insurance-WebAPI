using AutoMapper;
using Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities.Reports;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class PolicyReportsService : IPolicyReportsService
    {
        private readonly IPolicyReportsRepository _reports;
        private readonly IMapper _mapper;
        private readonly ILogger<PolicyReportsService> _logger;

        public PolicyReportsService(
            IPolicyReportsRepository reports,
            IMapper mapper,
            ILogger<PolicyReportsService> logger)
        {
            _reports = reports;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IReadOnlyList<PoliciesByCountryRowDTO>> PoliciesByCountryAsync(PolicyReportQueryDTO q, CancellationToken ct = default)
        {
            Validate(q);

            var domainQuery = _mapper.Map<PolicyReportQuery>(q);
            var rows = await _reports.PoliciesByCountryAsync(domainQuery, ct);

            _logger.LogInformation("Generated policies-by-country report from {From} to {To}", q.FromUtc, q.ToUtc);
            return rows.Select(_mapper.Map<PoliciesByCountryRowDTO>).ToList();
        }

        public async Task<IReadOnlyList<PoliciesByCountyRowDTO>> PoliciesByCountyAsync(PolicyReportQueryDTO q, CancellationToken ct = default)
        {
            Validate(q);

            var domainQuery = _mapper.Map<PolicyReportQuery>(q);
            var rows = await _reports.PoliciesByCountyAsync(domainQuery, ct);

            _logger.LogInformation("Generated policies-by-county report from {From} to {To}", q.FromUtc, q.ToUtc);
            return rows.Select(_mapper.Map<PoliciesByCountyRowDTO>).ToList();
        }

        public async Task<IReadOnlyList<PoliciesByCityRowDTO>> PoliciesByCityAsync(PolicyReportQueryDTO q, CancellationToken ct = default)
        {
            Validate(q);

            var domainQuery = _mapper.Map<PolicyReportQuery>(q);
            var rows = await _reports.PoliciesByCityAsync(domainQuery, ct);

            _logger.LogInformation("Generated policies-by-city report from {From} to {To}", q.FromUtc, q.ToUtc);
            return rows.Select(_mapper.Map<PoliciesByCityRowDTO>).ToList();
        }

        public async Task<IReadOnlyList<PoliciesByBrokerRowDTO>> PoliciesByBrokerAsync(PolicyReportQueryDTO q, CancellationToken ct = default)
        {
            Validate(q);

            var domainQuery = _mapper.Map<PolicyReportQuery>(q);
            var rows = await _reports.PoliciesByBrokerAsync(domainQuery, ct);

            _logger.LogInformation("Generated policies-by-broker report from {From} to {To}", q.FromUtc, q.ToUtc);
            return rows.Select(_mapper.Map<PoliciesByBrokerRowDTO>).ToList();
        }

        private static void Validate(PolicyReportQueryDTO q)
        {
            ValidationHelpers.RequireAfter(q.ToUtc, q.FromUtc, nameof(q.ToUtc), nameof(q.FromUtc));
        }
    }
