using Insurance.Domain.Entities;

namespace Insurance.Infrastructure.Repositories.Interfaces;

public interface IClientRepository
{
    Task<bool> ExistsByIdAsync(Guid clientId, CancellationToken ct = default);
    Task<bool> ExistsByIdentifierAsync(string identificationNumber, CancellationToken ct = default);

    Task AddAsync(Client client, CancellationToken ct = default);
    Task<Client?> GetByIdAsync(Guid clientId, bool trackChanges, CancellationToken ct = default);

    Task<(IReadOnlyList<Client> Items, int TotalCount)> SearchAsync(
        string? name, string? identifier, int page, int pageSize, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
    Task AddIdentifierChangeAsync(ClientIdentifierChange change, CancellationToken ct = default);
}
