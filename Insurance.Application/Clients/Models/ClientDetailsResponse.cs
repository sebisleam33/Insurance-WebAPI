using Insurance.Application.Buildings.Models;
using Insurance.Domain.Enums;

namespace Insurance.Application.Clients.Models;

    public sealed class ClientDetailsResponse
    {
        public Guid Id { get; init; }
        public ClientType Type { get; init; }
        public string Name { get; init; } = null!;
        public string IdentificationNumber { get; init; } = null!;
        public string? Email { get; init; }
        public string? Phone { get; init; } = null!;
        public string? PrimaryAddress { get; init; } = null!;

        public IReadOnlyList<BuildingListItemResponse> Buildings { get; init; } = Array.Empty<BuildingListItemResponse>();
    }
