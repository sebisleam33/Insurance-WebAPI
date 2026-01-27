using Insurance.Domain.Entities.Metadata;

namespace Insurance.Infrastructure.Repositories.Interfaces;

    public interface IFeeConfigurationRepository
    {
        Task<IReadOnlyList<FeeConfiguration>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<FeeConfiguration>> GetActiveForNowAsync(DateTime nowUtc, CancellationToken ct = default);
        Task<FeeConfiguration?> GetByIdAsync(int id, bool trackChanges, CancellationToken ct = default);
        Task AddAsync(FeeConfiguration fee, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
