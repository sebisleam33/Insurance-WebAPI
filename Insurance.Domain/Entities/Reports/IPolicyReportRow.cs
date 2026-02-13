namespace Insurance.Domain.Entities.Reports;

public interface IPolicyReportRow
    {
        int CurrencyId { get; }
        string CurrencyCode { get; }
        int PolicyCount { get; }
        decimal TotalFinalPremium { get; }
        decimal TotalFinalPremiumInBaseCurrency { get; }
    }
