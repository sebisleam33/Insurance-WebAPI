using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

public sealed class PolicyRepository : IPolicyRepository
    {
        private readonly AppDbContext _db;
        public PolicyRepository(AppDbContext db) => _db = db;

        public Task AddAsync(Policy policy, CancellationToken ct = default)
        {
            _db.Policies.Add(policy);
            return Task.CompletedTask;
        }

        public async Task<Policy?> GetByIdAsync(Guid policyId, bool trackChanges, CancellationToken ct = default)
        {
            var q = trackChanges ? _db.Policies : _db.Policies.AsNoTracking();
            return await q.FirstOrDefaultAsync(x => x.Id == policyId, ct);
        }

        public async Task<Policy?> GetByIdWithDetailsAsync(Guid policyId, CancellationToken ct = default)
        {
            return await _db.Policies
                .AsNoTracking()
                .Include(p => p.Client)
                .Include(p => p.Broker)
                .Include(p => p.Currency)
                .Include(p => p.Building)
                    .ThenInclude(b => b.City)
                        .ThenInclude(c => c.County)
                            .ThenInclude(co => co.Country)
                .FirstOrDefaultAsync(p => p.Id == policyId, ct);
        }

        public async Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchAsync(
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateTime? startDateUtc,
            DateTime? endDateUtc,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            IQueryable<Policy> q = _db.Policies
                .AsNoTracking()
                .Include(p => p.Client)
                .Include(p => p.Broker)
                .Include(p => p.Currency);

            if (clientId.HasValue) 
            { 
                q = q.Where(x => x.ClientId == clientId.Value); 
            }
            if (brokerId.HasValue) 
            { 
                q = q.Where(x => x.BrokerId == brokerId.Value); 
            }
            if (status.HasValue) 
            { 
                q = q.Where(x => x.Status == status.Value); 
            }
            if (startDateUtc.HasValue) 
            { 
                q = q.Where(x => x.EndDateUtc >= startDateUtc.Value); 
            }
            if (endDateUtc.HasValue) 
            { 
                q = q.Where(x => x.StartDateUtc <= endDateUtc.Value); 
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }