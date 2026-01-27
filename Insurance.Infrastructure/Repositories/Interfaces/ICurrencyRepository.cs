using Insurance.Domain.Entities.Metadata;

namespace Insurance.Infrastructure.Repositories.Interfaces;

    public interface ICurrencyRepository
    {
        Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default);
        Task<Currency?> GetByIdAsync(int id, bool trackChanges, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
        Task AddAsync(Currency currency, CancellationToken ct = default);
        Task<bool> IsUsedInActivePoliciesAsync(int currencyId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
