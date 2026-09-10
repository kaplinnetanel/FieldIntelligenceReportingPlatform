using Microsoft.AspNetCore.Diagnostics;
using Serilog;

namespace Api.ExceptionHandlingLab
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
                KeyNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                InvalidOperationException ex when ex.Message.Contains("Elasticsearch", StringComparison.OrdinalIgnoreCase) ||
                                                  ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) =>
                    (StatusCodes.Status502BadGateway, "Communication failure with external database or service."),

                _ => (StatusCodes.Status500InternalServerError, "An unexpected internal error occurred.")
            };

        
            if (statusCode >= 500)
            {
                Log.Warning(exception, "Critical system or communication error occurred. Status: {StatusCode}", statusCode);
            }
            else
            {
                Log.Warning("Client error encountered. Status: {StatusCode}, Message: {Message}", statusCode, message);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(
                new { error = message },
                cancellationToken);

            return true; 
        }
    }
}
