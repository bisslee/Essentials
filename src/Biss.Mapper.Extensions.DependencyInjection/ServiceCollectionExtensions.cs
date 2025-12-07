using Biss.Mapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Biss.Mapper.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Mapper services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mapper services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for mappers and converters.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMappers(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        foreach (var assembly in assemblies)
        {
            RegisterMappers(services, assembly);
            RegisterConverters(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Adds Mapper services to the service collection, scanning the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for mappers and converters.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMappersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        return services.AddMappers(assembly);
    }

    /// <summary>
    /// Adds Mapper services to the service collection, scanning the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly will be scanned.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMappersFromAssemblyContaining<T>(this IServiceCollection services)
    {
        return services.AddMappers(typeof(T).Assembly);
    }

    /// <summary>
    /// Adds a specific mapper type to the service collection.
    /// </summary>
    /// <typeparam name="TMapper">The mapper type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMapper<TMapper>(this IServiceCollection services)
        where TMapper : class
    {
        services.TryAddSingleton<TMapper>();
        return services;
    }

    /// <summary>
    /// Adds a specific type converter to the service collection.
    /// </summary>
    /// <typeparam name="TConverter">The converter type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTypeConverter<TConverter>(this IServiceCollection services)
        where TConverter : class
    {
        var converterType = typeof(TConverter);
        var interfaces = converterType.GetInterfaces()
            .Where(IsTypeConverterInterface)
            .ToList();

        foreach (var interfaceType in interfaces)
        {
            services.TryAddTransient(interfaceType, converterType);
        }

        return services;
    }

    private static void RegisterMappers(IServiceCollection services, Assembly assembly)
    {
        var mapperTypes = assembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => HasMapperAttribute(type))
            .ToList();

        foreach (var mapperType in mapperTypes)
        {
            services.TryAddSingleton(mapperType);
        }
    }

    private static void RegisterConverters(IServiceCollection services, Assembly assembly)
    {
        var converterTypes = assembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => IsTypeConverter(type))
            .ToList();

        foreach (var converterType in converterTypes)
        {
            var interfaces = converterType.GetInterfaces()
                .Where(IsTypeConverterInterface)
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.TryAddTransient(interfaceType, converterType);
            }
        }
    }

    private static bool HasMapperAttribute(Type type)
    {
        return type.GetCustomAttribute<MapperAttribute>() != null;
    }

    private static bool IsTypeConverter(Type type)
    {
        return type.GetInterfaces()
            .Any(interfaceType => interfaceType.IsGenericType &&
                 interfaceType.GetGenericTypeDefinition().Name == "ITypeConverter");
    }

    private static bool IsTypeConverterInterface(Type interfaceType)
    {
        if (!interfaceType.IsGenericType) return false;

        var genericDefinition = interfaceType.GetGenericTypeDefinition();
        return genericDefinition.Name == "ITypeConverter";
    }
}
