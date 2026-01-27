using Insurance.Application.Services.Implementations;
using Insurance.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Insurance.Application;

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IBuildingService, BuildingService>();
            services.AddScoped<IGeographyService, GeographyService>();
            //services.AddScoped<IPolicyPremiumCalculator, PolicyPremiumCalculator>();
            //services.AddScoped<IPolicyService, PolicyService>();

            //services.AddScoped<ICurrencyService, CurrencyService>();
            //services.AddScoped<IFeeConfigurationService, FeeConfigurationService>();
            //services.AddScoped<IRiskFactorConfigurationService, RiskFactorConfigurationService>();
            //services.AddScoped<IBrokerService, BrokerService>();

        return services;
        }
    }
