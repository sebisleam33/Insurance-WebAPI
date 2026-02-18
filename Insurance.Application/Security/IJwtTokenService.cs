using System.Security.Claims;

namespace Insurance.Application.Security;

public interface IJwtTokenService
    {
        string CreateToken(IEnumerable<Claim> claims);
    }
