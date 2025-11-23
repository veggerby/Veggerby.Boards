using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Ludo.Events;

/// <summary>
/// Event representing a piece entering the board from the base after rolling a 6.
/// </summary>
public class EnterPieceGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the piece to enter the board.
    /// </summary>
    public Piece Piece
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnterPieceGameEvent"/> class.
    /// </summary>
    /// <param name="piece">The piece to enter the board.</param>
    public EnterPieceGameEvent(Piece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);

        Piece = piece;
    }
}
