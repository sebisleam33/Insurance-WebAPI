using Insurance.Application.Abstractions;
using Insurance.Application.Buildings;
using Insurance.Application.Clients;
using Insurance.Application.Geography;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Insurance.Infrastructure;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                                       ?? "Data Source=insurance.db";

                options.UseSqlite(connectionString);
            });

            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IBuildingService, BuildingService>();
            services.AddScoped<IGeographyService, GeographyService>();

            return services;
        }
    }
