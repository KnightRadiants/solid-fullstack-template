using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace SolidFullstackTemplate.Api.Middlewares;

internal sealed class SlowRequestLoggingOptions
{
    public int ThresholdSeconds { get; init; } = 4;
}

internal class SlowRequestLoggingMiddleware(
    ILogger<SlowRequestLoggingMiddleware> logger,
    IOptions<SlowRequestLoggingOptions> options)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();

        TimeSpan threshold = TimeSpan.FromSeconds(options.Value.ThresholdSeconds);
        if (stopwatch.Elapsed >= threshold)
        {
            logger.LogWarning("Slow request: {RequestPath} took {ElapsedMilliseconds}ms",
                context.Request.Path, stopwatch.ElapsedMilliseconds);
        }
    }
}
