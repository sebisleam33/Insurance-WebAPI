namespace Insurance.Application.Clients.Models;

    public sealed class UpdateClientRequest
    {
        public string Name { get; init; } = null!;
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? PrimaryAddress { get; init; }
    }
