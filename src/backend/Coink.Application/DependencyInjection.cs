using Coink.Application.Geography;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Coink.Application;

/// <summary>
/// Registers application-layer services without introducing web or infrastructure dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds validators and application services from this assembly.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <returns>The same collection for composition chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);
        _ = services.AddScoped<IGeographyService, GeographyService>();

        return services;
    }
}
