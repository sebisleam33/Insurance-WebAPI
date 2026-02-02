using AutoMapper;
using FluentAssertions;
using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class RiskFactorAdminServiceTests
    {
        private Mock<IRiskFactorConfigurationRepository> _risks = null!;
        private Mock<IGeographyRepository> _geo = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<ILogger<RiskFactorAdminService>> _logger = null!;
        private RiskFactorAdminService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _risks = new Mock<IRiskFactorConfigurationRepository>(MockBehavior.Strict);
            _geo = new Mock<IGeographyRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _logger = new Mock<ILogger<RiskFactorAdminService>>(MockBehavior.Loose);

            _sut = new RiskFactorAdminService(_risks.Object, _geo.Object, _mapper.Object, _logger.Object);
        }

        [Test]
        public async Task ListAsync_NormalizesPageAndPageSize_MapsItems()
        {
            var items = new List<RiskFactorConfiguration> { new() { Id = 1 }, new() { Id = 2 } };

            _risks.Setup(r => r.ListAsync(1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 2));

            _mapper.Setup(m => m.Map<RiskFactorConfigurationResponseDTO>(items[0]))
                .Returns(new RiskFactorConfigurationResponseDTO { Id = 1 });

            _mapper.Setup(m => m.Map<RiskFactorConfigurationResponseDTO>(items[1]))
                .Returns(new RiskFactorConfigurationResponseDTO { Id = 2 });

            var result = await _sut.ListAsync(0, 999);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);

            _risks.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
        {
            _risks.Setup(r => r.GetByIdAsync(10, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RiskFactorConfiguration?)null);

            var act = async () => await _sut.GetByIdAsync(10);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Risk factor configuration not found.");

            _risks.VerifyAll();
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_MapsAndReturns()
        {
            var entity = new RiskFactorConfiguration { Id = 10 };

            _risks.Setup(r => r.GetByIdAsync(10, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapper.Setup(m => m.Map<RiskFactorConfigurationResponseDTO>(entity))
                .Returns(new RiskFactorConfigurationResponseDTO { Id = 10 });

            var result = await _sut.GetByIdAsync(10);

            result.Id.Should().Be(10);

            _risks.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_LevelBuildingType_WhenBuildingTypeMissing_ThrowsValidation()
        {
            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.BuildingType,
                BuildingType = null,
                ReferenceId = null,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("BuildingType must be provided when Level is BuildingType.");
        }

        [Test]
        public async Task CreateAsync_LevelBuildingType_WhenReferenceIdProvided_ThrowsValidation()
        {
            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.BuildingType,
                BuildingType = BuildingType.Residential,
                ReferenceId = 1,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ReferenceId must be null when Level is BuildingType.");
        }

        [Test]
        public async Task CreateAsync_LevelCountry_WhenReferenceMissing_ThrowsValidation()
        {
            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.Country,
                ReferenceId = null,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ReferenceId must be provided when Level is Country.");
        }

        [Test]
        public async Task CreateAsync_LevelCountry_WhenNotFound_ThrowsNotFound()
        {
            _geo.Setup(g => g.CountryExistsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.Country,
                ReferenceId = 10,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Country not found.");

            _geo.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_LevelCountry_WhenBuildingTypeProvided_ThrowsValidation()
        {
            _geo.Setup(g => g.CountryExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.Country,
                ReferenceId = 1,
                BuildingType = BuildingType.Residential,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("BuildingType must be null for geographic levels.");

            _geo.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_LevelCounty_WhenNotFound_ThrowsNotFound()
        {
            _geo.Setup(g => g.CountyExistsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.County,
                ReferenceId = 5,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("County not found.");

            _geo.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_LevelCity_WhenNotFound_ThrowsNotFound()
        {
            _geo.Setup(g => g.CityExistsAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.City,
                ReferenceId = 7,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("City not found.");

            _geo.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_InvalidLevel_ThrowsValidation()
        {
            var req = new CreateRiskFactorConfigurationRequestDTO
            {
                Level = (RiskFactorLevel)999,
                AdjustmentPercentage = 0.1m
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Invalid RiskFactorLevel.");
        }

        [Test]
        public async Task CreateAsync_ValidBuildingType_CreatesAndReturnsId()
        {
            var entity = new RiskFactorConfiguration { Id = 123 };

            _mapper.Setup(m => m.Map<RiskFactorConfiguration>(It.IsAny<CreateRiskFactorConfigurationRequestDTO>()))
                .Returns(entity);

            _risks.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _risks.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var id = await _sut.CreateAsync(new CreateRiskFactorConfigurationRequestDTO
            {
                Level = RiskFactorLevel.BuildingType,
                BuildingType = BuildingType.Residential,
                ReferenceId = null,
                AdjustmentPercentage = 0.1m
            });

            id.Should().Be(123);

            _mapper.VerifyAll();
            _risks.VerifyAll();
        }

        [Test]
        public async Task UpdateAsync_WhenMissing_ThrowsNotFound()
        {
            _risks.Setup(r => r.GetByIdAsync(5, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RiskFactorConfiguration?)null);

            var act = async () => await _sut.UpdateAsync(5, new UpdateRiskFactorConfigurationRequestDTO
            {
                AdjustmentPercentage = 0.1m,
                IsActive = true
            });

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Risk factor configuration not found.");

            _risks.VerifyAll();
        }

        [Test]
        public async Task UpdateAsync_WhenFound_UpdatesAndSaves()
        {
            var entity = new RiskFactorConfiguration { Id = 5, AdjustmentPercentage = 0.01m, IsActive = false };

            _risks.Setup(r => r.GetByIdAsync(5, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _risks.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _sut.UpdateAsync(5, new UpdateRiskFactorConfigurationRequestDTO
            {
                AdjustmentPercentage = 0.25m,
                IsActive = true
            });

            entity.AdjustmentPercentage.Should().Be(0.25m);
            entity.IsActive.Should().BeTrue();

            _risks.VerifyAll();
        }
    }
