using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;

public sealed class PolicyReportQueryDTO
    {
        public DateTime FromUtc { get; init; }
        public DateTime ToUtc { get; init; }
        public PolicyStatus? Status { get; init; }
        public int? CurrencyId { get; init; }
        public BuildingType? BuildingType { get; init; }
    }
