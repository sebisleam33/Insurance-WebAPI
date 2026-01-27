using Insurance.Domain.Entities.Metadata;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

    public sealed class FeeConfigurationRepository : IFeeConfigurationRepository
    {
        private readonly AppDbContext _db;
        public FeeConfigurationRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<FeeConfiguration>> GetAllAsync(CancellationToken ct = default)
            => await _db.FeeConfigurations.AsNoTracking().OrderByDescending(x => x.EffectiveFromUtc).ToListAsync(ct);

        public async Task<IReadOnlyList<FeeConfiguration>> GetActiveForNowAsync(DateTime nowUtc, CancellationToken ct = default)
        {
            return await _db.FeeConfigurations.AsNoTracking()
                .Where(x => x.IsActive
                            && x.EffectiveFromUtc <= nowUtc
                            && (x.EffectiveToUtc == null || x.EffectiveToUtc >= nowUtc))
                .ToListAsync(ct);
        }

        public async Task<FeeConfiguration?> GetByIdAsync(int id, bool trackChanges, CancellationToken ct = default)
        {
            var q = trackChanges ? _db.FeeConfigurations : _db.FeeConfigurations.AsNoTracking();
            return await q.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task AddAsync(FeeConfiguration fee, CancellationToken ct = default)
        {
            _db.FeeConfigurations.Add(fee);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
