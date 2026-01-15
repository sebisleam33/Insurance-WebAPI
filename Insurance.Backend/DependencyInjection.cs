using Insurance.Application.Services.Implementations;
using Insurance.Application.Services.Interfaces;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IBuildingService, BuildingService>();
            services.AddScoped<IGeographyService, GeographyService>();

            return services;
        }
    }
