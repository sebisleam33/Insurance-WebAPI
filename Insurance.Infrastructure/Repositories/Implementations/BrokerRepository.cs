using Insurance.Domain.Entities;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

public sealed class BrokerRepository : IBrokerRepository
    {
        private readonly AppDbContext _db;
        public BrokerRepository(AppDbContext db) => _db = db;

        public async Task<(IReadOnlyList<Broker> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var q = _db.Brokers.AsNoTracking().OrderBy(x => x.BrokerCode);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return (items, total);
        }

        public async Task<Broker?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken ct = default)
        {
            var q = trackChanges ? _db.Brokers : _db.Brokers.AsNoTracking();
            return await q.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task<bool> ExistsByCodeAsync(string brokerCode, CancellationToken ct = default)
            => _db.Brokers.AnyAsync(x => x.BrokerCode == brokerCode, ct);

        public Task AddAsync(Broker broker, CancellationToken ct = default)
        {
            _db.Brokers.Add(broker);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
