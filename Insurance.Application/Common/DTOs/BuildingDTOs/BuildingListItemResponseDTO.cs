namespace Insurance.Application.Common.DTOs.BuildingDTOs;

public sealed class BuildingListItemResponseDTO
    {
        public Guid Id { get; init; }
        public string Street { get; init; } = null!;
        public string Number { get; init; } = null!;
        public int CityId { get; init; }
        public decimal InsuredValue { get; init; }
    }
