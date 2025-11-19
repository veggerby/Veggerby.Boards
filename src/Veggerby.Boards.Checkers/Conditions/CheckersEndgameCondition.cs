using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Conditions;

/// <summary>
/// Detects when a checkers game has reached a terminal state:
/// - Active player has no valid moves (loses)
/// - Active player has no pieces remaining (loses)
/// </summary>
public sealed class CheckersEndgameCondition : IGameStateCondition
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckersEndgameCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public CheckersEndgameCondition(Game game)
    {
        _game = game;
    }

    /// <summary>
    /// Evaluates whether the game has ended.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <returns>Valid if game ended, Ignore if still in progress.</returns>
    public ConditionResponse Evaluate(GameState state)
    {
        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return ConditionResponse.Ignore("No active player");
        }

        // Check if active player has any pieces remaining on board
        var activePieces = _game.Artifacts
            .OfType<Piece>()
            .Where(p => p.Owner == activePlayer)
            .Select(p => state.GetStates<PieceState>().FirstOrDefault(ps => ps.Artifact == p))
            .Where(ps => ps != null && ps.CurrentTile != null)
            .ToList();

        if (activePieces.Count == 0)
        {
            // No pieces remaining - opponent wins
            return ConditionResponse.Valid;
        }

        // TODO: Check if active player has any valid moves
        // For now, simplified - game only ends when all pieces captured
        
        // Game still in progress
        return ConditionResponse.Ignore("Game not ended");
    }
}
