namespace Insurance.Application.Common.DTOs.GeographyDTOs;

    public sealed class CityResponseDTO
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public int CountyId { get; init; }
    }
