using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.PolicyDTOs;

public sealed class PolicyListItemResponseDTO
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = null!;
        public PolicyStatus Status { get; set; }

        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = null!;

        public Guid BrokerId { get; set; }
        public string BrokerName { get; set; } = null!;

        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = null!;

        public decimal BasePremium { get; set; }
        public decimal FinalPremium { get; set; }

        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
    }
