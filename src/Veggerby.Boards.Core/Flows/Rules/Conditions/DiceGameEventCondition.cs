using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class DiceGameEventCondition<T> : IGameEventCondition<RollDiceGameEvent<T>>
    {
        public DiceGameEventCondition(IEnumerable<Dice<T>> dice)
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

        public IEnumerable<Dice<T>> Dice { get; }

        public ConditionResponse Evaluate(GameState state, RollDiceGameEvent<T> @event)
        {
            return Dice.Join(@event.NewDiceStates, x => x, x => x.Artifact, (d, s) => s).All(s => s != null)
                ? ConditionResponse.Valid
                : ConditionResponse.Invalid;
        }
    }
}