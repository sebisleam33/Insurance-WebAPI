using Insurance.Domain.Entities.Reports;

namespace Insurance.Infrastructure.Repositories.Interfaces;

public interface IPolicyReportsRepository
    {
        Task<IReadOnlyList<PoliciesByCountryReportRow>> PoliciesByCountryAsync(PolicyReportQuery query, CancellationToken ct = default);
        Task<IReadOnlyList<PoliciesByCountyReportRow>> PoliciesByCountyAsync(PolicyReportQuery query, CancellationToken ct = default);
        Task<IReadOnlyList<PoliciesByCityReportRow>> PoliciesByCityAsync(PolicyReportQuery query, CancellationToken ct = default);
        Task<IReadOnlyList<PoliciesByBrokerReportRow>> PoliciesByBrokerAsync(PolicyReportQuery query, CancellationToken ct = default);
    }
