using System;


using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Generator that doubles the cube value up to a maximum (1→2→4→...→64).
/// </summary>
public class DoublingDiceValueGenerator : NumericDiceValueGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DoublingDiceValueGenerator"/> class.
    /// </summary>
    public DoublingDiceValueGenerator() : base(1, 64)
    {
    }

    /// <inheritdoc />
    public override int GetValue(IArtifactState currentState)
    {
        if (currentState is not DiceState<int> state)
        {
            throw new ArgumentException("Illegal dice state", nameof(currentState));
        }

        if (state.CurrentValue < MinValue || (state.CurrentValue & (state.CurrentValue - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentState), "Illegal dice value");
        }

        return state.CurrentValue < MaxValue
            ? 2 * state.CurrentValue
            : MaxValue;
    }
}