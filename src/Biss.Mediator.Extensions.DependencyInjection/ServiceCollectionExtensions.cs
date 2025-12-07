using Biss.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Biss.Mediator.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Mediator services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mediator services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Register the mediator
        services.TryAddSingleton<IMediator, Mediator>();

        // Register all handlers
        foreach (var assembly in assemblies)
        {
            RegisterHandlers(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Adds Mediator services to the service collection, scanning the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        return services.AddMediator(assembly);
    }

    /// <summary>
    /// Adds Mediator services to the service collection, scanning the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly will be scanned.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorFromAssemblyContaining<T>(this IServiceCollection services)
    {
        return services.AddMediator(typeof(T).Assembly);
    }

    /// <summary>
    /// Adds pipeline behaviors to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for behaviors.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorBehaviors(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        foreach (var assembly in assemblies)
        {
            RegisterBehaviors(services, assembly);
        }

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => IsRequestHandler(type) || IsNotificationHandler(type))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(IsHandlerInterface)
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.TryAddTransient(interfaceType, handlerType);
            }
        }
    }

    private static void RegisterBehaviors(IServiceCollection services, Assembly assembly)
    {
        var behaviorTypes = assembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(IsPipelineBehavior)
            .ToList();

        foreach (var behaviorType in behaviorTypes)
        {
            var interfaces = behaviorType.GetInterfaces()
                .Where(IsPipelineBehaviorInterface)
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.TryAddTransient(interfaceType, behaviorType);
            }
        }
    }

    private static bool IsRequestHandler(Type type)
    {
        return type.GetInterfaces()
            .Any(interfaceType => interfaceType.IsGenericType &&
                 (interfaceType.GetGenericTypeDefinition().Name == "IRequestHandler" ||
                  interfaceType.GetGenericTypeDefinition().Name == "ICommandHandler" ||
                  interfaceType.GetGenericTypeDefinition().Name == "IQueryHandler"));
    }

    private static bool IsNotificationHandler(Type type)
    {
        return type.GetInterfaces()
            .Any(interfaceType => interfaceType.IsGenericType &&
                 interfaceType.GetGenericTypeDefinition().Name == "INotificationHandler");
    }

    private static bool IsPipelineBehavior(Type type)
    {
        return type.GetInterfaces()
            .Any(interfaceType => interfaceType.IsGenericType &&
                 interfaceType.GetGenericTypeDefinition().Name == "IPipelineBehavior");
    }

    private static bool IsHandlerInterface(Type interfaceType)
    {
        if (!interfaceType.IsGenericType) return false;

        var genericDefinition = interfaceType.GetGenericTypeDefinition();
        var name = genericDefinition.Name;

        return name == "IRequestHandler" ||
               name == "ICommandHandler" ||
               name == "IQueryHandler" ||
               name == "INotificationHandler";
    }

    private static bool IsPipelineBehaviorInterface(Type interfaceType)
    {
        if (!interfaceType.IsGenericType) return false;

        var genericDefinition = interfaceType.GetGenericTypeDefinition();
        return genericDefinition.Name == "IPipelineBehavior";
    }
}
