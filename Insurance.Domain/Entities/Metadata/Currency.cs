namespace Insurance.Domain.Entities.Metadata;

public sealed class Currency
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal ExchangeRateToBase { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
