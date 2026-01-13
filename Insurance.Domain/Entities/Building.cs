using Insurance.Domain.Entities.Geography;
using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities;

    public class Building
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public string Street { get; set; } = null!;
        public string Number { get; set; } = null!;

        public int CityId { get; set; }
        public City City { get; set; } = null!;


        public int ConstructionYear { get; set; }
        public BuildingType BuildingType { get; set; }

        public int Floors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }

        public bool? IsFloodZone { get; set; }
        public int? EarthquakeRiskZone { get; set; }
    }
