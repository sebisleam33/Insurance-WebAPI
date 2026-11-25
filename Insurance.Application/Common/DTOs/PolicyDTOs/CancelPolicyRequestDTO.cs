namespace Insurance.Application.Common.DTOs.PolicyDTOs;

public sealed class CancelPolicyRequestDTO
    {
        public DateTime CancelledAtUtc { get; set; } 
        public string Reason { get; set; } = null!;
    }
