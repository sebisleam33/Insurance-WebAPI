using Insurance.Domain.Entities;

namespace Insurance.Infrastructure.Repositories.Interfaces;

public interface IBuildingRepository
{
    Task AddAsync(Building building, CancellationToken ct = default);

    Task<Building?> GetByIdAsync(Guid buildingId, bool trackChanges, CancellationToken ct = default);

    Task<Building?> GetByIdWithGeographyAsync(Guid buildingId, CancellationToken ct = default);

    Task<(IReadOnlyList<Building> Items, int TotalCount)> GetByClientAsync(
        Guid clientId, int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<Building>> GetAllByClientAsync(Guid clientId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
