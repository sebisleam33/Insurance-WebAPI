using Insurance.Application.Security;
using Insurance.Domain;
using Insurance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Insurance.Api.Controllers.Auth;

[ApiController]
[Route("api/auth/admin")]
public sealed class AdminAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AdminAuthController(AppDbContext db, IJwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public sealed record LoginRequest(string Email, string Password);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var admin = await _db.AdminUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == req.Email && x.IsActive, ct);

            if (admin is null)
            {
                return Unauthorized("Invalid credentials.");
            }

            if (!PasswordHasher.Verify(req.Password, admin.PasswordHash, admin.PasswordSalt))
            {
                return Unauthorized("Invalid credentials.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new(ClaimTypes.Name, admin.Email),
                new(ClaimTypes.Role, "Admin")
            };

            var token = _jwt.CreateToken(claims);
            return Ok(new { accessToken = token });
        }
    }
