using Biss.Mediator.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Biss.Mediator.Extensions.AspNetCore;

/// <summary>
/// Extension methods for registering Mediator services in ASP.NET Core.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mediator services and configures ASP.NET Core integration.
    /// This method registers the Mediator and scans the specified assemblies for handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorWithAspNetCore(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register Mediator using the base extension method
        services.AddMediator(assemblies);
        
        return services;
    }

    /// <summary>
    /// Adds Mediator services and configures ASP.NET Core integration.
    /// This method registers the Mediator and scans the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly will be scanned.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorWithAspNetCore<T>(this IServiceCollection services)
    {
        return services.AddMediatorWithAspNetCore(typeof(T).Assembly);
    }

    /// <summary>
    /// Adds Mediator services and configures ASP.NET Core integration.
    /// This method registers the Mediator and scans the calling assembly for handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorWithAspNetCore(this IServiceCollection services)
    {
        return services.AddMediatorWithAspNetCore(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Configures MVC options for Mediator integration.
    /// This method can be used to customize how Mediator integrates with ASP.NET Core MVC.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional action to configure MVC options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureMediatorMvc(
        this IServiceCollection services,
        Action<MvcOptions>? configure = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configure != null)
        {
            services.Configure(configure);
        }

        return services;
    }
}

