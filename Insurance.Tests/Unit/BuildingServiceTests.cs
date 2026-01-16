using AutoMapper;
using FluentAssertions;
using Insurance.Api.Mapping;
using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Insurance.Tests.Unit;

    [TestFixture]
    public sealed class BuildingServiceTests
    {
        private static IMapper CreateMapper()
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddProfile(new ApiMappingProfile());
            }, NullLoggerFactory.Instance);

            cfg.AssertConfigurationIsValid();
            return cfg.CreateMapper();
        }

        private static CreateBuildingRequestDTO ValidCreate(int cityId = 1) => new()
        {
            Street = "Main",
            Number = "1",
            CityId = cityId,
            ConstructionYear = 2000,
            SurfaceArea = 10,
            InsuredValue = 100
        };

        private static UpdateBuildingRequestDTO ValidUpdate(int cityId = 1) => new()
        {
            Street = "Main",
            Number = "1",
            CityId = cityId,
            ConstructionYear = 2000,
            SurfaceArea = 10,
            InsuredValue = 100
        };

    [Test]
    public async Task CreateAsync_ClientMissing_ThrowsNotFound()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.ExistsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        var buildings = new Mock<IBuildingRepository>(MockBehavior.Strict);
        var geo = new Mock<IGeographyRepository>(MockBehavior.Strict);

        var svc = new BuildingService(clients.Object, buildings.Object, geo.Object, CreateMapper(), NullLogger<BuildingService>.Instance);

        Func<Task> act = async () => await svc.CreateAsync(Guid.NewGuid(), ValidCreate());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Client not found.");
    }

    [Test]
    public async Task CreateAsync_CityMissing_ThrowsValidation()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.ExistsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var geo = new Mock<IGeographyRepository>();
        geo.Setup(x => x.CityExistsAsync(999, It.IsAny<CancellationToken>()))
           .ReturnsAsync(false);

        var buildings = new Mock<IBuildingRepository>(MockBehavior.Strict);

        var svc = new BuildingService(clients.Object, buildings.Object, geo.Object, CreateMapper(), NullLogger<BuildingService>.Instance);

        Func<Task> act = async () => await svc.CreateAsync(Guid.NewGuid(), ValidCreate(cityId: 999));

        await act.Should().ThrowAsync<ValidationException>().WithMessage("City does not exist.");
    }

    [Test]
    public async Task CreateAsync_Valid_Persists_AndReturnsId()
    {
        var clientId = Guid.NewGuid();

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var geo = new Mock<IGeographyRepository>();
        geo.Setup(x => x.CityExistsAsync(1, It.IsAny<CancellationToken>()))
           .ReturnsAsync(true);

        var buildings = new Mock<IBuildingRepository>();
        buildings.Setup(x => x.AddAsync(It.IsAny<Insurance.Domain.Entities.Building>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
        buildings.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var svc = new BuildingService(clients.Object, buildings.Object, geo.Object, CreateMapper(), NullLogger<BuildingService>.Instance);

        var id = await svc.CreateAsync(clientId, ValidCreate());

        id.Should().NotBeEmpty();
        buildings.Verify(x => x.AddAsync(It.Is<Insurance.Domain.Entities.Building>(b => b.ClientId == clientId), It.IsAny<CancellationToken>()), Times.Once);
        buildings.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_CityMissing_ThrowsValidation()
    {
        var geo = new Mock<IGeographyRepository>();
        geo.Setup(x => x.CityExistsAsync(999, It.IsAny<CancellationToken>()))
           .ReturnsAsync(false);

        var svc = new BuildingService(
            new Mock<IClientRepository>(MockBehavior.Loose).Object,
            new Mock<IBuildingRepository>(MockBehavior.Loose).Object,
            geo.Object,
            CreateMapper(),
            NullLogger<BuildingService>.Instance);

        Func<Task> act = async () => await svc.UpdateAsync(Guid.NewGuid(), ValidUpdate(cityId: 999));

        await act.Should().ThrowAsync<ValidationException>().WithMessage("City does not exist.");
    }

    [Test]
    public async Task UpdateAsync_BuildingMissing_ThrowsNotFound()
    {
        var geo = new Mock<IGeographyRepository>();
        geo.Setup(x => x.CityExistsAsync(1, It.IsAny<CancellationToken>()))
           .ReturnsAsync(true);

        var buildings = new Mock<IBuildingRepository>();
        buildings.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Insurance.Domain.Entities.Building?)null);

        var svc = new BuildingService(
            new Mock<IClientRepository>(MockBehavior.Loose).Object,
            buildings.Object,
            geo.Object,
            CreateMapper(),
            NullLogger<BuildingService>.Instance);

        Func<Task> act = async () => await svc.UpdateAsync(Guid.NewGuid(), ValidUpdate());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Building not found.");
    }

    [Test]
    public async Task GetByIdAsync_Missing_ThrowsNotFound()
    {
        var buildings = new Mock<IBuildingRepository>();
        buildings.Setup(x => x.GetByIdWithGeographyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Insurance.Domain.Entities.Building?)null);

        var svc = new BuildingService(
            new Mock<IClientRepository>(MockBehavior.Loose).Object,
            buildings.Object,
            new Mock<IGeographyRepository>(MockBehavior.Loose).Object,
            CreateMapper(),
            NullLogger<BuildingService>.Instance);

        Func<Task> act = async () => await svc.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Building not found.");
    }

    [Test]
    public async Task GetByClientAsync_Clamps_AndClientMissing_ThrowsNotFound()
    {
        var clientId = Guid.NewGuid();

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        var svc = new BuildingService(
            clients.Object,
            new Mock<IBuildingRepository>(MockBehavior.Loose).Object,
            new Mock<IGeographyRepository>(MockBehavior.Loose).Object,
            CreateMapper(),
            NullLogger<BuildingService>.Instance);

        // page<=0 => clamp; pageSize>200 => clamp (exercise those lines)
        Func<Task> act = async () => await svc.GetByClientAsync(clientId, page: 0, pageSize: 999);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Client not found.");
        clients.Verify(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetByClientAsync_ReturnsPagedResult()
    {
        var clientId = Guid.NewGuid();

        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var buildings = new Mock<IBuildingRepository>();
        buildings.Setup(x => x.GetByClientAsync(clientId, 1, 20, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((new List<Insurance.Domain.Entities.Building>(), 0));

        var svc = new BuildingService(
            clients.Object,
            buildings.Object,
            new Mock<IGeographyRepository>(MockBehavior.Loose).Object,
            CreateMapper(),
            NullLogger<BuildingService>.Instance);

        var result = await svc.GetByClientAsync(clientId, page: 0, pageSize: 999);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(0);
    }

    [Test]
            public void CreateAsync_MissingStreet_ThrowsValidation()
        {
            var clients = new Mock<IClientRepository>(MockBehavior.Loose);
            var buildings = new Mock<IBuildingRepository>(MockBehavior.Loose);
            var geo = new Mock<IGeographyRepository>(MockBehavior.Loose);

            var svc = new BuildingService(
                clients.Object,
                buildings.Object,
                geo.Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.CreateAsync(Guid.NewGuid(), new CreateBuildingRequestDTO
            {
                Street = "",
                Number = "1",
                CityId = 1,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 100_000
            });

            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public void CreateAsync_NegativeInsuredValue_ThrowsValidation()
        {
            var clients = new Mock<IClientRepository>(MockBehavior.Loose);
            var buildings = new Mock<IBuildingRepository>(MockBehavior.Loose);
            var geo = new Mock<IGeographyRepository>(MockBehavior.Loose);

            var svc = new BuildingService(
                clients.Object,
                buildings.Object,
                geo.Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.CreateAsync(Guid.NewGuid(), new CreateBuildingRequestDTO
            {
                Street = "Main",
                Number = "1",
                CityId = 1,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = -1
            });

            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task CreateAsync_ClientNotFound_ThrowsNotFound()
        {
            var clientId = Guid.NewGuid();

            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

            var buildings = new Mock<IBuildingRepository>(MockBehavior.Strict);
            var geo = new Mock<IGeographyRepository>(MockBehavior.Strict);

            var svc = new BuildingService(
                clients.Object,
                buildings.Object,
                geo.Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.CreateAsync(clientId, new CreateBuildingRequestDTO
            {
                Street = "Main",
                Number = "1",
                CityId = 1,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 100_000
            });

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task CreateAsync_CityNotFound_ThrowsValidation()
        {
            var clientId = Guid.NewGuid();

            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var geo = new Mock<IGeographyRepository>();
            geo.Setup(x => x.CityExistsAsync(999, It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

            var buildings = new Mock<IBuildingRepository>(MockBehavior.Strict);

            var svc = new BuildingService(
                clients.Object,
                buildings.Object,
                geo.Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.CreateAsync(clientId, new CreateBuildingRequestDTO
            {
                Street = "Main",
                Number = "1",
                CityId = 999,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 100_000
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task CreateAsync_Valid_CallsAddAndSave_ReturnsId()
        {
            var clientId = Guid.NewGuid();

            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var geo = new Mock<IGeographyRepository>();
            geo.Setup(x => x.CityExistsAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            var buildings = new Mock<IBuildingRepository>();
            buildings.Setup(x => x.AddAsync(It.IsAny<Insurance.Domain.Entities.Building>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            buildings.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            var svc = new BuildingService(
                clients.Object,
                buildings.Object,
                geo.Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            var id = await svc.CreateAsync(clientId, new CreateBuildingRequestDTO
            {
                Street = "Main",
                Number = "1",
                CityId = 1,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 100_000
            });

            id.Should().NotBeEmpty();
            buildings.Verify(x => x.AddAsync(It.IsAny<Insurance.Domain.Entities.Building>(), It.IsAny<CancellationToken>()), Times.Once);
            buildings.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_BuildingNotFound_ThrowsNotFound()
        {
            var buildings = new Mock<IBuildingRepository>();
            buildings.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Insurance.Domain.Entities.Building?)null);

            var clients = new Mock<IClientRepository>(MockBehavior.Loose);

            var geo = new Mock<IGeographyRepository>();
            geo.Setup(x => x.CityExistsAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            var svc = new BuildingService(
                clients.Object,
                buildings.Object,
                geo.Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.UpdateAsync(Guid.NewGuid(), new UpdateBuildingRequestDTO
            {
                Street = "Main",
                Number = "1",
                CityId = 1,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 100_000
            });

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task UpdateAsync_CityNotFound_ThrowsValidation()
        {
            var geo = new Mock<IGeographyRepository>();
            geo.Setup(x => x.CityExistsAsync(999, It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

            var buildings = new Mock<IBuildingRepository>(MockBehavior.Strict);
            var clients = new Mock<IClientRepository>(MockBehavior.Loose);

            var svc = new BuildingService(
                clients.Object,
                buildings.Object,
                geo.Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.UpdateAsync(Guid.NewGuid(), new UpdateBuildingRequestDTO
            {
                Street = "Main",
                Number = "1",
                CityId = 999,
                ConstructionYear = 2000,
                SurfaceArea = 100,
                InsuredValue = 100_000
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task GetByIdAsync_NotFound_ThrowsNotFound()
        {
            var buildings = new Mock<IBuildingRepository>();
            buildings.Setup(x => x.GetByIdWithGeographyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Insurance.Domain.Entities.Building?)null);

            var svc = new BuildingService(
                new Mock<IClientRepository>(MockBehavior.Loose).Object,
                buildings.Object,
                new Mock<IGeographyRepository>(MockBehavior.Loose).Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.GetByIdAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task GetByClientAsync_ClientNotFound_ThrowsNotFound()
        {
            var clientId = Guid.NewGuid();

            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

            var svc = new BuildingService(
                clients.Object,
                new Mock<IBuildingRepository>(MockBehavior.Loose).Object,
                new Mock<IGeographyRepository>(MockBehavior.Loose).Object,
                CreateMapper(),
                NullLogger<BuildingService>.Instance);

            Func<Task> act = async () => await svc.GetByClientAsync(clientId, 1, 20);

            await act.Should().ThrowAsync<NotFoundException>();
        }

    [Test]
    public void CreateAsync_NegativeSurface_ThrowsValidation()
    {
        var clients = new Mock<IClientRepository>();
        clients.Setup(x => x.ExistsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var geo = new Mock<IGeographyRepository>();
        geo.Setup(x => x.CityExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(true);

        var buildings = new Mock<IBuildingRepository>(MockBehavior.Loose);

        var svc = new BuildingService(clients.Object, buildings.Object, geo.Object, CreateMapper(), NullLogger<BuildingService>.Instance);

        Func<Task> act = async () => await svc.CreateAsync(Guid.NewGuid(), new CreateBuildingRequestDTO
        {
            Street = "Main",
            Number = "1",
            CityId = 1,
            ConstructionYear = 2000,
            SurfaceArea = -10,
            InsuredValue = 1000
        });

        act.Should().ThrowAsync<ValidationException>();
    }

        [Test]
        public async Task GetByClientAsync_ClampsPageAndPageSize()
        {
            var clientId = Guid.NewGuid();

            var clients = new Mock<IClientRepository>();
            clients.Setup(x => x.ExistsByIdAsync(clientId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var buildings = new Mock<IBuildingRepository>();
            buildings.Setup(x => x.GetByClientAsync(clientId, 1, 20, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((new List<Insurance.Domain.Entities.Building>(), 0));

            var geo = new Mock<IGeographyRepository>(MockBehavior.Loose);

            var svc = new BuildingService(clients.Object, buildings.Object, geo.Object, CreateMapper(), NullLogger<BuildingService>.Instance);

            var result = await svc.GetByClientAsync(clientId, page: 0, pageSize: 999);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);

            buildings.Verify(x => x.GetByClientAsync(clientId, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
