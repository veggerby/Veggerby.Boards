using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Ensures that at least one specified dice value matches the movement path distance of the event.
/// </summary>
public class HasDiceValueGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HasDiceValueGameEventCondition"/> class.
    /// </summary>
    /// <param name="dice">Dice considered during validation.</param>
    public HasDiceValueGameEventCondition(IEnumerable<Dice> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException("At least one dice must be added to condition", nameof(dice));
        }

        Dice = dice;
    }

    /// <summary>
    /// Gets the dice included in the check.
    /// </summary>
    public IEnumerable<Dice> Dice
    {
        get;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var diceStates = Dice.Select(dice => state.GetState<DiceState<int>>(dice));
        return diceStates.Any(x => x is not null && x.CurrentValue.Equals(@event.Path.Distance))
            ? ConditionResponse.Valid
            : ConditionResponse.Fail("No dice matching path distance");
    }
}