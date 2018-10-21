using System;
using System.Linq;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class DoublingDiceWithActivePlayerGameEventCondition : IGameEventCondition<RollDiceGameEvent<int>    >
    {
        public DoublingDiceWithActivePlayerGameEventCondition(Dice doublingDice)
        {
            if (doublingDice == null)
            {
                throw new ArgumentNullException(nameof(doublingDice));
            }

            DoublingDice = doublingDice;
        }

        public Dice DoublingDice { get; }

        public ConditionResponse Evaluate(GameEngine engine, GameState state, RollDiceGameEvent<int> @event)
        {
            var diceState = state.GetState<DoublingDiceState>(DoublingDice);
            var activePlayer = state.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive);

            return diceState.CurrentPlayer == null || diceState.CurrentPlayer.Equals(activePlayer.Artifact)
                ? ConditionResponse.Valid
                : ConditionResponse.Fail("Doubling dice with opponent");
        }
    }
}