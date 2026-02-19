using Insurance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Insurance.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        private const string TestJwtKey = "THIS_IS_A_TEST_KEY_32+_CHARS_LONG__CHANGE_ME";
        public string JwtIssuer => "Insurance.Api";
        public string JwtAudience => "Insurance.Api";
        public string JwtKey => TestJwtKey;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                ReplaceDbContextWithInMemorySqlite(services);
                ForceJwtBearerValidationForTests(services);

                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
            });
        }

        private void ReplaceDbContextWithInMemorySqlite(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        }

        private void ForceJwtBearerValidationForTests(IServiceCollection services)
        {
            services.PostConfigureAll<JwtBearerOptions>(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = JwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connection?.Dispose();
                _connection = null;
            }
        }
    }
