using System.Collections.Generic;

namespace Veggerby.Boards.Core.States
{
    public class GameState : CompositeState<Game>
    {
        public GameState(Game game, IEnumerable<IState> childStates) : base(game, childStates)
        {
        }
    }
}