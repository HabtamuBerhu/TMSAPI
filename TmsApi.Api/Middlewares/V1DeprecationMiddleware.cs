namespace TmsApi.Middleware;

public class V1DeprecationMiddleware(RequestDelegate next)
{
    private static readonly DateTimeOffset SunsetDate =
        new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            if (context.Request.Path.StartsWithSegments("/api/v1"))
            {
                context.Response.Headers["Deprecation"] = "true";

                context.Response.Headers["Sunset"] =
                    SunsetDate.ToString("R");

                var v1Path = context.Request.Path.Value ?? "";

                var v2Path = v1Path.StartsWith("/api/v1")
                    ? "/api/v2" + v1Path.Substring(6)
                    : v1Path;

                context.Response.Headers["Link"] =
                    $"<{context.Request.Scheme}://{context.Request.Host}{v2Path}>; rel=\"successor-version\"";
            }

            return Task.CompletedTask;
        });

        await next(context);
    }
}