namespace Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;

public sealed class PoliciesByBrokerRowDTO : IPolicyReportRowDTO
{
        public Guid BrokerId { get; init; }
        public string BrokerCode { get; init; } = null!;
        public string BrokerName { get; init; } = null!;
        public int CurrencyId { get; init; }
        public string CurrencyCode { get; init; } = null!;
        public int PolicyCount { get; init; }
        public decimal TotalFinalPremium { get; init; }
        public decimal TotalFinalPremiumInBaseCurrency { get; init; }
    }
