using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

    public sealed class CurrencyRepository : ICurrencyRepository
    {
        private readonly AppDbContext _db;
        public CurrencyRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default)
            => await _db.Currencies.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

        public async Task<Currency?> GetByIdAsync(int id, bool trackChanges, CancellationToken ct = default)
        {
            var q = trackChanges ? _db.Currencies : _db.Currencies.AsNoTracking();
            return await q.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
            => _db.Currencies.AnyAsync(x => x.Code == code, ct);

        public Task AddAsync(Currency currency, CancellationToken ct = default)
        {
            _db.Currencies.Add(currency);
            return Task.CompletedTask;
        }

        public Task<bool> IsUsedInActivePoliciesAsync(int currencyId, CancellationToken ct = default)
            => _db.Policies.AnyAsync(p => p.CurrencyId == currencyId && p.Status == PolicyStatus.Active, ct);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
