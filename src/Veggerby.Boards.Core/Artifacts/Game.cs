using System.Collections.Generic;
using Veggerby.Boards.Core.Phases;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Game : CompositeArtifact<Piece>
    {
        public Board Board { get; }

        public IEnumerable<GamePhase> GamePhases { get; }
        public IEnumerable<TurnPhase> TurnPhases { get; }

        public Game(string id, Board board, IEnumerable<Piece> pieces) : base(id, pieces)
        {
            Board = board;
            GamePhases = new [] { new GamePhase() };
            TurnPhases = new [] { new TurnPhase() };
        }
    }
}