using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities.Reports;

public sealed class PolicyReportQuery
    {
        public DateTime FromUtc { get; init; }
        public DateTime ToUtc { get; init; }
        public PolicyStatus? Status { get; init; }
        public int? CurrencyId { get; init; }
        public BuildingType? BuildingType { get; init; }
    }
