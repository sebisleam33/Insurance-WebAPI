using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;

public sealed class CreateFeeConfigurationRequestDTO
{
    public string Name { get; set; } = null!;
    public FeeType Type { get; set; }
    public decimal Percentage { get; set; }
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public bool IsActive { get; set; } = true;
}