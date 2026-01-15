using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.ClientDTOs;

namespace Insurance.Application.Services.Interfaces;

public interface IClientService
{
    Task<Guid> CreateAsync(CreateClientRequestDTO request, CancellationToken ct = default);
    Task UpdateAsync(Guid clientId, UpdateClientRequestDTO request, CancellationToken ct = default);
    Task<ClientDetailsResponseDTO> GetByIdAsync(Guid clientId, CancellationToken ct = default);
    Task<PagedResult<ClientListItemResponseDTO>> SearchAsync(
        string? name, string? identifier, int page, int pageSize, CancellationToken ct = default);
}
