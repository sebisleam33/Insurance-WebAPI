using FluentAssertions;
using Insurance.Application.Common.Exceptions;

namespace Insurance.Tests.Unit.Exceptions;

[TestFixture]
public sealed class ExceptionsTests
{
    [Test]
    public void BusinessRuleException_Has422StatusCode_AndDetails()
    {
        var ex = new BusinessRuleException("rule", new { A = 1 });

        ex.StatusCode.Should().Be(422);
        ex.Message.Should().Be("rule");
        ex.Details.Should().NotBeNull();
        ex.ErrorCode.Should().Be(nameof(BusinessRuleException));
    }

    [Test]
    public void ForbiddenException_Has403StatusCode()
    {
        var ex = new ForbiddenException("nope");
        ex.StatusCode.Should().Be(403);
        ex.ErrorCode.Should().Be(nameof(ForbiddenException));
    }

    [Test]
    public void UnauthorizedException_Has401StatusCode()
    {
        var ex = new UnauthorizedException("login");
        ex.StatusCode.Should().Be(401);
        ex.ErrorCode.Should().Be(nameof(UnauthorizedException));
    }
}
