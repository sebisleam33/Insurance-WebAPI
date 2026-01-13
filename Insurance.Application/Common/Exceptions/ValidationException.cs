namespace Insurance.Application.Common.Exceptions;

    public sealed class ValidationException : AppException
    {
        public ValidationException(string message, object? details = null) : base(message) => _details = details;

        private readonly object? _details;
        public override int StatusCode => 400;
        public override object? Details => _details;
    }
