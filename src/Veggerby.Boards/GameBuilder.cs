using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Builder.Artifacts;
using Veggerby.Boards.Builder.Phases;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Internal;
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
    protected string? BoardId { get; set; }
    private readonly IList<PlayerDefinition> _playerDefinitions = [];
    private readonly IList<TileDefinition> _tileDefinitions = [];
    private readonly IList<DirectionDefinition> _directionDefinitions = [];
    private readonly IList<DiceDefinition> _diceDefinitions = [];
    private readonly IList<TileRelationDefinition> _tileRelationDefinitions = [];
    private readonly IList<PieceDefinition> _pieceDefinitions = [];
    private readonly IList<PieceDirectionPatternDefinition> _pieceDirectionPatternDefinitions = [];
    private readonly IList<ArtifactDefinition> _artifactDefinitions = [];
    private readonly IList<GamePhaseDefinition> _gamePhaseDefinitions = [];
    private readonly IList<object> _extrasStates = new List<object>();
    private readonly IList<(string PlayerId, bool IsActive)> _activePlayerAssignments = new List<(string, bool)>();

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

    private static Artifact CreatePiece(PieceDefinition piece, IEnumerable<PieceDirectionPatternDefinition> pattern, IEnumerable<Direction> directions, IEnumerable<Player> players)
    {
        var player = !string.IsNullOrEmpty(piece.PlayerId)
            ? players.SingleOrDefault(x => string.Equals(x.Id, piece.PlayerId))
            : null;

        var patterns = pattern.Where(x => x.PieceId == piece.PieceId).ToList();

        if (player is null)
        {
            throw new InvalidOperationException($"Piece definition '{piece.PieceId}' references unknown player '{piece.PlayerId}'.");
        }
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
    /// Declares an initial active player projection. Multiple calls may be made (typically one per player) with exactly one marked active.
    /// </summary>
    /// <param name="playerId">Player identifier previously added via <see cref="AddPlayer"/>.</param>
    /// <param name="isActive">Whether the player starts active.</param>
    /// <remarks>
    /// Active player projections are optional; when absent, modules may infer sequencing implicitly (legacy behavior). Chess now uses explicit projections.
    /// </remarks>
    protected void WithActivePlayer(string playerId, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new ArgumentException("Player id must be supplied", nameof(playerId));
        }

        // Defer validation (existence, uniqueness) until compile to allow out-of-order declarations.
        _activePlayerAssignments.Add((playerId, isActive));
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

    private GameProgress? _initialGameProgress;

    private IEvaluationObserver _observer = NullEvaluationObserver.Instance;
    private ulong? _seed; // deterministic RNG seed (optional)

    /// <summary>
    /// Sets a custom evaluation observer for instrumentation (optional).
    /// </summary>
    /// <param name="observer">Observer instance (null ignored, retains existing).</param>
    /// <returns>Builder for fluent chaining.</returns>
    public GameBuilder WithObserver(IEvaluationObserver observer)
    {
        if (observer is not null)
        {
            _observer = observer;
        }

        return this;
    }

    /// <summary>
    /// Assigns a deterministic random seed for the initial <see cref="GameState"/>.
    /// </summary>
    /// <param name="seed">Seed value (0 permitted but discouraged as sentinel ambiguity; still applied).</param>
    /// <returns>Builder for fluent chaining.</returns>
    public GameBuilder WithSeed(ulong seed)
    {
        _seed = seed;
        return this;
    }

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
        var artifacts = _artifactDefinitions.Select(x => CreateArtifact(x)).ToList();

        // Add synthetic artifacts for extras states (one per extras type) so they participate in hashing & diffs deterministically.
        var extrasArtifacts = new Dictionary<Type, Artifact>();
        foreach (var extras in _extrasStates)
        {
            var t = extras.GetType();
            // stable id includes full type name for uniqueness across modules
            var id = $"extras-{t.FullName}";
            var art = new ExtrasArtifact(id);
            extrasArtifacts[t] = art;
            artifacts.Add(art);
        }

        // Shadow mode turn timeline artifact (single instance). Only emitted when sequencing enabled.
    TurnArtifact? turnArtifact = null;
        if (Internal.FeatureFlags.EnableTurnSequencing)
        {
            turnArtifact = new TurnArtifact("turn-timeline");
            artifacts.Add(turnArtifact);
        }

        if (BoardId is null)
        {
            throw new InvalidOperationException("BoardId must be configured before building the game.");
        }
        var board = new Board(BoardId, relations);
    var game = new Game(board, players, pieces.Concat(dice).Concat(artifacts));

        // compile Initial state

        var pieceStates = _piecePositions
            .Select(x =>
            {
                var p = game.GetPiece(x.Key) ?? throw new InvalidOperationException($"Unknown piece id '{x.Key}' in initial positions.");
                var t = game.GetTile(x.Value) ?? throw new InvalidOperationException($"Unknown tile id '{x.Value}' in initial positions.");
                return new PieceState(p, t);
            })
            .ToList();

        var diceStates = _diceState
            .Select(x =>
            {
                var d = game.GetArtifact<Dice>(x.Key) ?? throw new InvalidOperationException($"Unknown dice id '{x.Key}' in initial dice state.");
                return x.Value is null
                    ? (IArtifactState)new NullDiceState(d)
                    : (IArtifactState)new DiceState<int>(d, x.Value.Value);
            })
            .ToList();

        // Seed base states collection (pieces + dice)
        var baseStates = new List<IArtifactState>();
        baseStates.AddRange(pieceStates);
        baseStates.AddRange(diceStates);

        // Materialize ActivePlayerState projections when declared.
        if (_activePlayerAssignments.Any())
        {
            foreach (var (PlayerId, IsActive) in _activePlayerAssignments)
            {
                var player = game.Players.SingleOrDefault(p => string.Equals(p.Id, PlayerId));
                if (player is null)
                {
                    throw new InvalidOperationException($"Active player declaration references unknown player '{PlayerId}'.");
                }
                baseStates.Add(new ActivePlayerState(player, IsActive));
            }

            // Validate exactly one active when any declarations exist.
            var activeCount = _activePlayerAssignments.Count(a => a.IsActive);
            if (activeCount != 1)
            {
                throw new InvalidOperationException($"Exactly one active player must be declared; found {activeCount}.");
            }
        }

        // Materialize extras states into artifact states
        foreach (var extras in _extrasStates)
        {
            var t = extras.GetType();
            if (extrasArtifacts.TryGetValue(t, out var art))
            {
                // Wrap via generic runtime constructed ExtrasState<T>
                var extrasStateType = typeof(ExtrasState<>).MakeGenericType(t);
                if (Activator.CreateInstance(extrasStateType, art, extras) is IArtifactState state)
                {
                    baseStates.Add(state);
                }
            }
        }

        // Inject initial TurnState (turn 1, Start segment) only when sequencing enabled.
        if (turnArtifact is not null)
        {
            var initialTurnState = new TurnState(turnArtifact, 1, TurnSegment.Start, 0);
            baseStates.Add(initialTurnState);
        }

        var initialGameState = _seed.HasValue
            ? GameState.New([.. baseStates], Random.XorShiftRandomSource.Create(_seed.Value))
            : GameState.New([.. baseStates]);

        // compile GamePhase root

    GamePhase? gamePhaseRoot = null;

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
        // DecisionPlan always compiled (legacy traversal removed)
        var decisionPlan = DecisionPlan.Compile(gamePhaseRoot);

        // NEW capability wiring (sealed, non-leaky): Topology + PathResolver + AccelerationContext
        var shape = Internal.Layout.BoardShape.Build(game.Board);
        var topology = new Internal.Topology.BoardShapeTopologyAdapter(shape);

    Internal.Paths.IPathResolver? pathResolver = null;
        if (FeatureFlags.EnableCompiledPatterns)
        {
            var table = Flows.Patterns.PatternCompiler.Compile(game);
            Internal.Compiled.BoardAdjacencyCache? adjacency = null;
            if (FeatureFlags.EnableCompiledPatternsAdjacencyCache)
            {
                adjacency = Internal.Compiled.BoardAdjacencyCache.Build(game.Board);
            }

            var resolver = new Flows.Patterns.CompiledPatternResolver(table, game.Board, adjacency, shape);
            var adapter = new Internal.Paths.CompiledPathResolverAdapter(resolver);
            pathResolver = adapter;
        }
        else
        {
            // Fallback simple visitor-based resolver
            pathResolver = new Internal.Paths.SimplePatternPathResolver(game.Board);
        }

        // Acceleration context selection
        Internal.Acceleration.IAccelerationContext accelerationContext;
        var tileCount = game.Board.Tiles.Count();
        if (FeatureFlags.EnableBitboards && tileCount <= 128) // extended to 128 via Bitboard128 scaffolding
        {
            var pieceLayout = Internal.Layout.PieceMapLayout.Build(game);
            var pieceSnapshot = Internal.Layout.PieceMapSnapshot.Build(pieceLayout, initialGameState, shape);
            var bbLayout = Internal.Layout.BitboardLayout.Build(game);
            var bbSnapshot = Internal.Layout.BitboardSnapshot.Build(bbLayout, initialGameState, shape);
            // Bitboard occupancy index now built directly from layout + snapshot without wrapper service.
            var occupancy = new Internal.Occupancy.BitboardOccupancyIndex(bbLayout, bbSnapshot, shape, game, initialGameState);
            // Ensure initial snapshot is bound so IsEmpty/IsOwnedBy reflect initial piece placement.
            (occupancy as Internal.Acceleration.IBitboardBackedOccupancy)?.BindSnapshot(bbSnapshot);
            var sliding = Internal.Attacks.SlidingAttackGenerator.Build(shape);
            var attackServices = sliding; // implements IAttackRays
            accelerationContext = new Internal.Acceleration.BitboardAccelerationContext(bbLayout, bbSnapshot, pieceLayout, pieceSnapshot, shape, topology, occupancy, attackServices);
        }
        else
        {
            var occupancy = new Internal.Occupancy.NaiveOccupancyIndex(game, initialGameState);
            var sliding = Internal.Attacks.SlidingAttackGenerator.Build(shape);
            accelerationContext = new Internal.Acceleration.NaiveAccelerationContext(occupancy, sliding);
        }

        // Sliding fast-path decorator layering if enabled (decorates chosen resolver)
        if (FeatureFlags.EnableSlidingFastPath && pathResolver is not null)
        {
            var sliding = Internal.Attacks.SlidingAttackGenerator.Build(shape);
            pathResolver = new Internal.Paths.SlidingFastPathResolver(shape, sliding, accelerationContext.Occupancy, pathResolver);
        }

    // Ensure non-null pathResolver (legacy visitor resolver fallback already applied earlier if compiled disabled)
    pathResolver ??= new Internal.Paths.SimplePatternPathResolver(game.Board);
    var capabilities = new EngineCapabilities(topology, pathResolver, accelerationContext);
        var engine = new GameEngine(game, gamePhaseRoot, decisionPlan, _observer, capabilities);

        // GameProgress no longer carries snapshots explicitly (acceleration context retains internal state)
    _initialGameProgress = new GameProgress(engine, initialGameState, Enumerable.Empty<IGameEvent>());

    return _initialGameProgress;
    }

    /// <summary>
    /// Registers an immutable extras state record captured in the initial <see cref="GameState"/> (e.g., castling rights, ko info).
    /// </summary>
    /// <typeparam name="T">Record / class type representing extras.</typeparam>
    /// <param name="extras">Instance (must be reference type).</param>
    protected void WithState<T>(T extras) where T : class
    {
        ArgumentNullException.ThrowIfNull(extras);

        // TODO: Revisit Extras state/artifact design (naming + potential consolidation). Consider exposing a more explicit
        // registration API to distinguish engine-level capabilities from per-game auxiliary state. (Tracked from user note)
        _extrasStates.Add(extras);
    }
}