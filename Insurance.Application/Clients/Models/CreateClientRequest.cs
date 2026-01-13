using Insurance.Domain.Enums;

namespace Insurance.Application.Clients.Models;

    public sealed class CreateClientRequest
    {
        public ClientType  Type { get; init; }
        public string Name { get; init; } = null!;
        public string IdentificationNumber { get; init; } = null!;
        public string? Email {  get; init; }
        public string? Phone { get; init; }
        public string? PrimaryAddress { get; init; }
    }
