namespace Insurance.Application.Common.DTOs.ClientDTOs;

public sealed class UpdateClientRequestDTO
    {
        public string Name { get; init; } = null!;
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? PrimaryAddress { get; init; }
    }
