using System.Collections.Generic;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Immutable metadata map (piece id -&gt; role) injected once at game construction.
/// </summary>
/// <param name="Roles">Dictionary mapping piece identifiers to their <see cref="ChessPieceRole"/>. This map is static for the lifetime of a match.</param>
public sealed record ChessPieceRolesExtras(IReadOnlyDictionary<string, ChessPieceRole> Roles);