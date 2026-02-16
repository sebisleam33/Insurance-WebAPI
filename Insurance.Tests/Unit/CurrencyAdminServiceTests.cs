using AutoMapper;
using FluentAssertions;
using Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Domain.Entities.Metadata;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class CurrencyAdminServiceTests
    {
        private Mock<ICurrencyRepository> _repo = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<ILogger<CurrencyAdminService>> _logger = null!;
        private CurrencyAdminService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _repo = new Mock<ICurrencyRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Loose);
            _logger = new Mock<ILogger<CurrencyAdminService>>(MockBehavior.Loose);

            _sut = new CurrencyAdminService(_repo.Object, _mapper.Object, _logger.Object);
        }

        [Test]
        public void CreateAsync_WhenExchangeRateNonPositive_ThrowsValidation()
        {
            var act = async () => await _sut.CreateAsync(new CreateCurrencyRequestDTO
            {
                Code = "USD",
                Name = "US Dollar",
                ExchangeRateToBase = 0,
                IsActive = true
            });

            act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*ExchangeRateToBase*");
        }

        [Test]
        public void CreateAsync_WhenDuplicateCode_ThrowsConflict()
        {
            _repo.Setup(r => r.ExistsByCodeAsync("USD", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            var act = async () => await _sut.CreateAsync(new CreateCurrencyRequestDTO
            {
                Code = " usd ",
                Name = "US Dollar",
                ExchangeRateToBase = 4.5m,
                IsActive = true
            });

            act.Should().ThrowAsync<ConflictException>();

            _repo.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_WhenValid_PersistsAndReturnsId()
        {
            _repo.Setup(r => r.ExistsByCodeAsync("USD", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

            Currency? captured = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                 .Callback<Currency, CancellationToken>((c, _) => captured = c)
                 .Returns(Task.CompletedTask);

            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            var id = await _sut.CreateAsync(new CreateCurrencyRequestDTO
            {
                Code = " usd ",
                Name = " US Dollar ",
                ExchangeRateToBase = 4.6m,
                IsActive = true
            });

            captured.Should().NotBeNull();
            captured!.Code.Should().Be("USD");
            captured.Name.Should().Be("US Dollar");
            captured.ExchangeRateToBase.Should().Be(4.6m);
            captured.IsActive.Should().BeTrue();

            id.Should().Be(captured.Id);

            _repo.VerifyAll();
        }

        [Test]
        public void UpdateAsync_WhenNotFound_ThrowsNotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(10, true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Currency?)null);

            var act = async () => await _sut.UpdateAsync(10, new UpdateCurrencyRequestDTO
            {
                Name = "EUR",
                ExchangeRateToBase = 5m,
                IsActive = true
            });

            act.Should().ThrowAsync<NotFoundException>();

            _repo.VerifyAll();
        }

        [Test]
        public void UpdateAsync_WhenDeactivateUsedInActivePolicies_ThrowsBusinessRule()
        {
            _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Currency
                 {
                     Id = 1,
                     Code = "USD",
                     Name = "US Dollar",
                     ExchangeRateToBase = 4.6m,
                     IsActive = true
                 });

            _repo.Setup(r => r.IsUsedInActivePoliciesAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            var act = async () => await _sut.UpdateAsync(1, new UpdateCurrencyRequestDTO
            {
                Name = "US Dollar",
                ExchangeRateToBase = 4.7m,
                IsActive = false
            });

            act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("*Cannot deactivate*");

            _repo.VerifyAll();
        }

        [Test]
        public async Task UpdateAsync_WhenValid_UpdatesAndSaves()
        {
            var currency = new Currency
            {
                Id = 1,
                Code = "USD",
                Name = "Old",
                ExchangeRateToBase = 4.0m,
                IsActive = true
            };

            _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(currency);

            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await _sut.UpdateAsync(1, new UpdateCurrencyRequestDTO
            {
                Name = " New Name ",
                ExchangeRateToBase = 4.9m,
                IsActive = true
            });

            currency.Name.Should().Be("New Name");
            currency.ExchangeRateToBase.Should().Be(4.9m);
            currency.IsActive.Should().BeTrue();

            _repo.VerifyAll();
        }

        [Test]
        public async Task GetByIdAsync_WhenMissing_ThrowsNotFound_WithExactMessage()
        {
            _repo.Setup(r => r.GetByIdAsync(10, false, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Currency?)null);

            var act = async () => await _sut.GetByIdAsync(10);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Currency not found.");

            _repo.VerifyAll();
        }

        [Test]
        public async Task CreateAsync_WhenCodeEmpty_ThrowsValidation_WithExactMessage()
        {
            var act = async () => await _sut.CreateAsync(new CreateCurrencyRequestDTO
            {
                Code = "   ",
                Name = "US Dollar",
                ExchangeRateToBase = 1.2m,
                IsActive = true
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Code is required.");

            _repo.Verify(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenNameEmpty_ThrowsValidation_WithExactMessage()
        {
            var act = async () => await _sut.CreateAsync(new CreateCurrencyRequestDTO
            {
                Code = "USD",
                Name = "",
                ExchangeRateToBase = 1.2m,
                IsActive = true
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Name is required.");

            _repo.Verify(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenExchangeRateNonPositive_ThrowsValidation_WithExactMessage()
        {
            var act = async () => await _sut.CreateAsync(new CreateCurrencyRequestDTO
            {
                Code = "USD",
                Name = "US Dollar",
                ExchangeRateToBase = 0m,
                IsActive = true
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ExchangeRateToBase must be positive.");

            _repo.Verify(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenDuplicateCode_ThrowsConflict_WithExactMessage()
        {
            _repo.Setup(r => r.ExistsByCodeAsync("USD", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            var act = async () => await _sut.CreateAsync(new CreateCurrencyRequestDTO
            {
                Code = " usd ",
                Name = "US Dollar",
                ExchangeRateToBase = 4.5m,
                IsActive = true
            });

            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("A currency with the same code already exists.");

            _repo.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            _repo.VerifyAll();
        }

        [Test]
        public async Task UpdateAsync_WhenNameEmpty_ThrowsValidation_AndDoesNotHitRepo()
        {
            var act = async () => await _sut.UpdateAsync(1, new UpdateCurrencyRequestDTO
            {
                Name = " ",
                ExchangeRateToBase = 1.1m,
                IsActive = true
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Name is required.");

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), true, It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_WhenExchangeRateNonPositive_ThrowsValidation_WithExactMessage_AndDoesNotHitRepo()
        {
            var act = async () => await _sut.UpdateAsync(1, new UpdateCurrencyRequestDTO
            {
                Name = "EUR",
                ExchangeRateToBase = 0m,
                IsActive = true
            });

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("ExchangeRateToBase must be positive.");

            _repo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), true, It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_WhenDeactivatingAndUsed_ThrowsBusinessRule_WithExactMessage()
        {
            var entity = new Currency
            {
                Id = 1,
                Code = "USD",
                Name = "US Dollar",
                ExchangeRateToBase = 1.2m,
                IsActive = true
            };

            _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

            _repo.Setup(r => r.IsUsedInActivePoliciesAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            var act = async () => await _sut.UpdateAsync(1, new UpdateCurrencyRequestDTO
            {
                Name = "US Dollar",
                ExchangeRateToBase = 1.3m,
                IsActive = false
            });

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Cannot deactivate a currency that is used in active policies.");

            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _repo.VerifyAll();
        }

        [Test]
        public async Task UpdateAsync_WhenEntityAlreadyInactive_DoesNotCheckPolicyUsage()
        {
            var entity = new Currency
            {
                Id = 1,
                Code = "USD",
                Name = "US Dollar",
                ExchangeRateToBase = 1.2m,
                IsActive = false
            };

            _repo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await _sut.UpdateAsync(1, new UpdateCurrencyRequestDTO
            {
                Name = " Updated ",
                ExchangeRateToBase = 2.0m,
                IsActive = false
            });

            entity.Name.Should().Be("Updated");

            _repo.Verify(r => r.IsUsedInActivePoliciesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.VerifyAll();
        }
    }
