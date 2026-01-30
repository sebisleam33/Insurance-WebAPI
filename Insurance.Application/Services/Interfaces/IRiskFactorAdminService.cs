using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;

namespace Insurance.Application.Services.Interfaces;

public interface IRiskFactorAdminService
    {
        Task<PagedResult<RiskFactorConfigurationResponseDTO>> ListAsync(int page, int pageSize, CancellationToken ct = default);
        Task<RiskFactorConfigurationResponseDTO> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(CreateRiskFactorConfigurationRequestDTO request, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateRiskFactorConfigurationRequestDTO request, CancellationToken ct = default);
    }
