using System.Collections.Generic;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Game : CompositeArtifact<Piece>
    {
        public Board Board { get; }

        public Game(string id, Board board, IEnumerable<Piece> pieces) : base(id, pieces)
        {
            Board = board;
        }
    }
}