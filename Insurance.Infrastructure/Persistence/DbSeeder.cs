using Insurance.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Persistence;

    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
        {
            if (await db.Countries.AnyAsync(ct)) 
            { 
                return; 
            }

            var romania = new Country { Name = "Romania" };

            var cluj = new County { Name = "Cluj", Country = romania };
            var bucuresti = new County { Name = "Bucuresti", Country = romania };

            var clujNapoca = new City { Name = "Cluj-Napoca", County = cluj};
            var turda = new City { Name = "Turda", County = cluj };
            var sector1 = new City { Name = "Bucuresti - Sector 1", County = bucuresti };
            var sector2 = new City { Name = "Bucuresti - Sector 2", County = bucuresti };

            db.Countries.Add(romania);
            db.Counties.AddRange(cluj, bucuresti);
            db.Cities.AddRange(clujNapoca, turda, sector1, sector2);

            await db.SaveChangesAsync(ct);
        }
    }
