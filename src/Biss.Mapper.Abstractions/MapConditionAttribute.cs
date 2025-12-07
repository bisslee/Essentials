namespace Biss.Mapper.Abstractions;

/// <summary>
/// Specifies a condition for conditional mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MapConditionAttribute : Attribute
{
    /// <summary>
    /// Gets the condition expression for the mapping.
    /// </summary>
    public string Condition { get; }

    /// <summary>
    /// Initializes a new instance of the MapConditionAttribute class.
    /// </summary>
    /// <param name="condition">The condition expression for the mapping.</param>
    public MapConditionAttribute(string condition)
    {
        Condition = condition;
    }
}
