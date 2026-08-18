using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TmsApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Generate a short, unique correlation ID
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            // 2. Set context.Response.Headers before calling next (ensures availability in downstream pipelines)
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            // 3. Capture request details
            var method = context.Request.Method;
            var path = context.Request.Path;

            // 4. Log structured entry on entry
            _logger.LogInformation(
                "[{CorrelationId}] Request Started: {Method} {Path}", 
                correlationId, method, path
            );

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Pass control to the next middleware in the pipeline
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;

                // 5. Log structured entry on exit with timing and response status code
                _logger.LogInformation(
                    "[{CorrelationId}] Request Finished: {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    correlationId, method, path, statusCode, elapsedMs
                );
            }
        }
    }
}