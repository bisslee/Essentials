namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents a unit type for requests that don't return a value.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// The single instance of Unit.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Determines whether the specified Unit is equal to this instance.
    /// </summary>
    /// <param name="other">The Unit to compare with this instance.</param>
    /// <returns>true if the specified Unit is equal to this instance; otherwise, false.</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>true if the specified object is equal to this instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a string representation of this Unit.
    /// </summary>
    /// <returns>A string representation of this Unit.</returns>
    public override string ToString() => "Unit";
}
