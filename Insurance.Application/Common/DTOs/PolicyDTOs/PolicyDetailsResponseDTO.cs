using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.PolicyDTOs;

public sealed class PolicyDetailsResponseDTO
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = null!;
        public PolicyStatus Status { get; set; }

        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = null!;

        public Guid BuildingId { get; set; }
        public string BuildingStreet { get; set; } = null!;
        public string BuildingNumber { get; set; } = null!;

        public Guid CityId { get; set; }
        public string CityName { get; set; } = null!;
        public Guid CountyId { get; set; }
        public string CountyName { get; set; } = null!;
        public Guid CountryId { get; set; }
        public string CountryName { get; set; } = null!;

        public Guid BrokerId { get; set; }
        public string BrokerName { get; set; } = null!;

        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = null!;

        public decimal BasePremium { get; set; }
        public decimal FinalPremium { get; set; }

        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public DateTime? CancelledAtUtc { get; set; }
        public string? CancellationReason { get; set; }
    }
