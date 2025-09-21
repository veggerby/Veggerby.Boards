using System;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Flows.Events;

/// <summary>
/// Event representing the movement of a piece along a resolved path.
/// </summary>
public class MovePieceGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the moving piece.
    /// </summary>
    public Piece Piece { get; }
    /// <summary>
    /// Gets the traversed path.
    /// </summary>
    public TilePath Path { get; }
    /// <summary>
    /// Gets the origin tile.
    /// </summary>
    public Tile From => Path.From;
    /// <summary>
    /// Gets the destination tile.
    /// </summary>
    public Tile To => Path.To;
    /// <summary>
    /// Gets the path distance (movement magnitude).
    /// </summary>
    public int Distance => Path.Distance;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePieceGameEvent"/> class.
    /// </summary>
    /// <param name="piece">Piece to move.</param>
    /// <param name="path">Resolved tile path.</param>
    public MovePieceGameEvent(Piece piece, TilePath path)
    {
        ArgumentNullException.ThrowIfNull(piece);

        ArgumentNullException.ThrowIfNull(path);

        Piece = piece;
        Path = path;
    }
}