namespace Insurance.Application.Common.DTOs.PolicyDTOs;

    public sealed class CreatePolicyRequestDTO
    {
        public Guid ClientId { get; init; }
        public Guid BuildingId { get; init; }
        public Guid BrokerId { get; init; }
        public int CurrencyId { get; init; }

        public decimal BasePremium { get; init; }
        public DateTime StartDateUtc { get; init; }
        public DateTime EndDateUtc { get; init; }
    }
