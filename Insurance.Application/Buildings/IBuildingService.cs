using Insurance.Application.Buildings.Models;
using Insurance.Application.Common;

namespace Insurance.Application.Buildings;

    public interface IBuildingService
    {
        Task<Guid> CreateAsync(Guid clientId, CreateBuildingRequest request, CancellationToken ct = default);
        Task UpdateAsync (Guid buildingId, UpdateBuildingRequest request, CancellationToken ct = default);
        Task<BuildingDetailsResponse> GetByIdAsync (Guid buildingId, CancellationToken ct = default);
        Task<PagedResult<BuildingListItemResponse>> GetByClientAsync (Guid clientId, int page, int pageSize, CancellationToken ct = default); 
    }
