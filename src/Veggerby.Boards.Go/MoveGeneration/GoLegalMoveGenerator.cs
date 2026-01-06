using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go.MoveGeneration;

/// <summary>
/// Go-specific implementation of <see cref="ILegalMoveGenerator"/> that enumerates
/// legal stone placements and pass moves for the current game state.
/// </summary>
/// <remarks>
/// <para>
/// This generator produces legal moves by:
/// <list type="bullet">
/// <item><description>Enumerating all empty intersections as potential placement candidates</description></item>
/// <item><description>Filtering out ko-forbidden placements</description></item>
/// <item><description>Filtering out suicide moves (unless capturing)</description></item>
/// <item><description>Always allowing pass moves when game is not ended</description></item>
/// </list>
/// </para>
/// <para>
/// Performance characteristics:
/// <list type="bullet">
/// <item><description>19x19 empty board: ~361 candidates tested, target &lt; 5ms</description></item>
/// <item><description>Mid-game: fewer empty intersections reduce search space</description></item>
/// <item><description>Uses lazy evaluation to avoid unnecessary allocations</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class GoLegalMoveGenerator : ILegalMoveGenerator
{
    private readonly Game _game;
    private readonly DecisionPlanMoveGenerator _baseGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoLegalMoveGenerator"/> class.
    /// </summary>
    /// <param name="engine">The game engine containing the compiled decision plan.</param>
    /// <param name="state">The current game state.</param>
    /// <exception cref="ArgumentNullException">Thrown if engine or state is null.</exception>
    public GoLegalMoveGenerator(GameEngine engine, GameState state)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);

        _game = engine.Game;
        _baseGenerator = new DecisionPlanMoveGenerator(engine, state);
    }

    /// <inheritdoc />
    public IEnumerable<IGameEvent> GetLegalMoves(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Check if game has ended
        if (state.GetStates<GameEndedState>().Any())
        {
            yield break;
        }

        // Always allow pass
        var passEvent = new PassTurnGameEvent();
        var passValidation = Validate(passEvent, state);

        if (passValidation.IsLegal)
        {
            yield return passEvent;
        }

        // Get active player (Go may not have active player state if turn sequencing not configured)
        var activePlayer = GetActivePlayer(state);

        if (activePlayer is null)
        {
            // If no active player, try to enumerate moves for all players (permissive mode)
            // This allows the generator to work even without turn sequencing configured
            foreach (var player in _game.Players)
            {
                var stone = GetNextAvailableStone(state, player);

                if (stone is not null)
                {
                    foreach (var tile in _game.Board.Tiles)
                    {
                        if (state.GetPiecesOnTile(tile).Any())
                        {
                            continue;
                        }

                        var candidateEvent = new PlaceStoneGameEvent(stone, tile);
                        var validation = Validate(candidateEvent, state);

                        if (validation.IsLegal)
                        {
                            yield return candidateEvent;
                        }
                    }

                    // Only enumerate moves for first player with available stones
                    yield break;
                }
            }

            yield break;
        }

        // Get next available stone for active player
        var nextStone = GetNextAvailableStone(state, activePlayer);

        if (nextStone is null)
        {
            // No stones available, only pass is legal
            yield break;
        }

        // Enumerate all empty tiles as potential placements
        foreach (var tile in _game.Board.Tiles)
        {
            // Skip occupied tiles
            if (state.GetPiecesOnTile(tile).Any())
            {
                continue;
            }

            // Create candidate placement
            var candidateEvent = new PlaceStoneGameEvent(nextStone, tile);

            // Validate through base generator (uses DecisionPlan + Go mutator logic)
            var validation = Validate(candidateEvent, state);

            if (validation.IsLegal)
            {
                yield return candidateEvent;
            }
        }
    }

    /// <inheritdoc />
    public MoveValidation Validate(IGameEvent @event, GameState state)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(state);

        // Check if game has ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return MoveValidation.Illegal(@event, RejectionReason.GameEnded, "Game has already ended");
        }

        // Pass moves are always legal when game is not ended
        if (@event is PassTurnGameEvent)
        {
            return MoveValidation.Legal(@event);
        }

        // Handle stone placement
        if (@event is PlaceStoneGameEvent placeEvent)
        {
            return ValidatePlacement(placeEvent, state);
        }

        // Delegate to base generator for unknown event types
        return _baseGenerator.Validate(@event, state);
    }

    /// <inheritdoc />
    public IEnumerable<IGameEvent> GetLegalMovesFor(Artifact artifact, GameState state)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentNullException.ThrowIfNull(state);

        // For Go, individual stones don't have moves until placed
        // Only the active player's unplaced stones can be placed
        if (artifact is not Piece piece)
        {
            yield break;
        }

        // Check if this piece is the next available stone for active player
        var activePlayer = GetActivePlayer(state);

        if (activePlayer is null || piece.Owner != activePlayer)
        {
            yield break;
        }

        var nextStone = GetNextAvailableStone(state, activePlayer);

        if (nextStone != piece)
        {
            // Not the next stone to be placed
            yield break;
        }

        // Return all legal placements for this stone
        foreach (var tile in _game.Board.Tiles)
        {
            if (state.GetPiecesOnTile(tile).Any())
            {
                continue;
            }

            var candidateEvent = new PlaceStoneGameEvent(piece, tile);
            var validation = Validate(candidateEvent, state);

            if (validation.IsLegal)
            {
                yield return candidateEvent;
            }
        }
    }

    /// <summary>
    /// Validates a stone placement event.
    /// </summary>
    private MoveValidation ValidatePlacement(PlaceStoneGameEvent @event, GameState state)
    {
        // Check if tile is empty
        if (state.GetPiecesOnTile(@event.Target).Any())
        {
            return MoveValidation.Illegal(@event, RejectionReason.DestinationOccupied, "Intersection is already occupied");
        }

        // Check ko rule
        var extras = state.GetExtras<GoStateExtras>();

        if (extras?.KoTileId == @event.Target.Id)
        {
            return MoveValidation.Illegal(@event, RejectionReason.RuleViolation, "Move violates ko rule (would recreate previous position)");
        }

        // Check if stone belongs to active player (if active player is configured)
        var activePlayer = GetActivePlayer(state);

        if (activePlayer is not null && @event.Stone.Owner != activePlayer)
        {
            return MoveValidation.Illegal(@event, RejectionReason.PieceNotOwned, "Stone does not belong to active player");
        }

        // Check suicide rule by simulating the move
        // We need to use the mutator logic to check if this would be suicide
        // For performance, we validate via DecisionPlan which will apply the mutator
        var baseValidation = _baseGenerator.Validate(@event, state);

        if (!baseValidation.IsLegal)
        {
            // Map the rejection to appropriate reason
            if (baseValidation.Explanation.Contains("suicide", StringComparison.OrdinalIgnoreCase) ||
                baseValidation.Explanation.Contains("liberties", StringComparison.OrdinalIgnoreCase))
            {
                return MoveValidation.Illegal(@event, RejectionReason.RuleViolation, "Suicide move (would remove own last liberty without capturing)");
            }

            return baseValidation;
        }

        return MoveValidation.Legal(@event);
    }

    /// <summary>
    /// Gets the active player from the game state.
    /// </summary>
    private Player? GetActivePlayer(GameState state)
    {
        if (state.TryGetActivePlayer(out var player))
        {
            return player;
        }

        return null;
    }

    /// <summary>
    /// Gets the next available stone for the specified player.
    /// </summary>
    /// <remarks>
    /// Returns the first stone owned by the player that is not currently placed on the board.
    /// </remarks>
    private Piece? GetNextAvailableStone(GameState state, Player player)
    {
        // Get all stones owned by player
        var playerStones = _game.GetArtifacts<Piece>().Where(p => p.Owner == player);

        // Find first stone not on board
        foreach (var stone in playerStones)
        {
            var stoneState = state.GetState<PieceState>(stone);

            // Stone is available if it has no state (not yet placed) or if it's captured
            if (stoneState is null || state.IsCaptured(stone))
            {
                // Stone not yet placed or captured
                return stone;
            }
        }

        return null;
    }
}
