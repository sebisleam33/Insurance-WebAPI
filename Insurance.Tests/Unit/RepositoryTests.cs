using FluentAssertions;
using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Tests.Infrastructure.Repositories;

    [TestFixture]
    public sealed class RepositoryTests
    {
        private SqliteConnection _connection = null!;
        private DbContextOptions<AppDbContext> _dbOptions = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var db = new AppDbContext(_dbOptions);
            db.Database.EnsureCreated();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _connection.Dispose();
        }

        private AppDbContext NewDb()
            => new AppDbContext(_dbOptions);

        private static (Country c, County co, City ci) SeedGeography(AppDbContext db)
        {
            var country = new Country { Name = "Romania" };
            db.Countries.Add(country);
            db.SaveChanges();

            var county = new County { Name = "Cluj", CountryId = country.Id };
            db.Counties.Add(county);
            db.SaveChanges();

            var city = new City { Name = "Cluj-Napoca", CountyId = county.Id };
            db.Cities.Add(city);
            db.SaveChanges();

            return (country, county, city);
        }

        private static Client SeedClient(AppDbContext db, string name, string idNo)
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = name,
                IdentificationNumber = idNo
            };
            db.Clients.Add(client);
            db.SaveChanges();
            return client;
        }

        private static Building SeedBuilding(AppDbContext db, Guid clientId, int cityId, string street)
        {
            var b = new Building
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                CityId = cityId,
                Street = street,
                Number = "1",
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 1000
            };
            db.Buildings.Add(b);
            db.SaveChanges();
            return b;
        }

        [SetUp]
        public void Clean()
        {
            using var db = NewDb();
            db.Buildings.RemoveRange(db.Buildings);
            db.ClientIdentifierChanges.RemoveRange(db.ClientIdentifierChanges);
            db.Clients.RemoveRange(db.Clients);
            db.Cities.RemoveRange(db.Cities);
            db.Counties.RemoveRange(db.Counties);
            db.Countries.RemoveRange(db.Countries);
            db.SaveChanges();
        }

        [Test]
        public async Task ClientRepository_ExistsById_And_ExistsByIdentifier_Work()
        {
            using var db = NewDb();
            var client = SeedClient(db, "Ion Popescu", "CNP-1");

            var repo = new ClientRepository(db);

            (await repo.ExistsByIdAsync(client.Id)).Should().BeTrue();
            (await repo.ExistsByIdAsync(Guid.NewGuid())).Should().BeFalse();

            (await repo.ExistsByIdentifierAsync("CNP-1")).Should().BeTrue();
            (await repo.ExistsByIdentifierAsync("NOPE")).Should().BeFalse();
        }

        [Test]
        public async Task ClientRepository_GetById_TrackChanges_TogglesTracking()
        {
            using var db = NewDb();
            var client = SeedClient(db, "Ion Popescu", "CNP-1");

            var repo = new ClientRepository(db);

            var tracked = await repo.GetByIdAsync(client.Id, trackChanges: true);
            tracked.Should().NotBeNull();
            db.ChangeTracker.Entries().Should().NotBeEmpty(); 

            db.ChangeTracker.Clear();

            var untracked = await repo.GetByIdAsync(client.Id, trackChanges: false);
            untracked.Should().NotBeNull();
            db.ChangeTracker.Entries().Should().BeEmpty(); 
        }

        [Test]
        public async Task ClientRepository_Add_And_SaveChanges_Persists()
        {
            using var db = NewDb();
            var repo = new ClientRepository(db);

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Maria Ionescu",
                IdentificationNumber = "CNP-2"
            };

            await repo.AddAsync(client);
            await repo.SaveChangesAsync();

            (await repo.ExistsByIdAsync(client.Id)).Should().BeTrue();
        }

        [Test]
        public async Task ClientRepository_AddIdentifierChange_Persists()
        {
            using var db = NewDb();
            var client = SeedClient(db, "Ion Popescu", "OLD");

            var repo = new ClientRepository(db);

            var change = new ClientIdentifierChange
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                OldIdentificationNumber = "OLD",
                NewIdentificationNumber = "NEW",
                ChangedAtUtc = DateTime.UtcNow,
                Reason = "Fix typo"
            };

            await repo.AddIdentifierChangeAsync(change);
            await repo.SaveChangesAsync();

            var count = await db.ClientIdentifierChanges.CountAsync();
            count.Should().Be(1);
        }

        [Test]
        public async Task ClientRepository_Search_ByName_Tokens_IsCaseInsensitive_AndPaged()
        {
            using var db = NewDb();
            SeedClient(db, "Ion Popescu", "CNP-1");
            SeedClient(db, "Maria Ionescu", "CNP-2");
            SeedClient(db, "Alexandru", "CNP-3");

            var repo = new ClientRepository(db);

            var (items, total) = await repo.SearchAsync(name: "ion", identifier: null, page: 1, pageSize: 1);

            total.Should().Be(2); 
            items.Should().HaveCount(1);
            items[0].Name.Should().NotBeNull();
        }

        [Test]
        public async Task ClientRepository_Search_ByIdentifier_ExactMatch()
        {
            using var db = NewDb();
            SeedClient(db, "Ion Popescu", "CNP-1");
            SeedClient(db, "Maria Ionescu", "CNP-2");

            var repo = new ClientRepository(db);

            var (items, total) = await repo.SearchAsync(name: null, identifier: "CNP-2", page: 1, pageSize: 20);

            total.Should().Be(1);
            items.Should().ContainSingle();
            items[0].IdentificationNumber.Should().Be("CNP-2");
        }

        [Test]
        public async Task BuildingRepository_GetByClient_ReturnsTotalAndPagedItems()
        {
            using var db = NewDb();
            var (_, _, city) = SeedGeography(db);
            var client = SeedClient(db, "Ion Popescu", "CNP-1");

            SeedBuilding(db, client.Id, city.Id, "A Street");
            SeedBuilding(db, client.Id, city.Id, "B Street");

            var repo = new BuildingRepository(db);

            var (items, total) = await repo.GetByClientAsync(client.Id, page: 1, pageSize: 1);

            total.Should().Be(2);
            items.Should().HaveCount(1);
            items[0].ClientId.Should().Be(client.Id);
        }

        [Test]
        public async Task BuildingRepository_GetAllByClient_ReturnsOrdered()
        {
            using var db = NewDb();
            var (_, _, city) = SeedGeography(db);
            var client = SeedClient(db, "Ion Popescu", "CNP-1");

            SeedBuilding(db, client.Id, city.Id, "B Street");
            SeedBuilding(db, client.Id, city.Id, "A Street");

            var repo = new BuildingRepository(db);

            var items = await repo.GetAllByClientAsync(client.Id);

            items.Should().HaveCount(2);
            items.Select(x => x.Street).Should().ContainInOrder("A Street", "B Street");
        }

        [Test]
        public async Task BuildingRepository_GetById_TrackChanges_TogglesTracking()
        {
            using var db = NewDb();
            var (_, _, city) = SeedGeography(db);
            var client = SeedClient(db, "Ion Popescu", "CNP-1");
            var building = SeedBuilding(db, client.Id, city.Id, "Main");

            var repo = new BuildingRepository(db);

            var tracked = await repo.GetByIdAsync(building.Id, trackChanges: true);
            tracked.Should().NotBeNull();
            db.ChangeTracker.Entries().Should().NotBeEmpty();

            db.ChangeTracker.Clear();

            var untracked = await repo.GetByIdAsync(building.Id, trackChanges: false);
            untracked.Should().NotBeNull();
            db.ChangeTracker.Entries().Should().BeEmpty();
        }

        [Test]
        public async Task BuildingRepository_GetByIdWithGeography_IncludesCountryCountyCity()
        {
            using var db = NewDb();
            var (country, county, city) = SeedGeography(db);
            var client = SeedClient(db, "Ion Popescu", "CNP-1");

            var building = SeedBuilding(db, client.Id, city.Id, "Main");

            var repo = new BuildingRepository(db);

            var loaded = await repo.GetByIdWithGeographyAsync(building.Id);

            loaded.Should().NotBeNull();
            loaded!.City.Should().NotBeNull();
            loaded.City.County.Should().NotBeNull();
            loaded.City.County.Country.Should().NotBeNull();

            loaded.City.Id.Should().Be(city.Id);
            loaded.City.County.Id.Should().Be(county.Id);
            loaded.City.County.Country.Id.Should().Be(country.Id);
        }

        [Test]
        public async Task BuildingRepository_AddAndSave_Persists()
        {
            using var db = NewDb();
            var (_, _, city) = SeedGeography(db);
            var client = SeedClient(db, "Ion Popescu", "CNP-1");

            var repo = new BuildingRepository(db);

            var building = new Building
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                CityId = city.Id,
                Street = "Main",
                Number = "1",
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 1000
            };

            await repo.AddAsync(building);
            await repo.SaveChangesAsync();

            var exists = await db.Buildings.AnyAsync(x => x.Id == building.Id);
            exists.Should().BeTrue();
        }
    }
