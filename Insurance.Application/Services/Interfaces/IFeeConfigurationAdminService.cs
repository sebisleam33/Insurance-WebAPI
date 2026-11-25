using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;

namespace Insurance.Application.Services.Interfaces;

public interface IFeeConfigurationAdminService
    {
        Task<PagedResult<FeeConfigurationResponseDTO>> ListAsync(int page, int pageSize, CancellationToken ct = default);
        Task<FeeConfigurationResponseDTO> GetByIdAsync(int id, CancellationToken ct = default);   
        Task<int> CreateAsync(CreateFeeConfigurationRequestDTO request, CancellationToken ct = default); 
        Task UpdateAsync(int id, UpdateFeeConfigurationRequestDTO request, CancellationToken ct = default);
    }
