using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.BuildingDTOs;

namespace Insurance.Application.Services.Interfaces;

    public interface IBuildingService
    {
        Task<Guid> CreateAsync(Guid clientId, CreateBuildingRequestDTO request, CancellationToken ct = default);
        Task UpdateAsync (Guid buildingId, UpdateBuildingRequestDTO request, CancellationToken ct = default);
        Task<BuildingDetailsResponseDTO> GetByIdAsync (Guid buildingId, CancellationToken ct = default);
        Task<PagedResult<BuildingListItemResponseDTO>> GetByClientAsync (Guid clientId, int page, int pageSize, CancellationToken ct = default); 
    }
