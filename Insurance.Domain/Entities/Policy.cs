using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities;

public sealed class Policy
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = null!;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public Guid BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    public Guid BrokerId { get; set; }
    public Broker Broker { get; set; } = null!;

    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;

    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }

    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
}
