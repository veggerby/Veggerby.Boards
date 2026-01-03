using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Immutable metadata attached to Othello discs describing color.
/// </summary>
public sealed record OthelloDiscMetadata : IPieceMetadata
{
    /// <summary>
    /// Gets the disc color.
    /// </summary>
    public OthelloDiscColor Color
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OthelloDiscMetadata"/> class.
    /// </summary>
    /// <param name="color">Disc color.</param>
    public OthelloDiscMetadata(OthelloDiscColor color)
    {
        Color = color;
    }
}
