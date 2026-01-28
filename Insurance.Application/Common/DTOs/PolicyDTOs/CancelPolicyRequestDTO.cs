namespace Insurance.Application.Common.DTOs.PolicyDTOs;

    public sealed class CancelPolicyRequestDTO
    {
        public DateTime CancelledAtUtc { get; init; }
        public string Reason { get; init; } = null!;
    }
