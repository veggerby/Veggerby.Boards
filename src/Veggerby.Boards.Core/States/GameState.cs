using System.Collections.Generic;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class GameState : CompositeState<Game>
    {
        public GameState(Game game, IEnumerable<IState> childStates) : base(game, childStates)
        {
        }
    }
}