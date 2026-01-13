using Insurance.Domain.Enums;

namespace Insurance.Application.Clients.Models;

    public sealed class ClientListItemResponse
    {
        public Guid Id { get; init; }
        public ClientType Type { get; init; }
        public string Name { get; init; } = null!;
        public string IdentificationNumber { get; init; } = null!;
    }
