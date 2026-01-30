namespace Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;

public sealed class UpdateCurrencyRequestDTO
    {
        public string Name { get; set; } = null!;
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; }
    }
