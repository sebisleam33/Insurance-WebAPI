using Insurance.Api.Mapping;
using Insurance.Api.Middleware;
using Insurance.Application.Services.Implementations;
using Insurance.Application.Services.Interfaces;
using Insurance.Infrastructure.Persistence;
using Insurance.Infrastructure.Repositories.Implementations;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Insurance Backend API", Version = "v1" });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? "Data Source=insurance.db";
    options.UseSqlite(cs);
});

builder.Services.AddAutoMapper(cfg => { }, typeof(ApiMappingProfile));

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IGeographyRepository, GeographyRepository>();

builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IGeographyService, GeographyService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

app.MapControllers();
app.Run();

public partial class Program { }
