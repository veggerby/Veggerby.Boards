using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Flows.Events
{
    public class MovePieceGameEvent : IGameEvent
    {
        public Piece Piece { get; }
        public TilePath Path { get; }
        public Tile From => Path.From;
        public Tile To => Path.To;
        public int Distance => Path.Distance;

        public MovePieceGameEvent(Piece piece, TilePath path)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Piece = piece;
            Path = path;
        }
    }
}