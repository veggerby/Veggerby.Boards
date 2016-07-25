using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Phases
{
    public class GamePhase : Phase
    {
        private readonly GameEngine _gameEngine;
        private readonly IList<Round> _rounds;

        public GamePhase(GameEngine gameEngine)
        {
            if (gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            _gameEngine = gameEngine;
            _rounds = new List<Round>();
        }

        public Round NextRound()
        {
            var round = new Round(_rounds.Count() + 1, _gameEngine.Players.Select(x => new Turn(x)));
            _rounds.Add(round);
            return round;
        }

        public Round CurrentRound => _rounds.Last();
    }
}