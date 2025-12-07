namespace Biss.Mapper.Abstractions;

/// <summary>
/// Marks a property to be ignored during mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreAttribute : Attribute { }
