using Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;

namespace Insurance.Application.Services.Interfaces;

public interface ICurrencyAdminService
    {
        Task<IReadOnlyList<CurrencyResponseDTO>> GetAllAsync(CancellationToken ct = default);
        Task<CurrencyResponseDTO> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(CreateCurrencyRequestDTO request, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateCurrencyRequestDTO request, CancellationToken ct = default);
    }
