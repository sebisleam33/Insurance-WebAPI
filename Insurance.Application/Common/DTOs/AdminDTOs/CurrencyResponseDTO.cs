namespace Insurance.Application.Common.DTOs.AdminDTOs;

    public sealed class CurrencyResponseDTO
    {
        public int Id { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public decimal ExchangeRateToBase { get; init; }
        public bool IsActive { get; init; }
    }
