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
            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IPolicyPremiumCalculatorService, PolicyPremiumCalculatorService>();
            services.AddScoped<ICurrencyAdminService, CurrencyAdminService>();
            services.AddScoped<IBrokerAdminService, BrokerAdminService>();
            services.AddScoped<IFeeConfigurationAdminService, FeeConfigurationAdminService>();
            services.AddScoped<IRiskFactorAdminService, RiskFactorAdminService>();
            services.AddScoped<IPolicyReportsService, PolicyReportsService>();

            return services;
        }
    }
