using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Immutable metadata attached to Othello discs describing color.
/// </summary>
/// <param name="Color">Disc color.</param>
public sealed record OthelloDiscMetadata(OthelloDiscColor Color) : IPieceMetadata;
