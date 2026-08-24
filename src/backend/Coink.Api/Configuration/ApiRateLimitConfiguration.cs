using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Coink.Api.Configuration;

internal static class ApiRateLimitConfiguration
{
    private const int DefaultPermitLimit = 100;
    private const int DefaultWindowSeconds = 60;
    private const int MaximumPermitLimit = 10_000;
    private const int MaximumWindowSeconds = 3_600;

    internal static IServiceCollection AddConfiguredRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        int permitLimit = configuration.GetValue<int?>("RateLimiting:PermitLimit")
            ?? DefaultPermitLimit;
        int windowSeconds = configuration.GetValue<int?>("RateLimiting:WindowSeconds")
            ?? DefaultWindowSeconds;

        if (permitLimit is < 1 or > MaximumPermitLimit)
        {
            throw new InvalidOperationException(
                $"RateLimiting:PermitLimit must be between 1 and {MaximumPermitLimit}.");
        }

        if (windowSeconds is < 1 or > MaximumWindowSeconds)
        {
            throw new InvalidOperationException(
                $"RateLimiting:WindowSeconds must be between 1 and {MaximumWindowSeconds}.");
        }

        _ = services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = permitLimit,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                    }));
            options.OnRejected = static async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers["Retry-After"] = Math
                        .Ceiling(retryAfter.TotalSeconds)
                        .ToString(CultureInfo.InvariantCulture);
                }

                IProblemDetailsService problemDetailsService = context.HttpContext.RequestServices
                    .GetRequiredService<IProblemDetailsService>();

                _ = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests.",
                        Detail = "Retry after the period indicated by the Retry-After header.",
                        Instance = context.HttpContext.Request.Path,
                    },
                });
            };
        });

        return services;
    }
}
