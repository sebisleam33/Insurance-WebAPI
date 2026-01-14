namespace Insurance.Domain.Entities.Geography;
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<County> Counties { get; set; } = new List<County>();
    }
