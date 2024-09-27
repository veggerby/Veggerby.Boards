using System;
using System.Linq;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class DoublingDiceStateMutator : IStateMutator<RollDiceGameEvent<int>>
    {
        public DoublingDiceStateMutator(Dice doublingDice)
        {
            if (doublingDice == null)
            {
                throw new ArgumentNullException(nameof(doublingDice));
            }

            DoublingDice = doublingDice;
        }

        public Dice DoublingDice { get; }

        public GameState MutateState(GameEngine engine, GameState state, RollDiceGameEvent<int> @event)
        {
            var diceState = state.GetState<DoublingDiceState>(DoublingDice);
            var nonActivePlayer = state.GetStates<ActivePlayerState>().SingleOrDefault(x => !x.IsActive);
            var generator = new DoublingDiceValueGenerator();
            var newValue = generator.GetValue(diceState);
            var newState = new DoublingDiceState(DoublingDice, newValue, nonActivePlayer.Artifact);
            return state.Next([newState]);
        }
    }
}