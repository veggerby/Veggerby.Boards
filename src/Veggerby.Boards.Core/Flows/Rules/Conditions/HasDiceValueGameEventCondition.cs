using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions;

public class HasDiceValueGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    public HasDiceValueGameEventCondition(IEnumerable<Dice> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException("At least one dice must be added to condition", nameof(dice));
        }

        Dice = dice;
    }

    public IEnumerable<Dice> Dice { get; }

    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var diceStates = Dice.Select(dice => state.GetState<DiceState<int>>(dice));
        return diceStates.Any(x => x is not null && x.CurrentValue.Equals(@event.Path.Distance))
            ? ConditionResponse.Valid
            : ConditionResponse.Fail("No dice matching path distance");
    }
}