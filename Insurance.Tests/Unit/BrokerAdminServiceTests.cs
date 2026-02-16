using AutoMapper;
using FluentAssertions;
using Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class BrokerAdminServiceTests
    {
        private Mock<IBrokerRepository> _brokers = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<ILogger<BrokerAdminService>> _logger = null!;
        private BrokerAdminService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _brokers = new Mock<IBrokerRepository>(MockBehavior.Loose);
            _mapper = new Mock<IMapper>(MockBehavior.Loose);
            _logger = new Mock<ILogger<BrokerAdminService>>();
            _sut = new BrokerAdminService(_brokers.Object, _mapper.Object, _logger.Object);
        }

        [Test]
        public async Task CreateAsync_WhenEmailInvalid_ThrowsValidation_AndDoesNotCallRepo()
        {
            var req = new CreateBrokerRequestDTO
            {
                BrokerCode = "brk-002",
                Name = "X",
                Email = "NOT_AN_EMAIL"
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>();

            _brokers.Verify(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _brokers.Verify(r => r.AddAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()), Times.Never);
            _brokers.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenDuplicateCode_ThrowsConflict()
        {
            var req = new CreateBrokerRequestDTO
            {
                BrokerCode = "brk-002",
                Name = "X",
                Email = "x@local.com",
            };

            _brokers.Setup(r => r.ExistsByCodeAsync("BRK-002", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ConflictException>();

            _brokers.Verify(r => r.AddAsync(It.IsAny<Broker>(), It.IsAny<CancellationToken>()), Times.Never);
            _brokers.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_WhenNotFound_ThrowsNotFound()
        {
            _brokers.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Broker?)null);

            var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new UpdateBrokerRequestDTO
            {
                Name = "N",
                Email = "n@local.com"
            });

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task ActivateAsync_WhenNotFound_ThrowsNotFound()
        {
            _brokers.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Broker?)null);

            var act = async () => await _sut.ActivateAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task DeactivateAsync_WhenNotFound_ThrowsNotFound()
        {
            _brokers.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Broker?)null);

            var act = async () => await _sut.DeactivateAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task ListAsync_NormalizesPageAndPageSize_MapsItems()
        {
            var brokers = new List<Broker>
                {
                    new() { Id = Guid.NewGuid() },
                    new() { Id = Guid.NewGuid() }
                };

            _brokers.Setup(r => r.ListAsync(1, 20, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((brokers, brokers.Count));

            _mapper.Setup(m => m.Map<BrokerResponseDTO>(brokers[0]))
                   .Returns(new BrokerResponseDTO());
            _mapper.Setup(m => m.Map<BrokerResponseDTO>(brokers[1]))
                   .Returns(new BrokerResponseDTO());

            var result = await _sut.ListAsync(0, 999);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);

            _brokers.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
        {
            var id = Guid.NewGuid();

            _brokers.Setup(r => r.GetByIdAsync(id, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Broker?)null);

            var act = async () => await _sut.GetByIdAsync(id);

            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Broker not found.");

            _brokers.VerifyAll();
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_MapsAndReturns()
        {
            var id = Guid.NewGuid();
            var entity = new Broker { Id = id };

            _brokers.Setup(r => r.GetByIdAsync(id, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entity);

            _mapper.Setup(m => m.Map<BrokerResponseDTO>(entity))
                   .Returns(new BrokerResponseDTO { Id = id });

            var result = await _sut.GetByIdAsync(id);

            result.Id.Should().Be(id);

            _brokers.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_WhenValid_NormalizesCode_SetsStatus_Saves()
        {
            var req = new CreateBrokerRequestDTO
            {
                BrokerCode = " brk-001 ",
                Name = "  Test Broker ",
                Email = "test@local.com",
                CommissionPercentage = 12.5m
            };

            var mapped = new Broker { Id = Guid.NewGuid() };

            _brokers.Setup(r => r.ExistsByCodeAsync("BRK-001", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            _mapper.Setup(m => m.Map<Broker>(It.IsAny<CreateBrokerRequestDTO>()))
                   .Returns(mapped);

            _brokers.Setup(r => r.AddAsync(mapped, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            _brokers.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var id = await _sut.CreateAsync(req);

            id.Should().Be(mapped.Id);
            mapped.BrokerCode.Should().Be("BRK-001");
            mapped.Status.Should().Be(BrokerStatus.Active);

            _brokers.VerifyAll();
            _mapper.VerifyAll();
        }


        [Test]
        public async Task UpdateAsync_WhenEmailInvalid_ThrowsValidation_AndDoesNotHitRepo()
        {
            var req = new UpdateBrokerRequestDTO
            {
                Name = "Name",
                Email = "NOT_AN_EMAIL"
            };

            var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), req);

            await act.Should().ThrowAsync<ValidationException>();

            _brokers.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()),
                            Times.Never);
            _brokers.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                            Times.Never);
        }

        [Test]
        public async Task UpdateAsync_WhenValid_MapsAndSaves()
        {
            var id = Guid.NewGuid();
            var req = new UpdateBrokerRequestDTO
            {
                Name = "  New Name ",
                Email = "new@local.com"
            };

            var entity = new Broker { Id = id };

            _brokers.Setup(r => r.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entity);

            _mapper.Setup(m => m.Map(req, entity));

            _brokers.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            await _sut.UpdateAsync(id, req);

            _brokers.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task ActivateAsync_WhenFound_SetsStatusActiveAndSaves()
        {
            var id = Guid.NewGuid();
            var entity = new Broker { Id = id, Status = BrokerStatus.Inactive };

            _brokers.Setup(r => r.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entity);

            _brokers.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            await _sut.ActivateAsync(id);

            entity.Status.Should().Be(BrokerStatus.Active);

            _brokers.VerifyAll();
        }

        [Test]
        public async Task DeactivateAsync_WhenFound_SetsStatusInactiveAndSaves()
        {
            var id = Guid.NewGuid();
            var entity = new Broker { Id = id, Status = BrokerStatus.Active };

            _brokers.Setup(r => r.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entity);

            _brokers.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            await _sut.DeactivateAsync(id);

            entity.Status.Should().Be(BrokerStatus.Inactive);

            _brokers.VerifyAll();
        }
    }
