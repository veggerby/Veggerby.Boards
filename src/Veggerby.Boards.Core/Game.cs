using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core
{
    public class Game : Artifact
    {
        public Board Board { get; }

        public IEnumerable<Piece> Pieces { get; }

        public RuleEngine Rules { get; }

        public Game(string id, Board board, IEnumerable<Piece> pieces, RuleEngine rules) : base(id)
        {
            Board = board;
            Pieces = (pieces ?? Enumerable.Empty<Piece>()).ToList();
            Rules = rules;
        }
    }
}