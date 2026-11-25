namespace Insurance.Domain.Entities.Reports;

public sealed class PoliciesByBrokerReportRow : IPolicyReportRow
    {
        public Guid BrokerId { get; set; }
        public string BrokerCode { get; set; } = null!;
        public string BrokerName { get; set; } = null!;
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public int PolicyCount { get; set; }
        public decimal TotalFinalPremium { get; set; }
        public decimal TotalFinalPremiumInBaseCurrency { get; set; }
    }
