using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs;

    public sealed class BrokerResponseDTO
    {
        public Guid Id { get; init; }
        public string BrokerCode { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public BrokerStatus Status { get; init; }
        public decimal? ComissionPercentage { get; init; }
    }
