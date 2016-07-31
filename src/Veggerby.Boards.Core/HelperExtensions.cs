using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Phases;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public static class HelperExtensions
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T obj)
        {
            return list.Except(new [] { obj });
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> list, T obj)
        {
            return list.Concat(new [] { obj });
        }

        public static TurnState FirstTurn(this GameEngine gameEngine)
        {
            var player = gameEngine.Game.Players.First();
            var turn = new Turn(new Round(1), 1);
            return gameEngine.EvaluateTurnState(player, turn);
        }
    }
}