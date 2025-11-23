using System;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Ludo.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Mutators;

/// <summary>
/// Mutator that moves a piece from the base to its starting square on the track.
/// </summary>
public class EnterPieceStateMutator : IStateMutator<EnterPieceGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EnterPieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var owner = @event.Piece.Owner;
        if (owner is null)
        {
            throw new BoardException("Piece has no owner");
        }

        // Determine starting square based on player
        var playerColors = new[] { "red", "blue", "green", "yellow" };
        int playerIndex = Array.IndexOf(playerColors, owner.Id);
        if (playerIndex == -1)
        {
            throw new BoardException($"Unknown player color: {owner.Id}");
        }

        var startingSquareId = $"track-{playerIndex * 13}";
        var startingSquare = engine.Game.GetTile(startingSquareId);
        if (startingSquare is null)
        {
            throw new BoardException($"Starting square not found: {startingSquareId}");
        }

        var newState = new PieceState(@event.Piece, startingSquare);
        return gameState.Next([newState]);
    }
}
