using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Flows.Events
{
    public class MovePieceGameEvent : IGameEvent
    {
        public Piece Piece { get; }
        public Tile From { get; }
        public Tile To { get; }

        public MovePieceGameEvent(Piece piece, Tile from, Tile to)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            Piece = piece;
            From = from;
            To = to;
        }
    }
}