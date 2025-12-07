namespace Biss.Mapper.Abstractions;

/// <summary>
/// Specifies custom mapping configuration for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class MapPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the source property to map from.
    /// </summary>
    public string SourceProperty { get; }

    /// <summary>
    /// Gets or sets the name of the target property to map to. If not specified, uses the source property name.
    /// </summary>
    public string? TargetProperty { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore this property during mapping.
    /// </summary>
    public bool Ignore { get; set; } = false;

    /// <summary>
    /// Gets or sets the name of a custom converter to use for this property.
    /// </summary>
    public string? Converter { get; set; }

    /// <summary>
    /// Initializes a new instance of the MapPropertyAttribute class.
    /// </summary>
    /// <param name="sourceProperty">The name of the source property to map from.</param>
    public MapPropertyAttribute(string sourceProperty)
    {
        SourceProperty = sourceProperty;
    }
}
