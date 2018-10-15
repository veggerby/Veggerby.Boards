using System;
using System.Linq;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class SelectActivePlayerGameStateMutator : IStateMutator<RollDiceGameEvent<int>>
    {
        public GameState MutateState(GameEngine engine, GameState gameState, RollDiceGameEvent<int> @event)
        {
            var white = engine.Game.GetPlayer("white");
            var black = engine.Game.GetPlayer("black");
            var dice1 = engine.Game.GetArtifact<Dice>("dice-1");
            var dice2 = engine.Game.GetArtifact<Dice>("dice-2");
            var diceState1 = @event.NewDiceStates.Single(x => x.Artifact.Equals(dice1));
            var diceState2 = @event.NewDiceStates.Single(x => x.Artifact.Equals(dice2));

            if (diceState1.CurrentValue > diceState2.CurrentValue)
            {
                return gameState.Next(new [] { new ActivePlayerState(white, true), new ActivePlayerState(black, false)  });
            }
            else if (diceState1.CurrentValue < diceState2.CurrentValue)
            {
                return gameState.Next(new [] { new ActivePlayerState(white, false), new ActivePlayerState(black, true)  });
            }

            throw new InvalidGameEventException(@event, null, engine.Game, null, gameState);
        }
    }
}