namespace Insurance.Domain.Entities.Geography;

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CountyId  { get; set; }
        public County County { get; set; } = null!;
        public ICollection<Building> Buildings { get; set; } = new List<Building>();
    }

