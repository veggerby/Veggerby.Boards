using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.MoveGeneration;

/// <summary>
/// Chess-specific implementation of <see cref="ILegalMoveGenerator"/> that integrates
/// with the existing Chess move generation infrastructure.
/// </summary>
/// <remarks>
/// <para>
/// This generator combines:
/// <list type="bullet">
/// <item><description><see cref="ChessMoveGenerator"/>: Generates pseudo-legal moves based on piece movement patterns</description></item>
/// <item><description><see cref="ChessLegalityFilter"/>: Filters out moves that leave the king in check</description></item>
/// <item><description>Base validation: Uses <see cref="DecisionPlanMoveGenerator"/> for validation and rejection diagnostics</description></item>
/// </list>
/// </para>
/// <para>
/// The generator converts between the Chess module's <see cref="PseudoMove"/> representation
/// and the core engine's <see cref="MovePieceGameEvent"/> representation.
/// </para>
/// </remarks>
public sealed class ChessLegalMoveGenerator : ILegalMoveGenerator
{
    private readonly Game _game;
    private readonly ChessMoveGenerator _moveGenerator;
    private readonly ChessLegalityFilter _legalityFilter;
    private readonly DecisionPlanMoveGenerator _baseGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChessLegalMoveGenerator"/> class.
    /// </summary>
    /// <param name="engine">The game engine containing the compiled decision plan.</param>
    /// <param name="state">The current game state.</param>
    /// <exception cref="ArgumentNullException">Thrown if engine or state is null.</exception>
    public ChessLegalMoveGenerator(GameEngine engine, GameState state)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);

        _game = engine.Game;
        _moveGenerator = new ChessMoveGenerator(_game);
        _legalityFilter = new ChessLegalityFilter(_game);
        _baseGenerator = new DecisionPlanMoveGenerator(engine, state);
    }

    /// <inheritdoc />
    public IEnumerable<IGameEvent> GetLegalMoves(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Generate legal moves using the legality filter
        var legalMoves = _legalityFilter.GenerateLegalMoves(state);

        // Convert PseudoMove to IGameEvent
        foreach (var move in legalMoves)
        {
            var gameEvent = ConvertToGameEvent(move);

            if (gameEvent is not null)
            {
                yield return gameEvent;
            }
        }
    }

    /// <inheritdoc />
    public MoveValidation Validate(IGameEvent @event, GameState state)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(state);

        // First check if this is a move event we can handle
        if (@event is not MovePieceGameEvent moveEvent)
        {
            // Delegate to base generator for non-move events
            return _baseGenerator.Validate(@event, state);
        }

        // Check if the move would be in the set of legal moves
        // This is more efficient than simulating and checking for all rules
        var pseudoMoves = _moveGenerator.Generate(state);
        var matchingPseudoMove = FindMatchingPseudoMove(pseudoMoves, moveEvent);

        if (matchingPseudoMove is null)
        {
            // Move doesn't match any pseudo-legal move pattern
            return MoveValidation.Illegal(@event, RejectionReason.InvalidPattern, "Move does not match piece movement pattern");
        }

        // Check if it would leave king in check
        var legalMoves = _legalityFilter.FilterLegalMoves(state, [matchingPseudoMove]);

        if (legalMoves.Count == 0)
        {
            return MoveValidation.Illegal(@event, RejectionReason.RuleViolation, "Move would leave or place own king in check");
        }

        // Move is legal
        return MoveValidation.Legal(@event);
    }

    /// <inheritdoc />
    public IEnumerable<IGameEvent> GetLegalMovesFor(Artifact artifact, GameState state)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentNullException.ThrowIfNull(state);

        // Check if this is a piece
        if (artifact is not Piece piece)
        {
            yield break;
        }

        // Generate all legal moves
        var allLegalMoves = _legalityFilter.GenerateLegalMoves(state);

        // Filter to moves involving this piece
        foreach (var move in allLegalMoves)
        {
            if (move.Piece == piece)
            {
                var gameEvent = ConvertToGameEvent(move);

                if (gameEvent is not null)
                {
                    yield return gameEvent;
                }
            }
        }
    }

    /// <summary>
    /// Converts a <see cref="PseudoMove"/> to a <see cref="MovePieceGameEvent"/>.
    /// </summary>
    private MovePieceGameEvent? ConvertToGameEvent(PseudoMove move)
    {
        // Build the tile path
        var path = BuildTilePath(move.From, move.To);

        if (path is null)
        {
            return null;
        }

        return new MovePieceGameEvent(move.Piece, path);
    }

    /// <summary>
    /// Builds a TilePath from source to destination tile.
    /// </summary>
    private TilePath? BuildTilePath(Tile from, Tile to)
    {
        // For chess, we typically use simple two-tile paths (from -> to)
        // The engine will resolve intermediate tiles via pattern matching

        // Try to find a direct relation
        var directRelations = _game.Board.TileRelations
            .Where(r => r.From == from && r.To == to)
            .ToList();

        if (directRelations.Count > 0)
        {
            // Use the first matching relation
            return new TilePath([directRelations[0]]);
        }

        // No direct relation found - for chess, moves typically go directly
        // from one square to another via pattern matching, so if there's no
        // direct relation, we can't build a simple path
        // We could implement multi-hop path finding, but for now return null
        return null;
    }

    /// <summary>
    /// Finds a PseudoMove that matches the given MovePieceGameEvent.
    /// </summary>
    private static PseudoMove? FindMatchingPseudoMove(IReadOnlyCollection<PseudoMove> pseudoMoves, MovePieceGameEvent moveEvent)
    {
        foreach (var pseudoMove in pseudoMoves)
        {
            if (pseudoMove.Piece == moveEvent.Piece &&
                pseudoMove.From == moveEvent.From &&
                pseudoMove.To == moveEvent.To)
            {
                return pseudoMove;
            }
        }

        return null;
    }
}
