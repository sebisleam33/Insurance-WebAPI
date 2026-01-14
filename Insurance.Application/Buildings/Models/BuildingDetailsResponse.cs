using Insurance.Domain.Enums;

namespace Insurance.Application.Buildings.Models;

    public sealed class BuildingDetailsResponse
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }

        public string Street { get; init; } = null!;
        public string Number { get; init; } = null!;

        public int CityId { get; init; }
        public string CityName { get; init; } = null!;
        public int CountyId { get; init; }
        public string CountyName { get; init; } = null!;
        public int CountryId { get; init; }
        public string CountryName { get; init; } = null!;

        public int ConstructionYear { get; init; }
        public BuildingType BuildingType { get; init; }

        public int Floors { get; init; }
        public decimal SurfaceArea { get; init; }
        public decimal InsuredValue { get; init; }

        public bool? IsFloodZone { get; init; }
        public int? EarthquakeRiskZone { get; init; }
    }
