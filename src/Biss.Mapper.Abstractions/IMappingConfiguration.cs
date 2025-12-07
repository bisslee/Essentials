namespace Biss.Mapper.Abstractions;

/// <summary>
/// Represents a mapping configuration that can be used to configure mappings.
/// </summary>
public interface IMappingConfiguration
{
    /// <summary>
    /// Configures the mapping between the specified source and destination types.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="profile">The mapping profile to configure.</param>
    void ConfigureMapping<TSource, TDestination>(IMappingProfile<TSource, TDestination> profile);
}
