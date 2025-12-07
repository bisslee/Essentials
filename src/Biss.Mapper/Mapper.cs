using Biss.Mapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Biss.Mapper;

/// <summary>
/// Default mapper implementation that uses reflection for mapping.
/// This is used as a fallback when the source generator is not available.
/// </summary>
public partial class Mapper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> _mappings;

    public Mapper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _mappings = new Dictionary<Type, Dictionary<Type, Func<object, object>>>();
    }

    /// <summary>
    /// Maps an object from source type to destination type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped destination object.</returns>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var sourceType = typeof(TSource);
        var destinationType = typeof(TDestination);

        if (!_mappings.TryGetValue(sourceType, out var typeMappings))
        {
            typeMappings = new Dictionary<Type, Func<object, object>>();
            _mappings[sourceType] = typeMappings;
        }

        if (!typeMappings.TryGetValue(destinationType, out var mappingFunc))
        {
            mappingFunc = CreateMappingFunction<TSource, TDestination>();
            typeMappings[destinationType] = mappingFunc;
        }

        return (TDestination)mappingFunc(source);
    }

    private Func<object, object> CreateMappingFunction<TSource, TDestination>()
    {
        var sourceType = typeof(TSource);
        var destinationType = typeof(TDestination);

        // Try to find a custom converter
        var converterType = typeof(ITypeConverter<TSource, TDestination>);
        var converter = _serviceProvider.GetService(converterType);
        
        if (converter != null)
        {
            var convertMethod = converterType.GetMethod("Convert");
            return source => convertMethod!.Invoke(converter, new object[] { source, CreateMappingContext() })!;
        }

        // Use reflection-based mapping
        return source => MapWithReflection<TSource, TDestination>((TSource)source)!;
    }

    private TDestination MapWithReflection<TSource, TDestination>(TSource source)
    {
        var destination = Activator.CreateInstance<TDestination>();
        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProp in sourceProperties)
        {
            if (!sourceProp.CanRead) continue;

            var destinationProp = destinationProperties.FirstOrDefault(p => 
                p.Name == sourceProp.Name && 
                p.CanWrite && 
                p.PropertyType.IsAssignableFrom(sourceProp.PropertyType));

            if (destinationProp != null)
            {
                var value = sourceProp.GetValue(source);
                destinationProp.SetValue(destination, value);
            }
        }

        return destination;
    }

    private IMappingContext CreateMappingContext()
    {
        return new MappingContext(_serviceProvider);
    }
}

/// <summary>
/// Default implementation of IMappingContext.
/// </summary>
internal class MappingContext : IMappingContext
{
    public IServiceProvider ServiceProvider { get; }
    public IDictionary<string, object> Items { get; }

    public MappingContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Items = new Dictionary<string, object>();
    }
}
