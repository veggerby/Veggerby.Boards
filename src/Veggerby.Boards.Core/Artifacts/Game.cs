using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Game : CompositeArtifact<Piece>
    {
        public Board Board { get; }
        public IEnumerable<Player> Players { get; }

        public Game(string id, Board board, IEnumerable<Player> players, IEnumerable<Piece> pieces) : base(id, pieces)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            if (!players.Any())
            {
                throw new ArgumentException("Empty player list", nameof(players));
            }

            Board = board;
            Players = players.ToList();
        }
    }
}