using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows
{
    public class SingleStepPath
    {
        public SingleStepPath(GameState state, DiceState<int> diceState, TilePath path, SingleStepPath previous = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (diceState == null)
            {
                throw new ArgumentNullException(nameof(diceState));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

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
                if (Previous == null)
                {
                    return [Path];
                }

                return Previous.Paths.Concat([Path]).ToList();
            }
        }
    }
}