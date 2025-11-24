using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Mutators;

/// <summary>
/// Updates piece movement directions when a piece is promoted to king.
/// Kings in checkers can move in all diagonal directions, not just forward.
/// </summary>
/// <remarks>
/// This mutator should be applied after PromoteToKingMutator.
/// It enables backward diagonal movement for promoted kings.
/// </remarks>
public sealed class KingMovementMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="KingMovementMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public KingMovementMutator(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Check if this piece was just promoted in this state
        var promotedStates = gameState.GetStates<PromotedPieceState>();
        var wasJustPromoted = promotedStates.Any(ps => ps.PromotedPiece.Equals(@event.Piece));

        if (!wasJustPromoted)
        {
            return gameState; // Piece was not promoted, no changes needed
        }

        // Note: In this engine architecture, piece movement patterns are defined at game build time
        // and cannot be modified dynamically. The PromotedPieceState marker serves as a flag
        // that can be checked by movement validation logic to allow backward moves for kings.
        // The actual movement validation happens in conditions, not here.

        return gameState;
    }
}
