using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Infrastructure.Repositories.Interfaces;

public interface IPolicyRepository
    {
        Task AddAsync(Policy policy, CancellationToken ct = default);
        Task<Policy?> GetByIdAsync(Guid policyId, bool trackChanges, CancellationToken ct = default);
        Task<Policy?> GetByIdWithDetailsAsync(Guid policyId, CancellationToken ct = default);
        Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchAsync(
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateTime? startDateUtc,
            DateTime? endDateUtc,
            int page,
            int pageSize,
            CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
