using Microsoft.AspNetCore.Http;

namespace Insurance.Application.Common.Exceptions;

    public sealed class BusinessRuleException : AppException
    {
        private readonly object? _details;
        public BusinessRuleException(string message, object? details = null) : base(message) => _details = details;

        public override int StatusCode => StatusCodes.Status422UnprocessableEntity;
        public override object? Details => _details;
    }
