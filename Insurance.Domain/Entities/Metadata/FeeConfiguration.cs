using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities.Metadata;

public sealed class FeeConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public FeeType Type { get; set; }
    public decimal Percentage { get; set; } 

    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }

    public bool IsActive { get; set; } = true;
}
