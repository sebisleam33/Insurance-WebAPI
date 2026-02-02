using AutoMapper;
using FluentAssertions;
using Insurance.Application.Common.DTOs.PolicyDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Entities.Geography;
using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class PolicyServiceTests
    {
        private Mock<IClientRepository> _clients = null!;
        private Mock<IBuildingRepository> _buildings = null!;
        private Mock<IBrokerRepository> _brokers = null!;
        private Mock<ICurrencyRepository> _currencies = null!;
        private Mock<IPolicyRepository> _policies = null!;
        private Mock<IPolicyPremiumCalculatorService> _calculator = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<ILogger<PolicyService>> _logger = null!;
        private PolicyService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _clients = new Mock<IClientRepository>(MockBehavior.Strict);
            _buildings = new Mock<IBuildingRepository>(MockBehavior.Strict);
            _brokers = new Mock<IBrokerRepository>(MockBehavior.Strict);
            _currencies = new Mock<ICurrencyRepository>(MockBehavior.Strict);
            _policies = new Mock<IPolicyRepository>(MockBehavior.Strict);
            _calculator = new Mock<IPolicyPremiumCalculatorService>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _logger = new Mock<ILogger<PolicyService>>();

            _sut = new PolicyService(
                _clients.Object,
                _buildings.Object,
                _brokers.Object,
                _currencies.Object,
                _policies.Object,
                _calculator.Object,
                _mapper.Object,
                _logger.Object);
        }

        [Test]
        public async Task CreateDraftAsync_WhenClientIdEmpty_ThrowsValidation()
        {
            var act = async () => await _sut.CreateDraftAsync(new CreatePolicyRequestDTO
            {
                ClientId = Guid.Empty,
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 1m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(2)
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ClientId is required.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenClientNotFound_ThrowsNotFound()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 100m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(10)
            };

            _clients.Setup(c => c.ExistsByIdAsync(req.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Client not found.");

            _clients.VerifyAll();
        }

        [Test]
        public async Task CreateDraftAsync_WhenBuildingNotForClient_ThrowsValidation()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 100m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(10)
            };

            _clients.Setup(c => c.ExistsByIdAsync(req.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _buildings.Setup(b => b.GetByIdWithGeographyAsync(req.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building
                {
                    Id = req.BuildingId,
                    ClientId = Guid.NewGuid(), // different
                    CityId = 1,
                    City = new City { Id = 1, CountyId = 1, County = new County { Id = 1, CountryId = 1, Country = new Country { Id = 1, Name = "X" }, Name = "Y" }, Name = "Z" },
                    BuildingType = BuildingType.Residential,
                    Street = "S",
                    Number = "1",
                    ConstructionYear = 2000,
                    Floors = 1,
                    SurfaceArea = 10,
                    InsuredValue = 100
                });

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Building not found for client.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenBrokerInactive_ThrowsForbidden()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 100m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(10)
            };

            _clients.Setup(c => c.ExistsByIdAsync(req.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _buildings.Setup(b => b.GetByIdWithGeographyAsync(req.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeBuilding(req.BuildingId, req.ClientId));

            _brokers.Setup(b => b.GetByIdAsync(req.BrokerId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Broker { Id = req.BrokerId, BrokerCode = "BRK", Name = "B", Status = BrokerStatus.Inactive });

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("Broker is inactive and cannot create policies.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenCurrencyInactive_ThrowsBusinessRule()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 100m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(10)
            };

            _clients.Setup(c => c.ExistsByIdAsync(req.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _buildings.Setup(b => b.GetByIdWithGeographyAsync(req.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeBuilding(req.BuildingId, req.ClientId));

            _brokers.Setup(b => b.GetByIdAsync(req.BrokerId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Broker { Id = req.BrokerId, BrokerCode = "BRK", Name = "B", Status = BrokerStatus.Active });

            _currencies.Setup(c => c.GetByIdAsync(req.CurrencyId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Currency { Id = req.CurrencyId, Code = "RON", Name = "RON", IsActive = false, ExchangeRateToBase = 1m });

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Currency is inactive and cannot be used for new policies.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenValid_AddsAndSaves_ReturnsId()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 100m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(10)
            };

            _clients.Setup(c => c.ExistsByIdAsync(req.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var building = MakeBuilding(req.BuildingId, req.ClientId);

            _buildings.Setup(b => b.GetByIdWithGeographyAsync(req.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            _brokers.Setup(b => b.GetByIdAsync(req.BrokerId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Broker { Id = req.BrokerId, BrokerCode = "BRK", Name = "B", Status = BrokerStatus.Active });

            _currencies.Setup(c => c.GetByIdAsync(req.CurrencyId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Currency { Id = req.CurrencyId, Code = "RON", Name = "RON", IsActive = true, ExchangeRateToBase = 1m });

            _calculator.Setup(x => x.CalculateFinalPremiumAsync(
                    req.BasePremium,
                    building.City.County.CountryId,
                    building.City.CountyId,
                    building.CityId,
                    building.BuildingType,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(123.45m);

            Policy? saved = null;
            _policies.Setup(p => p.AddAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
                .Callback<Policy, CancellationToken>((x, _) => saved = x)
                .Returns(Task.CompletedTask);

            _policies.Setup(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var id = await _sut.CreateDraftAsync(req);

            id.Should().NotBe(Guid.Empty);
            saved.Should().NotBeNull();
            saved!.Status.Should().Be(PolicyStatus.Draft);
            saved.FinalPremium.Should().Be(123.45m);

            _clients.VerifyAll();
            _buildings.VerifyAll();
            _brokers.VerifyAll();
            _currencies.VerifyAll();
            _calculator.VerifyAll();
            _policies.VerifyAll();
        }

        [Test]
        public async Task ActivateAsync_WhenNotFound_ThrowsNotFound()
        {
            _policies.Setup(p => p.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Policy?)null);

            var act = async () => await _sut.ActivateAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Policy not found.");
        }

        [Test]
        public async Task ActivateAsync_WhenNotDraft_ThrowsBusinessRule()
        {
            var policy = new Policy { Id = Guid.NewGuid(), Status = PolicyStatus.Active, StartDateUtc = DateTime.UtcNow.AddDays(1) };

            _policies.Setup(p => p.GetByIdAsync(policy.Id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            var act = async () => await _sut.ActivateAsync(policy.Id);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Only Draft policies can be activated.");
        }

        [Test]
        public async Task ActivateAsync_WhenStartDateInPast_ThrowsBusinessRule()
        {
            var policy = new Policy { Id = Guid.NewGuid(), Status = PolicyStatus.Draft, StartDateUtc = DateTime.UtcNow.AddDays(-1) };

            _policies.Setup(p => p.GetByIdAsync(policy.Id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            var act = async () => await _sut.ActivateAsync(policy.Id);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Start date cannot be in the past.");
        }

        [Test]
        public async Task ActivateAsync_WhenValid_SetsActiveAndSaves()
        {
            var policy = new Policy { Id = Guid.NewGuid(), Status = PolicyStatus.Draft, StartDateUtc = DateTime.UtcNow.AddDays(1), UpdatedAtUtc = DateTime.UtcNow.AddDays(-10) };

            _policies.Setup(p => p.GetByIdAsync(policy.Id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _policies.Setup(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _sut.ActivateAsync(policy.Id);

            policy.Status.Should().Be(PolicyStatus.Active);
            policy.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            _policies.VerifyAll();
        }

        [Test]
        public async Task CancelAsync_WhenReasonEmpty_ThrowsValidation()
        {
            var act = async () => await _sut.CancelAsync(Guid.NewGuid(), new CancelPolicyRequestDTO
            {
                Reason = " ",
                CancelledAtUtc = DateTime.UtcNow
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task CancelAsync_WhenCancelledAtDefault_ThrowsValidation()
        {
            var act = async () => await _sut.CancelAsync(Guid.NewGuid(), new CancelPolicyRequestDTO
            {
                Reason = "x",
                CancelledAtUtc = default
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("CancelledAtUtc is required.");
        }

        [Test]
        public async Task CancelAsync_WhenNotActive_ThrowsBusinessRule()
        {
            var policy = new Policy { Id = Guid.NewGuid(), Status = PolicyStatus.Draft };

            _policies.Setup(p => p.GetByIdAsync(policy.Id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            var act = async () => await _sut.CancelAsync(policy.Id, new CancelPolicyRequestDTO
            {
                Reason = "x",
                CancelledAtUtc = DateTime.UtcNow
            });

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Only Active policies can be cancelled.");
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_MapsAndReturns()
        {
            var policy = new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL-1" };
            var dto = new PolicyDetailsResponseDTO { Id = policy.Id, PolicyNumber = "POL-1" };

            _policies.Setup(p => p.GetByIdWithDetailsAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _mapper.Setup(m => m.Map<PolicyDetailsResponseDTO>(policy))
                .Returns(dto);

            var result = await _sut.GetByIdAsync(policy.Id);

            result.Id.Should().Be(policy.Id);
            _policies.VerifyAll();
            _mapper.VerifyAll();
        }

        private static Building MakeBuilding(Guid buildingId, Guid clientId)
        {
            return new Building
            {
                Id = buildingId,
                ClientId = clientId,
                CityId = 3,
                City = new City
                {
                    Id = 3,
                    Name = "City",
                    CountyId = 2,
                    County = new County
                    {
                        Id = 2,
                        Name = "County",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Country" }
                    }
                },
                Street = "S",
                Number = "1",
                ConstructionYear = 2000,
                BuildingType = BuildingType.Residential,
                Floors = 1,
                SurfaceArea = 10,
                InsuredValue = 100
            };
        }

        [Test]
        public async Task ActivateAsync_WhenPolicyNotDraft_ThrowsBusinessRule()
        {
            var id = Guid.NewGuid();
            _policies.Setup(r => r.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Policy { Id = id, Status = PolicyStatus.Active });

            var act = async () => await _sut.ActivateAsync(id);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Only Draft policies can be activated.");
        }

        [Test]
        public async Task CancelAsync_WhenReasonMissing_ThrowsValidation()
        {
            var act = async () => await _sut.CancelAsync(Guid.NewGuid(), new CancelPolicyRequestDTO
            {
                Reason = "   ",
                CancelledAtUtc = DateTime.UtcNow
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task CreateDraftAsync_WhenBuildingIdEmpty_ThrowsValidation()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.Empty,
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 10m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(2)
            };

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("BuildingId is required.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenBrokerIdEmpty_ThrowsValidation()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.Empty,
                CurrencyId = 1,
                BasePremium = 10m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(2)
            };

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("BrokerId is required.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenEndDateNotAfterStart_ThrowsValidation()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 10m,
                StartDateUtc = new DateTime(2026, 1, 10),
                EndDateUtc = new DateTime(2026, 1, 10)
            };

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("EndDateUtc must be after StartDateUtc.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenBrokerMissing_ThrowsNotFound()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 1,
                BasePremium = 100m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(10)
            };

            _clients.Setup(c => c.ExistsByIdAsync(req.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _buildings.Setup(b => b.GetByIdWithGeographyAsync(req.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeBuilding(req.BuildingId, req.ClientId));

            _brokers.Setup(b => b.GetByIdAsync(req.BrokerId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Broker?)null);

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Broker not found.");
        }

        [Test]
        public async Task CreateDraftAsync_WhenCurrencyMissing_ThrowsNotFound()
        {
            var req = new CreatePolicyRequestDTO
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = 9,
                BasePremium = 100m,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(10)
            };

            _clients.Setup(c => c.ExistsByIdAsync(req.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var building = MakeBuilding(req.BuildingId, req.ClientId);

            _buildings.Setup(b => b.GetByIdWithGeographyAsync(req.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            _brokers.Setup(b => b.GetByIdAsync(req.BrokerId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Broker { Id = req.BrokerId, Status = BrokerStatus.Active });

            _currencies.Setup(c => c.GetByIdAsync(req.CurrencyId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Currency?)null);

            var act = async () => await _sut.CreateDraftAsync(req);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Currency not found.");
        }

        [Test]
        public async Task CancelAsync_WhenPolicyMissing_ThrowsNotFound()
        {
            var id = Guid.NewGuid();

            _policies.Setup(p => p.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Policy?)null);

            var act = async () => await _sut.CancelAsync(id, new CancelPolicyRequestDTO
            {
                Reason = "x",
                CancelledAtUtc = DateTime.UtcNow
            });

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Policy not found.");
        }

        [Test]
        public async Task CancelAsync_WhenActive_UpdatesFieldsAndSaves()
        {
            var id = Guid.NewGuid();
            var policy = new Policy { Id = id, Status = PolicyStatus.Active, UpdatedAtUtc = DateTime.UtcNow.AddDays(-10) };

            _policies.Setup(p => p.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _policies.Setup(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var now = DateTime.UtcNow;

            await _sut.CancelAsync(id, new CancelPolicyRequestDTO
            {
                Reason = "  because  ",
                CancelledAtUtc = now
            });

            policy.Status.Should().Be(PolicyStatus.Cancelled);
            policy.CancelledAtUtc.Should().Be(now);
            policy.CancellationReason.Should().Be("because");
            policy.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            _policies.VerifyAll();
        }

        [Test]
        public async Task SearchAsync_NormalizesPageAndPageSize_MapsItems()
        {
            var p1 = new Policy { Id = Guid.NewGuid() };
            var p2 = new Policy { Id = Guid.NewGuid() };

            _policies.Setup(r => r.SearchAsync(null, null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Policy> { p1, p2 }, 2));

            _mapper.Setup(m => m.Map<PolicyListItemResponseDTO>(p1))
                .Returns(new PolicyListItemResponseDTO { Id = p1.Id });

            _mapper.Setup(m => m.Map<PolicyListItemResponseDTO>(p2))
                .Returns(new PolicyListItemResponseDTO { Id = p2.Id });

            var result = await _sut.SearchAsync(null, null, null, null, null, 0, 999);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);

            _policies.VerifyAll();
            _mapper.VerifyAll();
        }
    }
