namespace Insurance.Application.Common.DTOs.ClientDTOs;

public sealed class ChangeClientIdentifierRequestDTO
{
    public string NewIdentificationNumber { get; init; } = null!;
    public string? Reason { get; init; }
}
