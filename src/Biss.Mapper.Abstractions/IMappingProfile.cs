using System.Linq.Expressions;

namespace Biss.Mapper.Abstractions;

/// <summary>
/// Represents a mapping configuration for a specific source and destination type pair.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMappingProfile<TSource, TDestination>
{
    /// <summary>
    /// Maps a property from the source to the destination.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="sourceMember">An expression representing the source property.</param>
    /// <param name="destinationMember">An expression representing the destination property.</param>
    /// <returns>The mapping profile for method chaining.</returns>
    IMappingProfile<TSource, TDestination> Map<TProperty>(
        Expression<Func<TSource, TProperty>> sourceMember,
        Expression<Func<TDestination, TProperty>> destinationMember);

    /// <summary>
    /// Ignores a property during mapping.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to ignore.</typeparam>
    /// <param name="destinationMember">An expression representing the destination property to ignore.</param>
    /// <returns>The mapping profile for method chaining.</returns>
    IMappingProfile<TSource, TDestination> Ignore<TProperty>(
        Expression<Func<TDestination, TProperty>> destinationMember);

    /// <summary>
    /// Uses a custom converter for the mapping.
    /// </summary>
    /// <typeparam name="TConverter">The type of the converter to use.</typeparam>
    /// <returns>The mapping profile for method chaining.</returns>
    IMappingProfile<TSource, TDestination> UseConverter<TConverter>()
        where TConverter : ITypeConverter<TSource, TDestination>;
}
