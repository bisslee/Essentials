namespace Biss.Mapper.Abstractions;

/// <summary>
/// Represents a custom type converter for mapping between different types.
/// </summary>
/// <typeparam name="TSource">The source type to convert from.</typeparam>
/// <typeparam name="TDestination">The destination type to convert to.</typeparam>
public interface ITypeConverter<TSource, TDestination>
{
    /// <summary>
    /// Converts a value from the source type to the destination type.
    /// </summary>
    /// <param name="source">The source value to convert.</param>
    /// <param name="context">The mapping context.</param>
    /// <returns>The converted destination value.</returns>
    TDestination Convert(TSource source, IMappingContext context);
}
