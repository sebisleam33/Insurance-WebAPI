using Insurance.Application.Common.Exceptions;
using System.Text.RegularExpressions;

namespace Insurance.Application.Common.Validation;

    public static class ValidationHelpers
    {
        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void RequireNotEmpty(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException($"{fieldName} is required.");
            }
        }

        public static void RequirePositive(decimal value, string fieldName)
        {
            if (value <= 0)
            {
                throw new ValidationException($"{fieldName} must be > 0.");
            }
        }

        public static void RequireRange(int value, int min, int max, string fieldName)
        {
            if (value < min || value > max)
            {
                throw new ValidationException($"{fieldName} must be between {min} and {max}.");
            }
        }

        public static void RequireInRange(decimal value, decimal min, decimal max, string fieldName)
        {
            if (value < min || value > max)
                throw new ValidationException($"{fieldName} must be between {min} and {max}.");
        }

    public static void ValidateEmailIfProvided(string? email)
        {
            if (!string.IsNullOrWhiteSpace(email) && !EmailRegex.IsMatch(email))
            {
                throw new ValidationException("Email is not a valid email address.");
            }
        }
    }
