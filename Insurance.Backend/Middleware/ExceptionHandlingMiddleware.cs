using Insurance.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace Insurance.Api.Middleware;

    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex.StatusCode;

                var payload = new
                {
                    errorCode = ex.ErrorCode,
                    message = ex.Message,
                    details = ex.Details
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Unhandled Exception.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var payload = new
                {
                    errorCode = "InternalServerError",
                    messsage = "An unexpected error occured."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }
