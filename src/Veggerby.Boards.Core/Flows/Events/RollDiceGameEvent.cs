using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Events;

/// <summary>
/// Event representing one or more dice receiving new values.
/// </summary>
public class RollDiceGameEvent<T> : IGameEvent
{
    /// <summary>
    /// Gets the new dice states.
    /// </summary>
    public IEnumerable<DiceState<T>> NewDiceStates { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RollDiceGameEvent{T}"/> class.
    /// </summary>
    /// <param name="states">Dice state results.</param>
    public RollDiceGameEvent(params DiceState<T>[] states)
    {
        ArgumentNullException.ThrowIfNull(states);

        if (!states.Any())
        {
            throw new ArgumentException("Must provide at least one new state", nameof(states));
        }

        NewDiceStates = states.ToList().AsReadOnly();
    }
}