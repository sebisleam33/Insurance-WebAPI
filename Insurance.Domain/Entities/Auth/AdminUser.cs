namespace Insurance.Domain.Entities.Auth;

public sealed class AdminUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;

        public byte[] PasswordHash { get; set; } = default!;
        public byte[] PasswordSalt { get; set; } = default!;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
