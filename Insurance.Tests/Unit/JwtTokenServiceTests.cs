using Insurance.Application.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Insurance.Tests.Unit;

[TestFixture]
public sealed class JwtTokenServiceTests
    {
        private const string Issuer = "Insurance.Api";
        private const string Audience = "Insurance.Api";
        private const string Key = "THIS_IS_A_UNIT_TEST_KEY_32+_CHARS_LONG__CHANGE_ME";

        private static IConfiguration BuildConfig(
            string? issuer = Issuer,
            string? audience = Audience,
            string? key = Key,
            string? expiresMinutes = "15")
        {
            var dict = new Dictionary<string, string?>();

            if (issuer is not null) dict["Jwt:Issuer"] = issuer;
            if (audience is not null) dict["Jwt:Audience"] = audience;
            if (key is not null) dict["Jwt:Key"] = key;
            if (expiresMinutes is not null) dict["Jwt:ExpiresMinutes"] = expiresMinutes;

            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
        }

        private static List<Claim> SampleClaims() => new()
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "admin@local"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        [Test]
        public void CreateToken_WhenKeyMissing_ThrowsInvalidOperationException()
        {
            var cfg = BuildConfig(key: null);
            var sut = new JwtTokenService(cfg);

            var ex = Assert.Throws<InvalidOperationException>(() => sut.CreateToken(SampleClaims()));

            Assert.That(ex!.Message, Does.Contain("Jwt:Key is missing"));
        }

        [Test]
        public void CreateToken_WhenExpiresMinutesIsValid_UsesConfiguredExpiration_AndIncludesClaims()
        {
            var cfg = BuildConfig(expiresMinutes: "7");
            var sut = new JwtTokenService(cfg);

            var claims = SampleClaims();

            var before = DateTime.UtcNow;

            var tokenString = sut.CreateToken(claims);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenString);

            Assert.Multiple(() =>
            {
                Assert.That(jwt.Issuer, Is.EqualTo(Issuer));
                Assert.That(jwt.Audiences, Does.Contain(Audience));

                Assert.That(jwt.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1"), Is.True);
                Assert.That(jwt.Claims.Any(c => c.Type == ClaimTypes.Name && c.Value == "admin@local"), Is.True);
                Assert.That(jwt.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"), Is.True);

                Assert.That(jwt.Header.Alg, Is.EqualTo(SecurityAlgorithms.HmacSha256));
            });

            var expectedMin = before.AddMinutes(7).AddSeconds(-10);
            var expectedMax = before.AddMinutes(7).AddSeconds(+10);

            Assert.That(jwt.ValidTo, Is.InRange(expectedMin, expectedMax));
            Assert.That(jwt.ValidFrom, Is.LessThanOrEqualTo(DateTime.UtcNow.AddSeconds(10)));
        }

        [Test]
        public void CreateToken_WhenExpiresMinutesIsNotAnInt_DefaultsTo60Minutes()
        {
            var cfg = BuildConfig(expiresMinutes: "not-a-number");
            var sut = new JwtTokenService(cfg);

            var before = DateTime.UtcNow;

            var tokenString = sut.CreateToken(SampleClaims());

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenString);

            var expectedMin = before.AddMinutes(60).AddSeconds(-10);
            var expectedMax = before.AddMinutes(60).AddSeconds(+10);

            Assert.That(jwt.ValidTo, Is.InRange(expectedMin, expectedMax));
        }

        [Test]
        public void CreateToken_TokenCanBeValidatedWithSameKey_Issuer_AndAudience()
        {
            var cfg = BuildConfig(expiresMinutes: "5");
            var sut = new JwtTokenService(cfg);

            var tokenString = sut.CreateToken(SampleClaims());

            var handler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var principal = handler.ValidateToken(tokenString, parameters, out var validatedToken);

            Assert.Multiple(() =>
            {
                Assert.That(validatedToken, Is.Not.Null);
                Assert.That(principal.Identity?.IsAuthenticated, Is.True);

                Assert.That(principal.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"), Is.True);
                Assert.That(principal.Claims.Any(c => c.Type == ClaimTypes.Name && c.Value == "admin@local"), Is.True);
            });
        }
    }
