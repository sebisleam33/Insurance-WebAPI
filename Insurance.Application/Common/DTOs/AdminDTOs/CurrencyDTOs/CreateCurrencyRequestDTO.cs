namespace Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;

    public sealed class CreateCurrencyRequestDTO
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public decimal ExchangeRateToBase {  get; init; }
        public bool IsActive { get; init; }
    }
