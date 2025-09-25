using System;

namespace Veggerby.Boards.Flows.Phases;

/// <summary>
/// Declares a static exclusivity group for a phase when applied to the phase condition or rule type.
/// </summary>
/// <remarks>
/// Precedence order when determining an entry's exclusivity group:
/// 1. Explicit builder assignment via <c>.Exclusive("group")</c>.
/// 2. Attribute on the phase condition type.
/// 3. Attribute on the rule type.
/// If multiple attributes are present the earlier precedence wins.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ExclusiveGroupAttribute(string groupId) : Attribute
{
    /// <summary>
    /// Declared exclusivity group identifier.
    /// </summary>
    public string GroupId { get; } = groupId;
}