using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;

public sealed class BrokerResponseDTO
    {
        public Guid Id { get; set; }
        public string BrokerCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public BrokerStatus Status { get; set; }
        public decimal? CommissionPercentage { get; set; }
    }
