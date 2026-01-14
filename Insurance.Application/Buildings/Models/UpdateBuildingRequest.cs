using Insurance.Domain.Enums;

namespace Insurance.Application.Buildings.Models;

public sealed class UpdateBuildingRequest
    {
        public string Number { get; init; } = null!;
        public string Street { get; init; } = null!;
        public int CityId { get; init; }

        public int ConstructionYear {  get; init; }
        public BuildingType BuildingType { get; init; }

        public int Floors { get; init; }
        public decimal SurfaceArea { get; init; }
        public decimal InsuredValuea { get; init; }

        public bool? IsFloodZone { get; init; }
        public int? EarthquakeRiskZone { get; init; }
    }
