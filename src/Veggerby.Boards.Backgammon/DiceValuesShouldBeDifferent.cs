using System;
using System.Linq;


using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Condition ensuring the rolled dice values are distinct (used to decide starting player).
/// </summary>
public class DiceValuesShouldBeDifferent : IGameEventCondition<RollDiceGameEvent<int>>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, RollDiceGameEvent<int> @event)
    {
        return @event.NewDiceStates.Select(x => x.CurrentValue).Distinct().Count() == @event.NewDiceStates.Count()
            ? ConditionResponse.Valid
            : ConditionResponse.Invalid;
    }
}