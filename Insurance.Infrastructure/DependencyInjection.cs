using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Implementations;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insurance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var cs = configuration.GetConnectionString("DefaultConnection")
                     ?? "Data Source=insurance.db";

            options.UseSqlite(cs);
        });

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IGeographyRepository, GeographyRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IFeeConfigurationRepository, FeeConfigurationRepository>();
        services.AddScoped<IRiskFactorConfigurationRepository, RiskFactorConfigurationRepository>();
        services.AddScoped<IBrokerRepository, BrokerRepository>();

        return services;
    }
}
