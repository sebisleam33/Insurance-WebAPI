using FluentAssertions;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Implementations;
using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class PolicyPremiumCalculatorServiceTests
    {
        private Mock<IFeeConfigurationRepository> _fees = null!;
        private Mock<IRiskFactorConfigurationRepository> _risk = null!;
        private Mock<ILogger<PolicyPremiumCalculatorService>> _logger = null!;
        private PolicyPremiumCalculatorService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _fees = new Mock<IFeeConfigurationRepository>(MockBehavior.Strict);
            _risk = new Mock<IRiskFactorConfigurationRepository>(MockBehavior.Strict);
            _logger = new Mock<ILogger<PolicyPremiumCalculatorService>>(MockBehavior.Loose);
            _sut = new PolicyPremiumCalculatorService(_fees.Object, _risk.Object, _logger.Object);
        }

        [Test]
        public async Task CalculateFinalPremiumAsync_WhenBaseNonPositive_ThrowsValidation()
        {
            var act = async () => await _sut.CalculateFinalPremiumAsync(0m, 1, 1, 1, BuildingType.Residential);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task CalculateFinalPremiumAsync_WhenResultRoundsToZero_ThrowsBusinessRule()
        {
            _fees.Setup(f => f.GetActiveForNowAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new FeeConfiguration { Percentage = -0.99m, IsActive = true } });

            _risk.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<RiskFactorConfiguration>());

            var act = async () => await _sut.CalculateFinalPremiumAsync(0.01m, 1, 1, 1, BuildingType.Residential);

            await act.Should().ThrowAsync<BusinessRuleException>()
                .WithMessage("Calculated final premium must be > 0.");

            _fees.VerifyAll();
            _risk.VerifyAll();
        }

        [Test]
        public async Task CalculateFinalPremiumAsync_WhenMatchingRiskFactors_AddsPercentagesAndRounds()
        {
            _fees.Setup(f => f.GetActiveForNowAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new FeeConfiguration { Percentage = 0.10m, IsActive = true } });

            _risk.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new RiskFactorConfiguration { Level = RiskFactorLevel.Country, ReferenceId = 1, AdjustmentPercentage = 0.05m, IsActive = true },
                    new RiskFactorConfiguration { Level = RiskFactorLevel.County, ReferenceId = 2, AdjustmentPercentage = 0.02m, IsActive = true },
                    new RiskFactorConfiguration { Level = RiskFactorLevel.City, ReferenceId = 3, AdjustmentPercentage = 0.01m, IsActive = true },
                    new RiskFactorConfiguration { Level = RiskFactorLevel.BuildingType, BuildingType = BuildingType.Residential, AdjustmentPercentage = 0.03m, IsActive = true },

                    new RiskFactorConfiguration { Level = RiskFactorLevel.City, ReferenceId = 999, AdjustmentPercentage = 9m, IsActive = true }
                });

            var final = await _sut.CalculateFinalPremiumAsync(100m, 1, 2, 3, BuildingType.Residential);

            final.Should().Be(121.00m);

            _fees.VerifyAll();
            _risk.VerifyAll();
        }
    }
