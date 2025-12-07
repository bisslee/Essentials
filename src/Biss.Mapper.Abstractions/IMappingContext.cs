namespace Biss.Mapper.Abstractions;

/// <summary>
/// Represents the context for a mapping operation.
/// </summary>
public interface IMappingContext
{
    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets a dictionary of additional items that can be used during mapping.
    /// </summary>
    IDictionary<string, object> Items { get; }
}
