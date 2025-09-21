using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Builder.Artifacts;
using Veggerby.Boards.Builder.Phases;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Declarative compiler for a concrete game module combining static structure (board, tiles, relations,
/// players, artifacts) with initial state (piece positions, dice values) and flow logic (phases + rules).
/// </summary>
/// <remarks>
/// Subclasses implement <see cref="Build"/> to register definitions via protected <c>Add*</c>/ <c>With*</c> helpers.
/// Calling <see cref="Compile"/> is idempotent; subsequent calls return the cached <c>GameProgress</c> representing
/// initial engine + state. The builder is not thread-safe and intended for one-time composition at startup.
/// </remarks>
public abstract class GameBuilder
{
    /// <summary>
    /// Gets or sets the identifier of the board to create. Must be assigned during <see cref="Build"/>.
    /// </summary>
    protected string BoardId { get; set; }
    private readonly IList<PlayerDefinition> _playerDefinitions = [];
    private readonly IList<TileDefinition> _tileDefinitions = [];
    private readonly IList<DirectionDefinition> _directionDefinitions = [];
    private readonly IList<DiceDefinition> _diceDefinitions = [];
    private readonly IList<TileRelationDefinition> _tileRelationDefinitions = [];
    private readonly IList<PieceDefinition> _pieceDefinitions = [];
    private readonly IList<PieceDirectionPatternDefinition> _pieceDirectionPatternDefinitions = [];
    private readonly IList<ArtifactDefinition> _artifactDefinitions = [];
    private readonly IList<GamePhaseDefinition> _gamePhaseDefinitions = [];

    private Tile CreateTile(TileDefinition tile)
    {
        return new Tile(tile.TileId);
    }

    private Direction CreateTileRelationDirection(DirectionDefinition direction)
    {
        return new Direction(direction.DirectionId);
    }

    private static TileRelation CreateTileRelation(TileRelationDefinition relation, IEnumerable<Tile> tiles, IEnumerable<Direction> directions)
    {
        var sourceTile = tiles.Single(x => string.Equals(x.Id, relation.FromTileId));
        var destinationTile = tiles.Single(x => string.Equals(x.Id, relation.ToTileId));
        var direction = directions.Single(x => string.Equals(x.Id, relation.DirectionId));
        return new TileRelation(sourceTile, destinationTile, direction);
    }

    private Artifact CreateDice(DiceDefinition dice)
    {
        return new Dice(dice.DiceId);
    }

    private Artifact CreatePiece(PieceDefinition piece, IEnumerable<PieceDirectionPatternDefinition> pattern, IEnumerable<Direction> directions, IEnumerable<Player> players)
    {
        var player = !string.IsNullOrEmpty(piece.PlayerId)
            ? players.SingleOrDefault(x => string.Equals(x.Id, piece.PlayerId))
            : null;

        var patterns = pattern.Where(x => x.PieceId == piece.PieceId).ToList();

        return new Piece(piece.PieceId, player, patterns.Select(x => CreatePattern(x, directions)));
    }

    private static IPattern CreatePattern(PieceDirectionPatternDefinition piece, IEnumerable<Direction> directions)
    {
        var patternDirections = piece.DirectionIds.Select(directionId => directions.Single(x => string.Equals(x.Id, directionId))).ToList();

        if (!patternDirections.Any())
        {
            return new NullPattern();
        }

        if (patternDirections.Count() == 1)
        {
            return new DirectionPattern(patternDirections.Single(), piece.IsRepeatable);
        }

        return piece.IsRepeatable
            ? new MultiDirectionPattern(patternDirections, true)
            : (IPattern)new FixedPattern(patternDirections);
    }

    private static Artifact CreateArtifact(ArtifactDefinition artifact)
    {
        return artifact.Factory(artifact.ArtifactId);
    }

    /// <summary>
    /// Adds a new player definition.
    /// </summary>
    /// <param name="playerId">Unique player identifier.</param>
    /// <returns>Fluent player definition.</returns>
    protected PlayerDefinition AddPlayer(string playerId)
    {
        var player = new PlayerDefinition(this).WithId(playerId);
        _playerDefinitions.Add(player);
        return player;
    }

    /// <summary>
    /// Gets an existing player definition for further configuration.
    /// </summary>
    /// <param name="playerId">Identifier of the previously added player.</param>
    /// <returns>Player definition.</returns>
    protected PlayerDefinition WithPlayer(string playerId)
    {
        return _playerDefinitions.Single(x => string.Equals(x.PlayerId, playerId));
    }

    /// <summary>
    /// Adds a new tile definition.
    /// </summary>
    /// <param name="tileId">Tile identifier.</param>
    /// <returns>Tile definition.</returns>
    protected TileDefinition AddTile(string tileId)
    {
        var tile = new TileDefinition(this).WithId(tileId);
        _tileDefinitions.Add(tile);
        return tile;
    }

    /// <summary>
    /// Gets an existing tile definition for additional configuration.
    /// </summary>
    /// <param name="tileId">Tile identifier.</param>
    /// <returns>Tile definition.</returns>
    protected TileDefinition WithTile(string tileId)
    {
        return _tileDefinitions.Single(x => string.Equals(x.TileId, tileId));
    }

    /// <summary>
    /// Adds a direction definition.
    /// </summary>
    /// <param name="directionId">Direction identifier.</param>
    /// <returns>Direction definition.</returns>
    protected DirectionDefinition AddDirection(string directionId)
    {
        var direction = new DirectionDefinition(this).WithId(directionId);
        _directionDefinitions.Add(direction);
        return direction;
    }

    /// <summary>
    /// Adds a dice definition.
    /// </summary>
    /// <param name="diceId">Dice identifier.</param>
    /// <returns>Dice definition.</returns>
    protected DiceDefinition AddDice(string diceId)
    {
        var dice = new DiceDefinition(this).WithId(diceId);
        _diceDefinitions.Add(dice);
        return dice;
    }

    /// <summary>
    /// Gets an existing dice definition.
    /// </summary>
    /// <param name="diceId">Dice identifier.</param>
    /// <returns>Dice definition.</returns>
    protected DiceDefinition WithDice(string diceId)
    {
        return _diceDefinitions.Single(x => string.Equals(x.DiceId, diceId));
    }

    /// <summary>
    /// Adds a piece definition.
    /// </summary>
    /// <param name="pieceId">Piece identifier.</param>
    /// <returns>Piece definition.</returns>
    protected PieceDefinition AddPiece(string pieceId)
    {
        var piece = new PieceDefinition(this).WithId(pieceId);
        _pieceDefinitions.Add(piece);
        return piece;
    }

    /// <summary>
    /// Gets an existing piece definition.
    /// </summary>
    /// <param name="pieceId">Piece identifier.</param>
    /// <returns>Piece definition.</returns>
    protected PieceDefinition WithPiece(string pieceId)
    {
        return _pieceDefinitions.Single(x => string.Equals(x.PieceId, pieceId));
    }

    /// <summary>
    /// Adds a generic artifact definition (custom runtime component backed by an <see cref="Artifact"/> subtype).
    /// </summary>
    /// <param name="artifactId">Artifact identifier.</param>
    /// <returns>Artifact definition.</returns>
    protected ArtifactDefinition AddArtifact(string artifactId)
    {
        var artifact = new ArtifactDefinition(this).WithId(artifactId);
        _artifactDefinitions.Add(artifact);
        return artifact;
    }

    /// <summary>
    /// Gets an existing artifact definition.
    /// </summary>
    /// <param name="artifactId">Artifact identifier.</param>
    /// <returns>Artifact definition.</returns>
    protected ArtifactDefinition WithArtifact(string artifactId)
    {
        return _artifactDefinitions.Single(x => string.Equals(x.ArtifactId, artifactId));
    }

    internal void Add(PieceDirectionPatternDefinition pattern)
    {
        _pieceDirectionPatternDefinitions.Add(pattern);
    }

    internal void Add(TileRelationDefinition relation)
    {
        _tileRelationDefinitions.Add(relation);
    }

    private readonly IDictionary<string, int?> _diceState = new Dictionary<string, int?>();

    private readonly IDictionary<string, string> _piecePositions = new Dictionary<string, string>();

    internal void AddDiceState(string diceId, int? value)
    {
        _diceState.Add(diceId, value);
    }

    internal void AddPieceOnTile(string pieceId, string tileId)
    {
        _piecePositions.Add(pieceId, tileId);
    }

    /// <summary>
    /// Adds a game phase definition which can attach rules, conditions and event handlers.
    /// </summary>
    /// <param name="label">Human readable label (not used functionally).</param>
    /// <returns>Phase definition fluent object.</returns>
    protected IGamePhaseDefinition AddGamePhase(string label) // label not used for anything, just to document in builder
    {
        var gamePhase = new GamePhaseDefinition(this, label);
        _gamePhaseDefinitions.Add(gamePhase);
        return gamePhase;
    }

    /// <summary>
    /// Override to register all static game definitions (board id, tiles, relations, players, pieces, dice, artifacts, phases, initial placements) using the provided <c>Add*</c> helpers.
    /// </summary>
    protected abstract void Build();

    private GameProgress _initialGameProgress;

    /// <summary>
    /// Compiles the game definition into an executable <see cref="GameEngine"/> + initial <see cref="GameState"/>.
    /// </summary>
    /// <remarks>
    /// Invocation is idempotent; the first call performs compilation and caches the resulting <see cref="GameProgress"/> which is returned on subsequent calls.
    /// </remarks>
    /// <returns>Initial game progress containing engine and initial state.</returns>
    public GameProgress Compile()
    {
        if (_initialGameProgress is not null)
        {
            return _initialGameProgress;
        }

        Build();

        // compile Game

        var players = _playerDefinitions.Select(x => new Player(x.PlayerId));
        var tiles = _tileDefinitions.Select(CreateTile).ToArray();
        var directions = _directionDefinitions.Select(CreateTileRelationDirection).ToArray();
        var dice = _diceDefinitions.Select(CreateDice).ToArray();
        var relations = _tileRelationDefinitions.Select(x => CreateTileRelation(x, tiles, directions)).ToArray();
        var pieces = _pieceDefinitions.Select(x => CreatePiece(x, _pieceDirectionPatternDefinitions, directions, players)).ToArray();
        var artifacts = _artifactDefinitions.Select(x => CreateArtifact(x)).ToArray();

        var board = new Board(BoardId, relations);
        var game = new Game(board, players, pieces.Concat(dice).Concat(artifacts));

        // compile Initial state

        var pieceStates = _piecePositions
            .Select(x => new PieceState(game.GetPiece(x.Key), game.GetTile(x.Value)))
            .ToList();

        var diceStates = _diceState
            .Select(x => x.Value is null
                ? (IArtifactState)new NullDiceState(game.GetArtifact<Dice>(x.Key))
                : (IArtifactState)new DiceState<int>(game.GetArtifact<Dice>(x.Key), x.Value.Value))
            .ToList();

        var initialGameState = GameState.New([.. pieceStates, .. diceStates]);

        // compile GamePhase root

        GamePhase gamePhaseRoot = null;

        if (_gamePhaseDefinitions.Any())
        {
            var parent = GamePhase.NewParent(1, null, null);

            var number = 1;
            foreach (var gamePhaseDefinition in _gamePhaseDefinitions)
            {
                var phase = gamePhaseDefinition.Build(number, game, parent);
                number = Math.Max(number, phase.Number) + 1;
            }

            gamePhaseRoot = parent;
        }
        else
        {
            // null pattern, no rules or event handlers
            gamePhaseRoot = GamePhase.New(1, "n/a", new States.Conditions.NullGameStateCondition(), GameEventRule<IGameEvent>.Null);
        }

        // combine
        var engine = new GameEngine(game, gamePhaseRoot);
        _initialGameProgress = new GameProgress(engine, initialGameState, null);

        return _initialGameProgress;
    }
}