using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Mutators;

/// <summary>
/// Mutator that adds game termination states when checkers game ends.
/// Determines the winner and creates appropriate outcome state.
/// </summary>
public sealed class CheckersEndGameMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckersEndGameMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public CheckersEndGameMutator(Game game)
    {
        _game = game;
    }

    /// <summary>
    /// Mutates the state to add game ended and outcome states if game is over.
    /// </summary>
    /// <param name="engine">The game engine.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="event">The triggering event.</param>
    /// <returns>New state with game ended markers if applicable.</returns>
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Check if already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return state;
        }

        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return state;
        }

        // Check if active player has any pieces remaining on board (not captured)
        var activePieces = _game.Artifacts
            .OfType<Piece>()
            .Where(p => p.Owner.Id == activePlayer.Id && !state.IsCaptured(p))
            .ToList();

        // Determine winner (opponent of active player who cannot move)
        Player? winner = null;

        if (activePieces.Count == 0)
        {
            // Active player loses (no pieces)
            winner = _game.Players.FirstOrDefault(p => p != activePlayer);
        }
        else
        {
            // Game not over yet
            return state;
        }

        // Create outcome state
        var outcomeState = new CheckersOutcomeState(winner, _game.Players);

        // Add both GameEndedState and outcome state
        return state.Next(new IArtifactState[]
        {
            new GameEndedState(),
            outcomeState
        });
    }
}
