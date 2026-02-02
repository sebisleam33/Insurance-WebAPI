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
    }
