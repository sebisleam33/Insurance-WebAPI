namespace Insurance.Domain.Entities;

    public sealed class ClientIdentifierChange
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public string OldIdentificationNumber { get; set; } = null!;
        public string NewIdentificationNumber { get; set;} = null!;

        public DateTime ChangedAtUtc { get; set; }
        public string? Reason { get; set; }
    }
