namespace Insurance.Application.Buildings.Models;

    public sealed class BuildingListItemResponse
    {
        public Guid Id { get; init; }
        public string Street { get; init; } = null!;
        public string Number { get; init; } = null!;
        public int CityId { get; init; }
        public decimal InsuredValue { get; init; }
    }
