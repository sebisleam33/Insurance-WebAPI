using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;

public sealed class RiskFactorConfigurationResponseDTO
    {
        public int Id { get; set; }
        public RiskFactorLevel Level { get; set; }
        public int? ReferenceId { get; set; }
        public BuildingType? BuildingType { get; set; }
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; }
    }
