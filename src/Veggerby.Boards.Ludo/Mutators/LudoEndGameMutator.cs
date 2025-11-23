using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Mutators;

/// <summary>
/// Mutator that marks the game as ended when a player wins.
/// </summary>
public class LudoEndGameMutator : IStateMutator<IGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Determine winner
        var playerColors = new[] { "red", "blue", "green", "yellow" };
        Player? winner = null;

        foreach (var playerColor in playerColors)
        {
            var finalHomeTileId = $"home-{playerColor}-4";

            var piecesAtFinalHome = gameState.GetStates<PieceState>()
                .Count(ps => ps.CurrentTile is not null &&
                             string.Equals(ps.CurrentTile.Id, finalHomeTileId, StringComparison.Ordinal));

            if (piecesAtFinalHome >= 4)
            {
                winner = engine.Game.GetPlayer(playerColor);
                break;
            }
        }

        if (winner is null)
        {
            throw new BoardException("No winner found despite win condition being met");
        }

        var outcome = new LudoOutcomeState(winner);
        var endedState = new GameEndedState();

        return gameState.Next([outcome, endedState]);
    }
}
