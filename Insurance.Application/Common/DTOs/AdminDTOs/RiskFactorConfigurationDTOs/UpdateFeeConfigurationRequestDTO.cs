namespace Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;

public sealed class UpdateFeeConfigurationRequestDTO
    {
        public decimal Percentage { get; set; }
        public DateTime EffectiveFromUtc { get; set; }
        public DateTime? EffectiveToUtc { get; set; }
        public bool IsActive { get; set; }
    }
