using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public class GameEngine
    {
        public Game Game { get; }
        public GamePhase GamePhaseRoot { get; }

        public GameEngine(Game game, GamePhase gamePhaseRoot)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (gamePhaseRoot == null)
            {
                throw new ArgumentNullException(nameof(gamePhaseRoot));
            }

            Game = game;
            GamePhaseRoot = gamePhaseRoot;
        }
    }
}