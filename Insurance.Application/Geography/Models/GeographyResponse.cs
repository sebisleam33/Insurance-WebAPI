namespace Insurance.Application.Geography.Models;

    public sealed class CountryResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
    }

    public sealed class CountyResponse
    { 
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public int CountryId { get; init; }
    }

    public sealed class CityResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public int CountyId { get; init; }
    }
