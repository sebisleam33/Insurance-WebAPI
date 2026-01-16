using Microsoft.AspNetCore.Http;

namespace Insurance.Application.Common.Exceptions;

    public sealed class ValidationException : AppException
    {
        private readonly object? _details;

        public ValidationException(string message, object? details = null) : base(message) => _details = details;

        public override int StatusCode => StatusCodes.Status400BadRequest;
        public override object? Details => _details;
    }
