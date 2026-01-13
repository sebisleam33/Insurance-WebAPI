using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Application.Abstractions;

    public interface IAppDbContext
    {
        DbSet<Client> Clients { get; }
        DbSet<Country> Countries{ get; }
        DbSet<Building> Buildings { get; }
        DbSet<County> Counties { get; }
        DbSet<City> Cities { get; }

        Task<int> SaveChangesAsync (CancellationToken cancellationToken = default);
    }
