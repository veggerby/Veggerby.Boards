using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class MovePieceStateMutator : IStateMutator<MovePieceGameEvent>
    {
        public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
        {
            var currentState = gameState.GetState<PieceState>(@event.Piece);

            if (!currentState.CurrentTile.Equals(@event.From))
            {
                throw new BoardException("Invalid from tile on move piece event");
            }

            var newState = new PieceState(@event.Piece, @event.To);
            return gameState.Next(new [] { newState });
        }
    }
}