using Insurance.Application.Services.Interfaces;
using Insurance.Infrastructure.Repositories.Interfaces;
using Insurance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

    public sealed class PolicyPremiumCalculatorService : IPolicyPremiumCalculatorService
    {
        private readonly IFeeConfigurationRepository _fees;
        private readonly IRiskFactorConfigurationRepository _risk;
        private readonly ILogger<PolicyPremiumCalculatorService> _logger;

        public PolicyPremiumCalculatorService(
            IFeeConfigurationRepository fees,
            IRiskFactorConfigurationRepository risk,
            ILogger<PolicyPremiumCalculatorService> logger)
        {
            _fees = fees;
            _risk = risk;
            _logger = logger;
        }

        public async Task<decimal> CalculateFinalPremiumAsync(
            decimal basePremium,
            int countryId,
            int countyId,
            int cityId,
            BuildingType buildingType,
            CancellationToken ct = default)
        {
            if (basePremium <= 0) return basePremium;

            var now = DateTime.UtcNow;

            var fees = await _fees.GetActiveForNowAsync(now, ct);
            var risks = await _risk.GetActiveAsync(ct);

            decimal sum = 0m;

            foreach (var f in fees)
                sum += f.Percentage;

            foreach (var r in risks)
            {
                switch (r.Level)
                {
                    case RiskFactorLevel.Country when r.ReferenceId == countryId:
                        sum += r.AdjustmentPercentage;
                        break;

                    case RiskFactorLevel.County when r.ReferenceId == countyId:
                        sum += r.AdjustmentPercentage;
                        break;

                    case RiskFactorLevel.City when r.ReferenceId == cityId:
                        sum += r.AdjustmentPercentage;
                        break;

                    case RiskFactorLevel.BuildingType when r.BuildingType == buildingType:
                        sum += r.AdjustmentPercentage;
                        break;
                }
            }

            var final = basePremium * (1 + sum);
            final = Math.Round(final, 2, MidpointRounding.AwayFromZero);

            _logger.LogInformation("Premium calc base={Base} sum={Sum} final={Final}", basePremium, sum, final);
            return final;
        }
    }
