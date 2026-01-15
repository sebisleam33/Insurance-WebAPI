using Insurance.Domain.Entities;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

    public sealed class BuildingRepository : IBuildingRepository
    {
        private readonly AppDbContext _db;

        public BuildingRepository(AppDbContext db) => _db = db;

        public Task AddAsync(Building building, CancellationToken ct = default)
        {
            _db.Buildings.Add(building);
            return Task.CompletedTask;
        }

        public async Task<Building?> GetByIdAsync(Guid buildingId, bool trackChanges, CancellationToken ct = default)
        {
            var query = trackChanges ? _db.Buildings : _db.Buildings.AsNoTracking();
            return await query.FirstOrDefaultAsync(x => x.Id == buildingId, ct);
        }

        public async Task<Building?> GetByIdWithGeographyAsync(Guid buildingId, CancellationToken ct = default)
        {
            return await _db.Buildings
                .AsNoTracking()
                .Include(b => b.City)
                .ThenInclude(c => c.County)
                .ThenInclude(co => co.Country)
                .FirstOrDefaultAsync(b => b.Id == buildingId, ct);
        }

        public async Task<(IReadOnlyList<Building> Items, int TotalCount)> GetByClientAsync(
            Guid clientId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Buildings.AsNoTracking().Where(x => x.ClientId == clientId);

            var total =  await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Street)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<IReadOnlyList<Building>> GetAllByClientAsync(Guid clientId, CancellationToken ct = default)
             => await _db.Buildings.AsNoTracking()
                .Where(x => x.ClientId ==  clientId)
                .OrderBy(x => x.Street)
                .ToListAsync(ct);
    

        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
