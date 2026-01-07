using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon.MoveGeneration;

/// <summary>
/// Backgammon-specific implementation of <see cref="ILegalMoveGenerator"/> that generates
/// legal dice-driven piece moves including bar re-entry and bearing off.
/// </summary>
/// <remarks>
/// <para>
/// This generator produces legal moves by:
/// <list type="bullet">
/// <item><description>Rolling dice when no dice values are present</description></item>
/// <item><description>Enumerating piece moves constrained by available dice values</description></item>
/// <item><description>Prioritizing bar re-entry when pieces are on the bar</description></item>
/// <item><description>Allowing bearing off when all pieces are in home board</description></item>
/// </list>
/// </para>
/// <para>
/// Performance characteristics:
/// <list type="bullet">
/// <item><description>Typical position: O(pieces × dice_values) candidate moves</description></item>
/// <item><description>Bar position: O(dice_values) re-entry attempts only</description></item>
/// <item><description>Uses lazy evaluation to avoid unnecessary allocations</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class BackgammonLegalMoveGenerator : ILegalMoveGenerator
{
    private readonly Game _game;
    private readonly DecisionPlanMoveGenerator _baseGenerator;
    private readonly Dice _dice1;
    private readonly Dice _dice2;
    private readonly Tile? _bar;
    private readonly Tile? _homeWhite;
    private readonly Tile? _homeBlack;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgammonLegalMoveGenerator"/> class.
    /// </summary>
    /// <param name="engine">The game engine containing the compiled decision plan.</param>
    /// <param name="state">The current game state.</param>
    /// <exception cref="ArgumentNullException">Thrown if engine or state is null.</exception>
    public BackgammonLegalMoveGenerator(GameEngine engine, GameState state)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);

        _game = engine.Game;
        _baseGenerator = new DecisionPlanMoveGenerator(engine, state);

        // Get required artifacts
        _dice1 = _game.GetArtifact<Dice>("dice-1") ?? throw new InvalidOperationException("Backgammon requires dice-1");
        _dice2 = _game.GetArtifact<Dice>("dice-2") ?? throw new InvalidOperationException("Backgammon requires dice-2");
        _bar = _game.GetTile("bar");
        _homeWhite = _game.GetTile("home-white");
        _homeBlack = _game.GetTile("home-black");
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

        // Check if we need to roll dice
        var diceStates = GetDiceValues(state);

        if (diceStates.Count == 0)
        {
            // No dice values available - can't enumerate specific moves
            // The base generator will handle the RollDiceGameEvent validation
            yield break;
        }

        // Get active player
        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer is null)
        {
            yield break;
        }

        // Check if we have pieces on the bar
        var piecesOnBar = _bar is not null
            ? state.GetPiecesOnTile(_bar, activePlayer).ToList()
            : new List<Piece>();

        if (piecesOnBar.Count > 0)
        {
            // Must re-enter from bar first
            foreach (var diceValue in diceStates)
            {
                var entryMoves = GenerateBarReEntry(state, piecesOnBar[0], activePlayer, diceValue);

                foreach (var move in entryMoves)
                {
                    var validation = Validate(move, state);

                    if (validation.IsLegal)
                    {
                        yield return move;
                    }
                }
            }

            yield break;
        }

        // Generate normal piece moves using available dice
        var playerPieces = new List<Piece>();

        foreach (var piece in _game.GetArtifacts<Piece>())
        {
            if (piece.Owner == activePlayer)
            {
                playerPieces.Add(piece);
            }
        }

        foreach (var piece in playerPieces)
        {
            var pieceState = state.GetState<PieceState>(piece);

            if (pieceState is null || pieceState.CurrentTile is null)
            {
                continue;
            }

            // Skip pieces on bar (already handled) or in home
            if (_bar is not null && pieceState.CurrentTile == _bar)
            {
                continue;
            }

            var homeTile = activePlayer.Id == "white" ? _homeWhite : _homeBlack;

            if (homeTile is not null && pieceState.CurrentTile == homeTile)
            {
                continue;
            }

            // Generate moves for each available dice value
            foreach (var diceValue in diceStates)
            {
                var moves = GeneratePieceMoves(state, piece, pieceState.CurrentTile, activePlayer, diceValue);

                foreach (var move in moves)
                {
                    var validation = Validate(move, state);

                    if (validation.IsLegal)
                    {
                        yield return move;
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public MoveValidation Validate(IGameEvent @event, GameState state)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(state);

        // Delegate to base generator for validation
        return _baseGenerator.Validate(@event, state);
    }

    /// <inheritdoc />
    public IEnumerable<IGameEvent> GetLegalMovesFor(Artifact artifact, GameState state)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentNullException.ThrowIfNull(state);

        // For Backgammon, only pieces can have moves
        if (artifact is not Piece piece)
        {
            yield break;
        }

        // Check if game has ended
        if (state.GetStates<GameEndedState>().Any())
        {
            yield break;
        }

        // Get piece state
        var pieceState = state.GetState<PieceState>(piece);

        if (pieceState is null || pieceState.CurrentTile is null)
        {
            yield break;
        }

        // Get dice values
        var diceStates = GetDiceValues(state);

        if (diceStates.Count == 0)
        {
            yield break;
        }

        // Get active player
        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer is null)
        {
            yield break;
        }

        // Only show moves for active player's pieces
        if (piece.Owner != activePlayer)
        {
            yield break;
        }

        // Generate moves for this piece
        foreach (var diceValue in diceStates)
        {
            var moves = _bar is not null && pieceState.CurrentTile == _bar
                ? GenerateBarReEntry(state, piece, activePlayer, diceValue)
                : GeneratePieceMoves(state, piece, pieceState.CurrentTile, activePlayer, diceValue);

            foreach (var move in moves)
            {
                var validation = Validate(move, state);

                if (validation.IsLegal)
                {
                    yield return move;
                }
            }
        }
    }

    /// <summary>
    /// Gets the current dice values from the game state.
    /// </summary>
    private List<int> GetDiceValues(GameState state)
    {
        var values = new List<int>();
        var dice1State = state.GetState<DiceState<int>>(_dice1);
        var dice2State = state.GetState<DiceState<int>>(_dice2);

        if (dice1State is not null && dice1State.CurrentValue > 0)
        {
            values.Add(dice1State.CurrentValue);
        }

        if (dice2State is not null && dice2State.CurrentValue > 0)
        {
            // Handle doubles - add both dice twice
            if (dice1State is not null && dice1State.CurrentValue == dice2State.CurrentValue)
            {
                values.Add(dice2State.CurrentValue);
                values.Add(dice2State.CurrentValue);
                values.Add(dice2State.CurrentValue);
            }
            else
            {
                values.Add(dice2State.CurrentValue);
            }
        }

        return values;
    }

    /// <summary>
    /// Generates bar re-entry moves for a piece on the bar.
    /// </summary>
    private IEnumerable<MovePieceGameEvent> GenerateBarReEntry(GameState state, Piece piece, Player player, int diceValue)
    {
        if (_bar is null)
        {
            yield break;
        }

        // Determine entry direction and starting point
        var direction = player.Id == "white" ? "clockwise" : "counterclockwise";
        var startPoint = player.Id == "white" ? "point-1" : "point-24";

        // Calculate entry point based on dice value
        var entryPoint = CalculateDestinationPoint(startPoint, direction, diceValue - 1);

        if (entryPoint is null)
        {
            yield break;
        }

        // Build path from bar to entry point
        var path = BuildPath(_bar, entryPoint, direction, diceValue);

        if (path is not null)
        {
            yield return new MovePieceGameEvent(piece, path);
        }
    }

    /// <summary>
    /// Generates normal piece moves using the specified dice value.
    /// </summary>
    private IEnumerable<MovePieceGameEvent> GeneratePieceMoves(GameState state, Piece piece, Tile fromTile, Player player, int diceValue)
    {
        var direction = player.Id == "white" ? "clockwise" : "counterclockwise";
        var homeTile = player.Id == "white" ? _homeWhite : _homeBlack;

        // Calculate destination
        var destination = CalculateDestinationPoint(fromTile.Id, direction, diceValue);

        // Check for bearing off
        if (destination is null && homeTile is not null)
        {
            // Might be able to bear off
            if (CanBearOff(state, piece, player))
            {
                var pathToHome = BuildPath(fromTile, homeTile, direction, diceValue);

                if (pathToHome is not null)
                {
                    yield return new MovePieceGameEvent(piece, pathToHome);
                }
            }

            yield break;
        }

        if (destination is not null)
        {
            var path = BuildPath(fromTile, destination, direction, diceValue);

            if (path is not null)
            {
                yield return new MovePieceGameEvent(piece, path);
            }
        }
    }

    /// <summary>
    /// Calculates the destination point given a starting point, direction, and steps.
    /// </summary>
    private Tile? CalculateDestinationPoint(string fromPointId, string direction, int steps)
    {
        var currentTile = _game.GetTile(fromPointId);

        if (currentTile is null)
        {
            return null;
        }

        // Navigate through the board
        for (var i = 0; i < steps; i++)
        {
            TileRelation? nextRelation = null;

            foreach (var relation in _game.Board.TileRelations)
            {
                if (relation.From == currentTile && relation.Direction.Id == direction)
                {
                    nextRelation = relation;
                    break;
                }
            }

            if (nextRelation is null)
            {
                return null;
            }

            currentTile = nextRelation.To;
        }

        return currentTile;
    }

    /// <summary>
    /// Builds a tile path from source to destination.
    /// </summary>
    private TilePath? BuildPath(Tile from, Tile to, string direction, int steps)
    {
        var relations = new List<TileRelation>();
        var currentTile = from;

        for (var i = 0; i < steps; i++)
        {
            TileRelation? nextRelation = null;

            foreach (var relation in _game.Board.TileRelations)
            {
                if (relation.From == currentTile && relation.Direction.Id == direction)
                {
                    nextRelation = relation;
                    break;
                }
            }

            if (nextRelation is null)
            {
                // Try to connect directly to destination if it's the last step
                if (i == steps - 1)
                {
                    foreach (var relation in _game.Board.TileRelations)
                    {
                        if (relation.From == currentTile && relation.To == to)
                        {
                            nextRelation = relation;
                            break;
                        }
                    }
                }

                if (nextRelation is null)
                {
                    return null;
                }
            }

            relations.Add(nextRelation);
            currentTile = nextRelation.To;

            if (currentTile == to && i < steps - 1)
            {
                // Reached destination early
                break;
            }
        }

        return relations.Count > 0 ? new TilePath(relations) : null;
    }

    /// <summary>
    /// Checks if a piece can bear off (all player's pieces in home board).
    /// </summary>
    private bool CanBearOff(GameState state, Piece piece, Player player)
    {
        // Define home board range
        var homeStart = player.Id == "white" ? 19 : 1;
        var homeEnd = player.Id == "white" ? 24 : 6;

        foreach (var p in _game.GetArtifacts<Piece>())
        {
            if (p.Owner != player)
            {
                continue;
            }

            var pieceState = state.GetState<PieceState>(p);

            if (pieceState is null || pieceState.CurrentTile is null)
            {
                continue;
            }

            // Check if piece is on bar
            if (_bar is not null && pieceState.CurrentTile == _bar)
            {
                return false;
            }

            // Check if piece is in home board
            if (pieceState.CurrentTile.Id.StartsWith("point-", StringComparison.Ordinal))
            {
                var pointNum = int.Parse(pieceState.CurrentTile.Id.AsSpan(6));

                if (player.Id == "white" && (pointNum < homeStart || pointNum > homeEnd))
                {
                    return false;
                }

                if (player.Id == "black" && (pointNum < homeStart || pointNum > homeEnd))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
