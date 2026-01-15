using Insurance.Domain.Entities;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories.Implementations;

public sealed class ClientRepository : IClientRepository
{
    private readonly AppDbContext _db;
    public ClientRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsByIdAsync(Guid clientId, CancellationToken ct = default)
        => _db.Clients.AnyAsync(x => x.Id == clientId, ct);

    public Task<bool> ExistsByIdentifierAsync(string identificationNumber, CancellationToken ct = default)
        => _db.Clients.AnyAsync(x => x.IdentificationNumber == identificationNumber, ct);

    public Task AddAsync(Client client, CancellationToken ct = default)
    {
        _db.Clients.Add(client);
        return Task.CompletedTask;
    }

    public async Task<Client?> GetByIdAsync(Guid clientId, bool trackChanges, CancellationToken ct = default)
    {
        var query = trackChanges ? _db.Clients : _db.Clients.AsNoTracking();
        return await query.FirstOrDefaultAsync(x => x.Id == clientId, ct);
    }

    public async Task<(IReadOnlyList<Client> Items, int TotalCount)> SearchAsync(
        string? name, string? identifier, int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<Client> query = _db.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.Name.Contains(name.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            query = query.Where(x => x.IdentificationNumber == identifier.Trim());
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
