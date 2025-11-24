using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Checkers;

/// <summary>
/// Immutable metadata attached to checkers pieces describing role and color.
/// </summary>
public sealed record CheckersPieceMetadata : IPieceMetadata
{
    /// <summary>
    /// Gets the piece role (regular or king).
    /// </summary>
    public CheckersPieceRole Role
    {
        get;
    }

    /// <summary>
    /// Gets the piece color.
    /// </summary>
    public CheckersPieceColor Color
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckersPieceMetadata"/> class.
    /// </summary>
    /// <param name="role">Piece role.</param>
    /// <param name="color">Piece color.</param>
    public CheckersPieceMetadata(CheckersPieceRole role, CheckersPieceColor color)
    {
        Role = role;
        Color = color;
    }
}
