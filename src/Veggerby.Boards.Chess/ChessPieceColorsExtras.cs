using System.Collections.Generic;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Immutable metadata map (piece id -&gt; color) injected once at game construction.
/// </summary>
/// <param name="Colors">Dictionary mapping piece identifiers to their <see cref="ChessPieceColor"/>. This map is static for the lifetime of a match.</param>
public sealed record ChessPieceColorsExtras(IReadOnlyDictionary<string, ChessPieceColor> Colors);