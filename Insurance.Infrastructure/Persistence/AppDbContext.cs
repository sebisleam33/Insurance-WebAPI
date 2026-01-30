using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;
using Insurance.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Persistence;

    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Building> Buildings => Set<Building>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<County> Counties => Set<County>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Broker> Brokers => Set<Broker>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<FeeConfiguration> FeeConfigurations => Set<FeeConfiguration>();
        public DbSet<RiskFactorConfiguration> RiskFactorConfigurations => Set<RiskFactorConfiguration>();

        public DbSet<ClientIdentifierChange> ClientIdentifierChanges => Set<ClientIdentifierChange>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired();
                b.Property(x => x.IdentificationNumber).IsRequired();
                b.HasIndex(x => x.IdentificationNumber).IsUnique();

                b.HasMany(x => x.IdentifierChanges)
                 .WithOne(x => x.Client)
                 .HasForeignKey(x => x.ClientId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClientIdentifierChange>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.OldIdentificationNumber).IsRequired();
                b.Property(x => x.NewIdentificationNumber).IsRequired();
                b.Property(x => x.ChangedAtUtc).IsRequired();
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

            modelBuilder.Entity<Broker>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.BrokerCode).IsRequired();
                b.Property(x => x.Name).IsRequired();
                b.HasIndex(x => x.BrokerCode).IsUnique();
            });

            modelBuilder.Entity<Currency>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Code).IsRequired();
                b.Property(x => x.Name).IsRequired();
                b.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<FeeConfiguration>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired();
                b.Property(x => x.Percentage).IsRequired();
                b.Property(x => x.EffectiveFromUtc).IsRequired();
            });

            modelBuilder.Entity<RiskFactorConfiguration>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.AdjustmentPercentage).IsRequired();
            });

            modelBuilder.Entity<Policy>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.PolicyNumber).IsRequired();
                b.HasIndex(x => x.PolicyNumber).IsUnique();

                b.Property(x => x.BasePremium).IsRequired();
                b.Property(x => x.FinalPremium).IsRequired();
                b.Property(x => x.CreatedAtUtc).IsRequired();
                b.Property(x => x.UpdatedAtUtc).IsRequired();

                b.HasOne(x => x.Client)
                    .WithMany(c => c.Policies)
                    .HasForeignKey(x => x.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Building)
                    .WithMany(bu => bu.Policies)
                    .HasForeignKey(x => x.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Broker)
                    .WithMany(br => br.Policies)
                    .HasForeignKey(x => x.BrokerId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Currency)
                    .WithMany(cu => cu.Policies)
                    .HasForeignKey(x => x.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
