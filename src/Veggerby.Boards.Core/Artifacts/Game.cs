using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Game
    {
        public Board Board { get; }
        public IEnumerable<Piece> Pieces { get; }
        public IEnumerable<Player> Players { get; }

        public Game(Board board, IEnumerable<Player> players, IEnumerable<Piece> pieces)
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

            if (pieces == null)
            {
                throw new ArgumentNullException(nameof(pieces));
            }

            if (!pieces.Any())
            {
                throw new ArgumentException("Empty piece list", nameof(pieces));
            }

            Board = board;
            Players = players.ToList().AsReadOnly();
            Pieces = pieces.ToList().AsReadOnly();
        }
    }
}