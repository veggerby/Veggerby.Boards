using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States.Conditions;

public class DiceGameStateCondition<TValue> : IGameStateCondition
{
    public IEnumerable<Dice> Dice { get; }
    public CompositeMode Mode { get; }

    public DiceGameStateCondition(IEnumerable<Dice> dice, CompositeMode mode)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException("Dice list cannot be empty", nameof(dice));
        }

        if (dice.Any(x => x is null))
        {
            throw new ArgumentException("All dice must be non null", nameof(dice));
        }

        Dice = dice.Distinct().ToList().AsReadOnly();
        Mode = mode;
    }

    public ConditionResponse Evaluate(GameState state)
    {
        var rolledDice = Dice
            .Select(x => state.GetState<DiceState<TValue>>(x))
            .Where(x => x is not null && !EqualityComparer<TValue>.Default.Equals(x.CurrentValue, default(TValue)))
            .Select(x => x.Artifact)
            .ToList();

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