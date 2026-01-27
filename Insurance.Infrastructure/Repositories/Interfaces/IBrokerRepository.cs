using Insurance.Domain.Entities;

namespace Insurance.Infrastructure.Repositories.Interfaces;

    public interface IBrokerRepository
    {
        Task AddAsync(Broker broker, CancellationToken ct = default);
        Task<Broker?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string brokerCode, CancellationToken ct = default);
        Task<(IReadOnlyList<Broker> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
