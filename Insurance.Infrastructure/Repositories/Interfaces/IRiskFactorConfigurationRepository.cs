using Insurance.Domain.Entities.Metadata;

namespace Insurance.Infrastructure.Repositories.Interfaces;

    public interface IRiskFactorConfigurationRepository
    {
        Task<IReadOnlyList<RiskFactorConfiguration>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RiskFactorConfiguration>> GetActiveAsync(CancellationToken ct = default);
        Task<RiskFactorConfiguration?> GetByIdAsync(int id, bool trackChanges, CancellationToken ct = default);
        Task AddAsync(RiskFactorConfiguration cfg, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
