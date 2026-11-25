using Insurance.Domain.Enums;

namespace Insurance.Application.Services.Interfaces;

    public interface IPolicyPremiumCalculatorService
    {
        Task<decimal> CalculateFinalPremiumAsync(
            decimal basePremium, int countryId, int countyId, int cityId, 
            BuildingType buildingType, CancellationToken ct = default);
    }
