using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Ensures a dice roll event updates all targeted dice artifacts.
/// </summary>
public class DiceGameEventCondition<T> : IGameEventCondition<RollDiceGameEvent<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiceGameEventCondition{T}"/> class.
    /// </summary>
    /// <param name="dice">Dice that must appear in the roll event.</param>
    public DiceGameEventCondition(IEnumerable<Dice> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException("At least one dice must be added to condition", nameof(dice));
        }

        Dice = dice;
    }

    /// <summary>
    /// Gets the required dice.
    /// </summary>
    public IEnumerable<Dice> Dice { get; }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, RollDiceGameEvent<T> @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        return Dice.Join(@event.NewDiceStates, x => x, x => x.Artifact, (d, s) => s).All(s => s is not null)
            ? ConditionResponse.Valid
            : ConditionResponse.Invalid;
    }
}