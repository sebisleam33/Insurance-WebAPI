using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Domain.Enums;

namespace Insurance.Application.Common.DTOs.ClientDTOs;

    public sealed class ClientDetailsResponseDTO
    {
        public Guid Id { get; init; }
        public ClientType Type { get; init; }
        public string Name { get; init; } = null!;
        public string IdentificationNumber { get; init; } = null!;
        public string? Email { get; init; }
        public string? Phone { get; init; } = null!;
        public string? PrimaryAddress { get; init; } = null!;

        public IReadOnlyList<BuildingListItemResponseDTO> Buildings { get; init; } = Array.Empty<BuildingListItemResponseDTO>();
    }
