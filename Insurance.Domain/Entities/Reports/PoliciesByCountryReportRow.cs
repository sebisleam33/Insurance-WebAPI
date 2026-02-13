namespace Insurance.Domain.Entities.Reports;

public sealed class PoliciesByCountryReportRow : IPolicyReportRow
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; } = null!;
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public int PolicyCount { get; set; }
        public decimal TotalFinalPremium { get; set; }
        public decimal TotalFinalPremiumInBaseCurrency { get; set; }
    }
