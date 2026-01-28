using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs;

    public sealed class RiskFactorResponseDTO
    {
        public int Id { get; init; }
        public RiskFactorLevel Level { get; init; }
        public int? ReferenceId { get; init; }
        public BuildingType? BuildingType { get; init; }
        public decimal AdjustmentPercentage { get; init; }
        public bool IsActive { get; init; }
    }
