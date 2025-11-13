using System;

namespace Veggerby.Boards.Diagnostics;

/// <summary>
/// Marks a public type to be excluded from XML documentation coverage enforcement tests.
/// </summary>
/// <remarks>Use sparingly; prefer adding proper documentation. Intended for generated or trivial container types.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
public sealed class DocCoverageIgnoreAttribute : Attribute
{
}
