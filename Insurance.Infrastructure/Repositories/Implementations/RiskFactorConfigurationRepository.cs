using Insurance.Domain.Entities.Metadata;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

public sealed class RiskFactorConfigurationRepository : IRiskFactorConfigurationRepository
    {
        private readonly AppDbContext _db;
        public RiskFactorConfigurationRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<RiskFactorConfiguration>> GetActiveAsync(CancellationToken ct = default)
            => await _db.RiskFactorConfigurations
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(ct);

        public async Task<(IReadOnlyList<RiskFactorConfiguration> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var q = _db.RiskFactorConfigurations.AsNoTracking().OrderByDescending(x => x.IsActive);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return (items, total);
        }

        public async Task<RiskFactorConfiguration?> GetByIdAsync(int id, bool trackChanges, CancellationToken ct = default)
        {
            var q = trackChanges ? _db.RiskFactorConfigurations : _db.RiskFactorConfigurations.AsNoTracking();
            return await q.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task AddAsync(RiskFactorConfiguration risk, CancellationToken ct = default)
        {
            _db.RiskFactorConfigurations.Add(risk);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
