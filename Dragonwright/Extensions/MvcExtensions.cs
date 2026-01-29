using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dragonwright.Extensions;

/// <summary>
/// Provides extension methods for MVC-related functionalities.
/// </summary>
internal static class MvcExtensions
{
    /// <summary>
    /// Adds MVC-related services to the provided <see cref="IServiceCollection"/>. It scans the assembly for
    /// <see cref="IHostedService"/>s and registers them for dependency injection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    public static void AddServices(this IServiceCollection services)
    {
        var assembly = typeof(MvcExtensions).Assembly;
        var hostedServiceTypes = assembly.GetTypes()
            .Where(t => typeof(IHostedService).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

        foreach (var type in hostedServiceTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), type));
        }
    }
}