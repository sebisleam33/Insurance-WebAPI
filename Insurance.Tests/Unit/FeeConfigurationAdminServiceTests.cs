using AutoMapper;
using FluentAssertions;
using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Domain.Entities.Metadata;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class FeeConfigurationAdminServiceTests
    {
        private Mock<IFeeConfigurationRepository> _fees = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<ILogger<FeeConfigurationAdminService>> _logger = null!;
        private FeeConfigurationAdminService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _fees = new Mock<IFeeConfigurationRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _logger = new Mock<ILogger<FeeConfigurationAdminService>>();
            _sut = new FeeConfigurationAdminService(_fees.Object, _mapper.Object, _logger.Object);
        }

        [Test]
        public async Task ListAsync_NormalizesPageAndPageSize_MapsItems()
        {
            var items = new List<FeeConfiguration>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" }
            };

            _fees.Setup(r => r.ListAsync(1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 2));

            _mapper.Setup(m => m.Map<FeeConfigurationResponseDTO>(items[0]))
                .Returns(new FeeConfigurationResponseDTO { Id = 1, Name = "A" });

            _mapper.Setup(m => m.Map<FeeConfigurationResponseDTO>(items[1]))
                .Returns(new FeeConfigurationResponseDTO { Id = 2, Name = "B" });

            var result = await _sut.ListAsync(page: 0, pageSize: 999);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);

            _fees.VerifyAll();
            _mapper.VerifyAll();
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
        {
            _fees.Setup(r => r.GetByIdAsync(123, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FeeConfiguration?)null);

            var act = async () => await _sut.GetByIdAsync(123);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Fee configuration not found.");

            _fees.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_WhenEffectiveToBeforeFrom_ThrowsValidation()
        {
            var req = new CreateFeeConfigurationRequestDTO
            {
                Name = "Fee",
                Percentage = 0.1m,
                EffectiveFromUtc = new DateTime(2025, 1, 2),
                EffectiveToUtc = new DateTime(2025, 1, 1)
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("EffectiveToUtc must be after EffectiveFromUtc.");

            _fees.Verify(r => r.AddAsync(It.IsAny<FeeConfiguration>(), It.IsAny<CancellationToken>()), Times.Never);
            _fees.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mapper.Verify(m => m.Map<FeeConfiguration>(It.IsAny<CreateFeeConfigurationRequestDTO>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenValid_MapsAddsSavesAndReturnsId()
        {
            var req = new CreateFeeConfigurationRequestDTO
            {
                Name = "Fee",
                Percentage = 0.1m,
                EffectiveFromUtc = new DateTime(2025, 1, 1),
                EffectiveToUtc = new DateTime(2026, 1, 1),
                IsActive = true
            };

            var entity = new FeeConfiguration { Id = 77, Name = "Fee" };

            _mapper.Setup(m => m.Map<FeeConfiguration>(req)).Returns(entity);

            _fees.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _fees.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var id = await _sut.CreateAsync(req);

            id.Should().Be(77);
            _mapper.VerifyAll();
            _fees.VerifyAll();
        }

        [Test]
        public async Task UpdateAsync_WhenMissing_ThrowsNotFound()
        {
            _fees.Setup(r => r.GetByIdAsync(5, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FeeConfiguration?)null);

            var act = async () => await _sut.UpdateAsync(5, new UpdateFeeConfigurationRequestDTO
            {
                Percentage = 0.1m,
                EffectiveFromUtc = DateTime.UtcNow,
                EffectiveToUtc = null,
                IsActive = true
            });

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Fee configuration not found.");

            _fees.VerifyAll();
        }

        [Test]
        public async Task UpdateAsync_WhenEffectiveToBeforeFrom_ThrowsValidation()
        {
            var act = async () => await _sut.UpdateAsync(5, new UpdateFeeConfigurationRequestDTO
            {
                Percentage = 0.1m,
                EffectiveFromUtc = new DateTime(2025, 1, 2),
                EffectiveToUtc = new DateTime(2025, 1, 1),
                IsActive = true
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("EffectiveToUtc must be after EffectiveFromUtc.");

            _fees.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            _fees.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_WhenFound_UpdatesFieldsAndSaves()
        {
            var entity = new FeeConfiguration
            {
                Id = 5,
                Percentage = 0.01m,
                EffectiveFromUtc = new DateTime(2020, 1, 1),
                EffectiveToUtc = null,
                IsActive = true
            };

            _fees.Setup(r => r.GetByIdAsync(5, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _fees.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var req = new UpdateFeeConfigurationRequestDTO
            {
                Percentage = 0.25m,
                EffectiveFromUtc = new DateTime(2025, 1, 1),
                EffectiveToUtc = new DateTime(2026, 1, 1),
                IsActive = false
            };

            await _sut.UpdateAsync(5, req);

            entity.Percentage.Should().Be(0.25m);
            entity.EffectiveFromUtc.Should().Be(req.EffectiveFromUtc);
            entity.EffectiveToUtc.Should().Be(req.EffectiveToUtc);
            entity.IsActive.Should().BeFalse();

            _fees.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_WhenPercentageOutOfRange_ThrowsValidation_AndNoRepoCalls()
        {
            var req = new CreateFeeConfigurationRequestDTO
            {
                Name = "Fee",
                Percentage = 10m, // invalid > 5
                EffectiveFromUtc = new DateTime(2025, 1, 1)
            };

            var act = async () => await _sut.CreateAsync(req);

            await act.Should().ThrowAsync<ValidationException>();

            _fees.Verify(r => r.AddAsync(It.IsAny<FeeConfiguration>(), It.IsAny<CancellationToken>()), Times.Never);
            _fees.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_WhenFound_MapsAndReturns()
        {
            var entity = new FeeConfiguration { Id = 9, Name = "X" };

            _fees.Setup(r => r.GetByIdAsync(9, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapper.Setup(m => m.Map<FeeConfigurationResponseDTO>(entity))
                .Returns(new FeeConfigurationResponseDTO { Id = 9, Name = "X" });

            var result = await _sut.GetByIdAsync(9);

            result.Id.Should().Be(9);
            result.Name.Should().Be("X");
        }

        [Test]
        public async Task UpdateAsync_WhenPercentageOutOfRange_ThrowsValidation_AndNoRepoCalls()
        {
            var act = async () => await _sut.UpdateAsync(1, new UpdateFeeConfigurationRequestDTO
            {
                Percentage = -2m, // invalid < -0.99
                EffectiveFromUtc = DateTime.UtcNow,
                IsActive = true
            });

            await act.Should().ThrowAsync<ValidationException>();

            _fees.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            _fees.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
