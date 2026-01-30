using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.PolicyDTOs;
using Insurance.Domain.Enums;

namespace Insurance.Application.Services.Interfaces;

public interface IPolicyService
    {
        Task<Guid> CreateDraftAsync(CreatePolicyRequestDTO request, CancellationToken ct = default);
        Task ActivateAsync(Guid policyId, CancellationToken ct = default);
        Task CancelAsync(Guid policyId, CancelPolicyRequestDTO request, CancellationToken ct = default);
        Task<PolicyDetailsResponseDTO> GetByIdAsync(Guid policyId, CancellationToken ct = default);

        Task<PagedResult<PolicyListItemResponseDTO>> SearchAsync(
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateTime? startDateUtc,
            DateTime? endDateUtc,
            int page,
            int pageSize,
            CancellationToken ct = default);
    }
