namespace Insurance.Domain.Entities.Geography;
    public class County
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CountryId { get; set; }
        public Country country { get; set; } = null!;

        public ICollection<City> Cities { get; set; } = new List<City>();

    }
