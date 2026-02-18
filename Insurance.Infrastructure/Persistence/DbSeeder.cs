using Insurance.Domain;
using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Auth;
using Insurance.Domain.Entities.Geography;
using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Persistence;

    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
        {
            if (!await db.Countries.AnyAsync(ct))
            {
                var romania = new Country { Name = "Romania" };

                var cluj = new County { Name = "Cluj", Country = romania };
                var bucuresti = new County { Name = "Bucuresti", Country = romania };

                var clujNapoca = new City { Name = "Cluj-Napoca", County = cluj };
                var turda = new City { Name = "Turda", County = cluj };
                var sector1 = new City { Name = "Bucuresti - Sector 1", County = bucuresti };
                var sector2 = new City { Name = "Bucuresti - Sector 2", County = bucuresti };

                db.Countries.Add(romania);
                db.Counties.AddRange(cluj, bucuresti);
                db.Cities.AddRange(clujNapoca, turda, sector1, sector2);

                await db.SaveChangesAsync(ct);
            }

            if (!await db.Currencies.AnyAsync(ct))
            {
                db.Currencies.AddRange(
                    new Currency { Code = "RON", Name = "Romanian Leu", ExchangeRateToBase = 1m, IsActive = true },
                    new Currency { Code = "EUR", Name = "Euro", ExchangeRateToBase = 4.95m, IsActive = true }
                );
                await db.SaveChangesAsync(ct);
            }

            if (!await db.FeeConfigurations.AnyAsync(ct))
            {
                var now = DateTime.UtcNow;

                db.FeeConfigurations.AddRange(
                    new FeeConfiguration
                    {
                        Name = "Standard admin fee",
                        Type = FeeType.AdminFee,
                        Percentage = 0.03m,
                        EffectiveFromUtc = now.AddYears(-1),
                        EffectiveToUtc = null,
                        IsActive = true
                    },
                    new FeeConfiguration
                    {
                        Name = "Standard broker commission",
                        Type = FeeType.BrokerCommission,
                        Percentage = 0.05m,
                        EffectiveFromUtc = now.AddYears(-1),
                        EffectiveToUtc = null,
                        IsActive = true
                    }
                );

                await db.SaveChangesAsync(ct);
            }

            if (!await db.RiskFactorConfigurations.AnyAsync(ct))
            {
                var clujNapoca = await db.Cities.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Cluj-Napoca", ct);
                if (clujNapoca != null)
                {
                    db.RiskFactorConfigurations.Add(new RiskFactorConfiguration
                    {
                        Level = RiskFactorLevel.City,
                        ReferenceId = clujNapoca.Id,
                        BuildingType = null,
                        AdjustmentPercentage = 0.02m,
                        IsActive = true
                    });

                    await db.SaveChangesAsync(ct);
                }
            }

            if (!await db.Brokers.AnyAsync(ct))
            {
                db.Brokers.Add(new Broker
                {
                    Id = Guid.NewGuid(),
                    BrokerCode = "BRK-001",
                    Name = "Default Broker",
                    Email = "broker@local",
                    Phone = "0000",
                    Status = BrokerStatus.Active,
                    CommissionPercentage = null
                });

                await db.SaveChangesAsync(ct);
            }

            if (!await db.AdminUsers.AnyAsync(ct))
            {
                var (hash, salt) = PasswordHasher.Hash("Admin123!"); // dev default

                db.AdminUsers.Add(new AdminUser
                {
                    Email = "admin@local",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                });

                await db.SaveChangesAsync(ct);
            }
        }
    }
