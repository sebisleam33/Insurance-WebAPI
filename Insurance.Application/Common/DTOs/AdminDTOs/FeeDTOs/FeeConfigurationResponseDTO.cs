using Insurance.Domain.Enums;

public sealed class FeeConfigurationResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public FeeType Type { get; set; }
        public decimal Percentage { get; set; }
        public DateTime EffectiveFromUtc { get; set; }
        public DateTime? EffectiveToUtc { get; set; }
        public bool IsActive { get; set; }
    }