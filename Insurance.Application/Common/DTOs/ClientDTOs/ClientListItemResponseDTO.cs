using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.ClientDTOs;

public sealed class ClientListItemResponseDTO
    {
        public Guid Id { get; init; }
        public ClientType Type { get; init; }
        public string Name { get; init; } = null!;
        public string IdentificationNumber { get; init; } = null!;
    }
