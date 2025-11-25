using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that sends a player to jail.
/// </summary>
public class GoToJailStateMutator : IStateMutator<GoToJailGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, GoToJailGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var playerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            throw new InvalidOperationException($"Player state not found for {@event.Player.Id}");
        }

        // Move player piece to Jail (position 10)
        var piece = engine.Game.Artifacts.OfType<Artifacts.Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, @event.Player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            throw new InvalidOperationException($"Piece not found for player {@event.Player.Id}");
        }

        var jailTile = engine.Game.Board.GetTile(MonopolyBoardConfiguration.GetTileId(10));
        if (jailTile is null)
        {
            throw new InvalidOperationException("Jail tile not found");
        }

        var newPieceState = new PieceState(piece, jailTile);
        var newPlayerState = playerState.GoToJail();

        return gameState.Next([newPieceState, newPlayerState]);
    }
}
