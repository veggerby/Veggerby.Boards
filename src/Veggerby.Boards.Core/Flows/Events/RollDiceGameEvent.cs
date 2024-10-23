using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Events;

public class RollDiceGameEvent<T> : IGameEvent
{
    public IEnumerable<DiceState<T>> NewDiceStates { get; }

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