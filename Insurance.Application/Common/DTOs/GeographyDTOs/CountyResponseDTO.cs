namespace Insurance.Application.Common.DTOs.GeographyDTOs;

    public sealed class CountyResponseDTO
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public int CountryId { get; init; }
    }
