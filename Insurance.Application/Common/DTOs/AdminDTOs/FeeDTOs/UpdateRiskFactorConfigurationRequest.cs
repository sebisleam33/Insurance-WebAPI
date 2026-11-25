namespace Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;

public sealed class UpdateRiskFactorConfigurationRequestDTO
    {
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; }
    }
