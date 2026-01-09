using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.RockPaperScissors;

/// <summary>
/// Condition validating that a <see cref="SelectChoiceEvent"/> is applicable.
/// </summary>
internal sealed class SelectChoiceCondition : IGameEventCondition<SelectChoiceEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, SelectChoiceEvent @event)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        return ConditionResponse.Valid;
    }
}
