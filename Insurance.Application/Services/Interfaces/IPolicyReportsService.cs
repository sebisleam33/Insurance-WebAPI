using Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;

namespace Insurance.Application.Services.Interfaces;

public interface IPolicyReportsService
    {
        Task<IReadOnlyList<PoliciesByCountryRowDTO>> PoliciesByCountryAsync(PolicyReportQueryDTO query, CancellationToken ct = default);
        Task<IReadOnlyList<PoliciesByCountyRowDTO>> PoliciesByCountyAsync(PolicyReportQueryDTO query, CancellationToken ct = default);
        Task<IReadOnlyList<PoliciesByCityRowDTO>> PoliciesByCityAsync(PolicyReportQueryDTO query, CancellationToken ct = default);
        Task<IReadOnlyList<PoliciesByBrokerRowDTO>> PoliciesByBrokerAsync(PolicyReportQueryDTO query, CancellationToken ct = default);
    }
