namespace Insurance.Application.Common.DTOs.PolicyDTOs;

public sealed class CreatePolicyRequestDTO
    {
        public Guid ClientId { get; set; }
        public Guid BuildingId { get; set; }
        public Guid BrokerId { get; set; }
        public int CurrencyId { get; set; }

        public decimal BasePremium { get; set; }

        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
    }
