using Insurance.Application.Common.DTOs.AdminDTOs;

namespace Insurance.Application.Services.Interfaces;

    public interface ICurrencyAdminService
    {
        Task<IReadOnlyList<CurrencyResponseDTO>> GetAllAsync(CancellationToken ct = default);
        Task<int> CreateAsync(CurrencyRequestDTO request, CancellationToken ct = default);
        Task UpdateAsync(int id, CurrencyRequestDTO request, CancellationToken ct = default);
        Task DeactivateAsync(int id, CancellationToken ct = default);
        Task ActivateAsync(int id, CancellationToken ct = default);
    }
