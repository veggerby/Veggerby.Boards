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

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return ConditionResponse.Ignore("No active player");
        }

        // Check if active player has any pieces remaining on board (not captured)
        var activePieces = _game.Artifacts
            .OfType<Piece>()
            .Where(p => p.Owner.Id == activePlayer.Id && !state.IsCaptured(p))
            .ToList();

        if (activePieces.Count == 0)
        {
            // No pieces remaining - opponent wins
            return ConditionResponse.Valid;
        }

        // Known Limitation: No-move detection not yet implemented.
        // See README.md Known Limitations section.
        // Future implementation will:
        // 1. Enumerate all possible moves for each active piece
        // 2. If no legal moves exist, return Valid (game over)
        // 3. Otherwise, return Ignore (game continues)

        // Game still in progress
        return ConditionResponse.Ignore("Game not ended");
    }
}
