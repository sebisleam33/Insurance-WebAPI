using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;

    public sealed class CreateBrokerRequestDTO
    {
        public string BrokerCode { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public BrokerStatus Status { get; init; } = BrokerStatus.Active;
        public decimal? CommissionPercentage { get; init; }
    }
