using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class ClearDiceStateMutator : IStateMutator<MovePieceGameEvent>
    {
        public ClearDiceStateMutator(IEnumerable<Dice> dice)
        {
            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            if (!dice.Any())
            {
                throw new ArgumentException("At least one dice must be added to condition", nameof(dice));
            }

            Dice = dice;
        }

        public IEnumerable<Dice> Dice { get; }

        public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
        {
            var diceState = Dice
                .Select(dice => state.GetState<DiceState<int>>(dice))
                .FirstOrDefault(x => x != null && x.CurrentValue.Equals(@event.Path.Distance));

            if (diceState == null)
            {
                throw new BoardException("No valid dice state for path");
            }

            var newState = new NullDiceState(diceState.Artifact);
            return state.Next(new [] { newState });
        }
    }
}