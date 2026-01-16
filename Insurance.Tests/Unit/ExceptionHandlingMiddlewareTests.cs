using FluentAssertions;
using Insurance.Api.Middleware;
using Insurance.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace Insurance.Tests.Unit;

    [TestFixture]
    public class ExceptionHandlingMiddlewareTests
    {
        [Test]
        public async Task AppException_ReturnsExpectedStatusAndPayload()
        {
            var middleware = new ExceptionHandlingMiddleware(
                _ => throw new NotFoundException("Missing"),
                NullLogger<ExceptionHandlingMiddleware>.Instance);

            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            await middleware.Invoke(ctx);

            ctx.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            ctx.Response.Body.Position = 0;
            var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
            body.Should().Contain("NotFoundException").And.Contain("Missing");
        }

        [Test]
        public async Task UnknownException_Returns500()
        {
            var middleware = new ExceptionHandlingMiddleware(
                _ => throw new InvalidOperationException("boom"),
                NullLogger<ExceptionHandlingMiddleware>.Instance);

            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            await middleware.Invoke(ctx);

            ctx.Response.StatusCode.Should().Be(500);

            ctx.Response.Body.Position = 0;
            var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
            body.Should().Contain("InternalServerError");
        }
    }
