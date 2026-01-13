using Insurance.Application.Clients.Models;
using Insurance.Application.Common;

namespace Insurance.Application.Clients;

public interface IClientService
{
    Task<Guid> CreateAsync(CreateClientRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid clientId, UpdateClientRequest request, CancellationToken ct = default);
    Task<ClientDetailsResponse> GetByIdAsync(Guid clientId, CancellationToken ct = default);
    Task<PagedResult<ClientListItemResponse>> SearchAsync(
        string? name, string? identifier, int page, int pageSize, CancellationToken ct = default);
}
