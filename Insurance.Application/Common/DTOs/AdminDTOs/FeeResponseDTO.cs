using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.AdminDTOs;

    public sealed class FeeResponseDTO
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public FeeType Type { get; init; }
        public decimal Percentage { get; init; }
        public DateTime EffectiveFromUtc { get; init; }
        public DateTime? EffectiveToUtc { get; init; }
        public bool IsActive { get; init; }
    }
