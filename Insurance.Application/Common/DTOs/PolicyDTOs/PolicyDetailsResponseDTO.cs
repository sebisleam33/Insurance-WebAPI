using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.PolicyDTOs;

    public sealed class PolicyDetailsResponseDTO
    {
        public Guid Id { get; init; }
        public string PolicyNumber { get; init; } = null!;
        public PolicyStatus Status { get; init; }

        public Guid ClientId { get; init; }
        public string ClientName { get; init; } = null!;

        public Guid BrokerId { get; init; }
        public string BrokerName { get; init;} = null!;

        public Guid BuildingId { get; init; }
        public string BuildingStreet { get; init; } = null!;
        public string BuildingNumber { get; init; } = null!;

        public int CityId { get; init; }
        public string CityName { get; init; } = null!;
        public int CountyId { get; init; }
        public string CountyName { get; init; } = null!;
        public int CountryId { get; init; }
        public string CountryName { get; init; } = null!;

        public int CurrencyId { get; init; }
        public string CurrencyCode { get; init; } = null!;

        public decimal BasePremium { get; init; }
        public decimal FinalPremium { get; init; }

        public DateTime StartDateUtc { get; init; }
        public DateTime EndDateUtc { get; init; }

        public DateTime CreatedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }

        public DateTime? CancelledAtUtc { get; init; }
        public string? CancellationReason { get; init; }
    }
