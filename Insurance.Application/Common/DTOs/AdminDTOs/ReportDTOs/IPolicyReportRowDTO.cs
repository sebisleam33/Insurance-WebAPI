namespace Insurance.Application.Common.DTOs.AdminDTOs.ReportDTOs;

public interface IPolicyReportRowDTO
    {
        int CurrencyId { get; init; }
        string CurrencyCode { get; init; }
        int PolicyCount { get; init; }
        decimal TotalFinalPremium { get; init; }
        decimal TotalFinalPremiumInBaseCurrency { get; init; }
    }
