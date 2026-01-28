using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.PolicyDTOs;

    public sealed class PolicyListItemResponseDTO
    {
        public Guid Id { get; init; }
        public string PolicyNumber { get; init; } = null!;
        public PolicyStatus Status { get; init; }

        public Guid ClientId { get; init; }
        public string ClientName { get; init; } = null!;
        public Guid BrokerId { get; init; }
        public string BrokerName { get; init;} = null!;

        public decimal BasePremium { get; init; }
        public decimal FinalPremium { get; init; }
        public string CurrencyCode { get; init; } = null!;

        public DateTime StartAtUtc { get; init; }
        public DateTime EndAtUtc { get; init; }
    }
