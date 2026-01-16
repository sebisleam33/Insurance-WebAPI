using Microsoft.AspNetCore.Http;

namespace Insurance.Application.Common.Exceptions
{
    public sealed class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message) { }

        public override int StatusCode => StatusCodes.Status401Unauthorized;
    }
}
