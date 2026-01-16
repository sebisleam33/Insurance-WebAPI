using Insurance.Application.Common.Exceptions;
using System.Text.Json;

namespace Insurance.Api.Middleware;

    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Handled application exception: {ErrorCode}", ex.ErrorCode);
                await WriteErrorResponse(context, ex.StatusCode, ex.ErrorCode, ex.Message, ex.Details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteErrorResponse(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "InternalServerError",
                    "An unexpected error occurred.");
            }
        }

        private static async Task WriteErrorResponse(
            HttpContext context,
            int statusCode,
            string errorCode,
            string message,
            object? details = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = new ErrorResponse
            {
                StatusCode = statusCode,
                ErrorCode = errorCode,
                Message = message,
                Details = details
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(payload,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }

        private sealed class ErrorResponse
        {
            public int StatusCode { get; init; }
            public string ErrorCode { get; init; } = null!;
            public string Message { get; init; } = null!;
            public object? Details { get; init; }
        }
    }
