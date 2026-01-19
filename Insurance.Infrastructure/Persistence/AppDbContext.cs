using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Insurance.Infrastructure.Persistence;

    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Building> Buildings => Set<Building>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<County> Counties => Set<County>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<ClientIdentifierChange> ClientIdentifierChanges => Set<ClientIdentifierChange>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired();
                b.Property(x => x.IdentificationNumber).IsRequired();
                b.HasIndex(x => x.IdentificationNumber).IsUnique();
            });

            modelBuilder.Entity<Building>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Street).IsRequired();
                b.Property(x => x.Number).IsRequired();

                b.HasOne(x => x.Client)
                    .WithMany(c => c.Buildings)
                    .HasForeignKey(x => x.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.City)
                    .WithMany(c => c.Buildings)
                    .HasForeignKey(x => x.CityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Country>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired();
            });

            modelBuilder.Entity<County>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired();

                b.HasOne(x => x.Country)
                    .WithMany(c => c.Counties)
                    .HasForeignKey(x => x.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<City>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired();

                b.HasOne(x => x.County)
                    .WithMany(c => c.Cities)
                    .HasForeignKey(x => x.CountyId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => new { x.CountyId, x.Name }).IsUnique();
            });

            modelBuilder.Entity<ClientIdentifierChange>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.OldIdentificationNumber).IsRequired();
                b.Property(x => x.NewIdentificationNumber).IsRequired();
                b.Property(x => x.ChangedAtUtc).IsRequired();

                b.HasOne(x => x.Client)
                    .WithMany(c => c.IdentifierChanges) 
                    .HasForeignKey(x => x.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => x.ClientId);
            });
    }
}
