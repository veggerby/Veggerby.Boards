using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts;

using Veggerby.Boards.Internal;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Condition that evaluates whether a set of dice have produced (or not produced) a rolled value.
/// </summary>
/// <typeparam name="TValue">The underlying value type held by the dice state.</typeparam>
/// <remarks>
/// A die is considered "rolled" when its associated <see cref="DiceState{TValue}"/> exists in the <see cref="GameState"/>
/// and its <see cref="DiceState{TValue}.CurrentValue"/> is not the default for <typeparamref name="TValue"/>.
/// The <see cref="CompositeMode"/> determines how the set membership is interpreted:
/// <list type="bullet">
/// <item><see cref="CompositeMode.All"/>: all specified dice must have rolled values.</item>
/// <item><see cref="CompositeMode.Any"/>: at least one specified die must have a rolled value.</item>
/// <item><see cref="CompositeMode.None"/>: no specified dice may have rolled values.</item>
/// </list>
/// </remarks>
public class DiceGameStateCondition<TValue> : IGameStateCondition
{
    /// <summary>
    /// Gets the distinct dice participating in this condition.
    /// </summary>
    public IEnumerable<Dice> Dice
    {
        get;
    }

    /// <summary>
    /// Gets the composite evaluation mode applied to the dice results.
    /// </summary>
    public CompositeMode Mode
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiceGameStateCondition{TValue}"/> class.
    /// </summary>
    /// <param name="dice">The dice to evaluate.</param>
    /// <param name="mode">The composite evaluation mode.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dice"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the dice collection is empty or contains null elements.</exception>
    public DiceGameStateCondition(IEnumerable<Dice> dice, CompositeMode mode)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException(ExceptionMessages.DiceListEmpty, nameof(dice));
        }

        if (dice.Any(x => x is null))
        {
            throw new ArgumentException(ExceptionMessages.DiceListContainsNull, nameof(dice));
        }

        Dice = dice.Distinct().ToList().AsReadOnly();
        Mode = mode;
    }

    /// <summary>
    /// Evaluates the condition against the provided <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The game state.</param>
    /// <returns><see cref="ConditionResponse.Valid"/> if the composite dice rule is satisfied; otherwise <see cref="ConditionResponse.Invalid"/>.</returns>
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var rolledDice = new List<Dice>();
        foreach (var d in Dice)
        {
            var ds = state.GetState<DiceState<TValue>>(d);
            if (ds is not null && !EqualityComparer<TValue>.Default.Equals(ds.CurrentValue, default!))
            {
                rolledDice.Add(ds.Artifact);
            }
        }

        bool result;
        switch (Mode)
        {
            case CompositeMode.All:
                result = Dice.All(x => rolledDice.Contains(x));
                break;
            case CompositeMode.Any:
                result = Dice.Any(x => rolledDice.Contains(x));
                break;
            case CompositeMode.None:
                result = !Dice.Any(x => rolledDice.Contains(x));
                break;
            default:
                result = false;
                break;
        }

        return result ? ConditionResponse.Valid : ConditionResponse.Invalid;
    }
}