using FluentAssertions;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;

namespace Insurance.Tests.Unit.Validation;

    [TestFixture]
    public sealed class ValidationHelpersTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void RequireNotEmpty_Empty_Throws(string? value)
        {
            Action act = () => ValidationHelpers.RequireNotEmpty(value, "Field");
            act.Should().Throw<ValidationException>().WithMessage("Field is required.");
        }

        [Test]
        public void RequireNotEmpty_Valid_DoesNotThrow()
        {
            Action act = () => ValidationHelpers.RequireNotEmpty("x", "Field");
            act.Should().NotThrow();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void RequirePositive_ZeroOrNegative_Throws(decimal value)
        {
            Action act = () => ValidationHelpers.RequirePositive(value, "Amount");
            act.Should().Throw<ValidationException>().WithMessage("Amount must be > 0.");
        }

        [Test]
        public void RequirePositive_Positive_DoesNotThrow()
        {
            Action act = () => ValidationHelpers.RequirePositive(1, "Amount");
            act.Should().NotThrow();
        }

        [Test]
        public void RequireRange_Outside_Throws()
        {
            Action act = () => ValidationHelpers.RequireRange(1799, 1800, 2025, "Year");
            act.Should().Throw<ValidationException>().WithMessage("Year must be between 1800 and 2025.");
        }

        [Test]
        public void RequireRange_Inside_DoesNotThrow()
        {
            Action act = () => ValidationHelpers.RequireRange(2000, 1800, 2025, "Year");
            act.Should().NotThrow();
        }

        [Test]
        public void ValidateEmailIfProvided_NullOrEmpty_DoesNotThrow()
        {
            Action a1 = () => ValidationHelpers.ValidateEmailIfProvided(null);
            Action a2 = () => ValidationHelpers.ValidateEmailIfProvided("");
            a1.Should().NotThrow();
            a2.Should().NotThrow();
        }

        [Test]
        public void ValidateEmailIfProvided_Invalid_Throws()
        {
            Action act = () => ValidationHelpers.ValidateEmailIfProvided("bad");
            act.Should().Throw<ValidationException>().WithMessage("Email is not a valid email address.");
        }

        [Test]
        public void ValidateEmailIfProvided_Valid_DoesNotThrow()
        {
            Action act = () => ValidationHelpers.ValidateEmailIfProvided("a@b.com");
            act.Should().NotThrow();
        }

        [Test]
        public void RequireFromTo_FromDefault_Throws()
        {
            var from = default(DateTime);
            var to = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            Action act = () => ValidationHelpers.RequireFromTo(from, to, "From", "To");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("From is required.");
        }

        [Test]
        public void RequireFromTo_ToDefault_Throws()
        {
            var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = default(DateTime);

            Action act = () => ValidationHelpers.RequireFromTo(from, to, "From", "To");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("To is required.");
        }

        [Test]
        public void RequireFromTo_ToBeforeFrom_Throws()
        {
            var from = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Action act = () => ValidationHelpers.RequireFromTo(from, to, "From", "To");

            act.Should()
               .Throw<ValidationException>()
               .WithMessage("To must be after From");
        }

        [Test]
        public void RequireFromTo_Valid_DoesNotThrow()
        {
            var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            Action act = () => ValidationHelpers.RequireFromTo(from, to, "From", "To");

            act.Should().NotThrow();
        }
    }
