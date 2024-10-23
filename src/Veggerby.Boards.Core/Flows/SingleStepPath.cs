using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows;

public class SingleStepPath
{
    public SingleStepPath(GameState state, DiceState<int> diceState, TilePath path, SingleStepPath previous = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        ArgumentNullException.ThrowIfNull(diceState);

        ArgumentNullException.ThrowIfNull(path);

        NewState = state;
        DiceState = diceState;
        Path = path;
        Previous = previous;
    }

    public GameState NewState { get; }
    public DiceState<int> DiceState { get; }
    public TilePath Path { get; }
    public SingleStepPath Previous { get; }

    public IEnumerable<TilePath> Paths
    {
        get
        {
            if (Previous is null)
            {
                return [Path];
            }

            return Previous.Paths.Concat([Path]).ToList();
        }
    }
}