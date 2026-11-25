namespace Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;

public sealed class PoliciesByCityRowDTO : IPolicyReportRowDTO
{
        public int CityId { get; init; }
        public string CityName { get; init; } = null!;
        public int CountyId { get; init; }
        public string CountyName { get; init; } = null!;
        public int CountryId { get; init; }
        public string CountryName { get; init; } = null!;
        public int CurrencyId { get; init; }
        public string CurrencyCode { get; init; } = null!;
        public int PolicyCount { get; init; }
        public decimal TotalFinalPremium { get; init; }
        public decimal TotalFinalPremiumInBaseCurrency { get; init; }
    }
