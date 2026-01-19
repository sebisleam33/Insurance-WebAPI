using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities;
    public class Client
    {
        public Guid Id { get; set; }
        public ClientType Type { get; set; }
        public string Name { get; set; } = null!;
        public string IdentificationNumber { get; set; } = null!; // CNP or CUI
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PrimaryAddress { get; set; }

        public ICollection<Building> Buildings { get; set; } = new List<Building>();
        public ICollection<ClientIdentifierChange> IdentifierChanges { get; set; } = new List<ClientIdentifierChange>();

    }

