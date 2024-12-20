using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions;

public class DiceGameEventCondition<T> : IGameEventCondition<RollDiceGameEvent<T>>
{
    public DiceGameEventCondition(IEnumerable<Dice> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException("At least one dice must be added to condition", nameof(dice));
        }

        Dice = dice;
    }

    public IEnumerable<Dice> Dice { get; }

    public ConditionResponse Evaluate(GameEngine engine, GameState state, RollDiceGameEvent<T> @event)
    {
        return Dice.Join(@event.NewDiceStates, x => x, x => x.Artifact, (d, s) => s).All(s => s is not null)
            ? ConditionResponse.Valid
            : ConditionResponse.Invalid;
    }
}