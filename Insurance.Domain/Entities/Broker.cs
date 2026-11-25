using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities;

public sealed class Broker
{
    public Guid Id { get; set; }
    public string BrokerCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public BrokerStatus Status { get; set; } = BrokerStatus.Active;

    public decimal? CommissionPercentage { get; set; }

    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
