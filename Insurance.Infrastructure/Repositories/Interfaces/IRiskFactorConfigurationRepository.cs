using Insurance.Domain.Entities.Metadata;

namespace Insurance.Infrastructure.Repositories.Interfaces;

public interface IRiskFactorConfigurationRepository
    {
        Task<IReadOnlyList<RiskFactorConfiguration>> GetActiveAsync(CancellationToken ct = default);
        Task<(IReadOnlyList<RiskFactorConfiguration> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken ct = default);
        Task<RiskFactorConfiguration?> GetByIdAsync(int id, bool trackChanges, CancellationToken ct = default);
        Task AddAsync(RiskFactorConfiguration risk, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
