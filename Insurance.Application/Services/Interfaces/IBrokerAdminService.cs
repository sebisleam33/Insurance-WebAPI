using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;

namespace Insurance.Application.Services.Interfaces;

public interface IBrokerAdminService
    {
        Task<PagedResult<BrokerResponseDTO>> ListAsync(int page, int pageSize, CancellationToken ct = default);
        Task<BrokerResponseDTO> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(CreateBrokerRequestDTO request, CancellationToken ct = default);
        Task UpdateAsync(Guid id, UpdateBrokerRequestDTO request, CancellationToken ct = default);
        Task ActivateAsync(Guid id, CancellationToken ct = default);
        Task DeactivateAsync(Guid id, CancellationToken ct = default);
    }
