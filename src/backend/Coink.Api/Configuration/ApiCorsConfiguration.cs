namespace Coink.Api.Configuration;

internal static class ApiCorsConfiguration
{
    internal const string PolicyName = "WebClient";

    private const string AllowedOriginsPath = "Cors:AllowedOrigins";

    internal static IServiceCollection AddConfiguredCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string[] allowedOrigins = configuration
            .GetSection(AllowedOriginsPath)
            .Get<string[]>()?
            .Where(static origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        foreach (string? origin in allowedOrigins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    $"CORS origin '{origin}' must be an absolute HTTP or HTTPS URI.");
            }
        }

        _ = services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    _ = policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }
}
