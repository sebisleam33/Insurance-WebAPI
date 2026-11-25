namespace Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;

public sealed class UpdateBrokerRequestDTO
    {
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public decimal? CommissionPercentage { get; set; }
    }
