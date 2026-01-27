using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities.Metadata;

public sealed class RiskFactorConfiguration
{
    public int Id { get; set; }

    public RiskFactorLevel Level { get; set; }

    public int? ReferenceId { get; set; }

    public int? BuildingType { get; set; }

    public decimal AdjustmentPercentage { get; set; } 

    public bool IsActive { get; set; } = true;
}
