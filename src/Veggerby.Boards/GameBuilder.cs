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
    protected string? BoardId
    {
        get; set;
    }

    private readonly IList<PlayerDefinition> _playerDefinitions = [];
    private readonly Dictionary<string, PlayerDefinition> _playerDefinitionsById = new(StringComparer.Ordinal);
    private readonly IList<TileDefinition> _tileDefinitions = [];
    private readonly Dictionary<string, TileDefinition> _tileDefinitionsById = new(StringComparer.Ordinal);
    private readonly IList<DirectionDefinition> _directionDefinitions = [];
    private readonly Dictionary<string, DirectionDefinition> _directionDefinitionsById = new(StringComparer.Ordinal);
    private readonly IList<DiceDefinition> _diceDefinitions = [];
    private readonly Dictionary<string, DiceDefinition> _diceDefinitionsById = new(StringComparer.Ordinal);
    private readonly IList<TileRelationDefinition> _tileRelationDefinitions = [];
    private readonly IList<PieceDefinition> _pieceDefinitions = [];
    private readonly Dictionary<string, PieceDefinition> _pieceDefinitionsById = new(StringComparer.Ordinal);
    private readonly IList<PieceDirectionPatternDefinition> _pieceDirectionPatternDefinitions = [];
    private readonly IList<ArtifactDefinition> _artifactDefinitions = [];
    private readonly Dictionary<string, ArtifactDefinition> _artifactDefinitionsById = new(StringComparer.Ordinal);
    private readonly IList<GamePhaseDefinition> _gamePhaseDefinitions = [];
    // Extras states now keyed by concrete type to enforce single instance per type.
    private readonly Dictionary<Type, object> _extrasStates = new();
    private readonly IList<(string PlayerId, bool IsActive)> _activePlayerAssignments = new List<(string, bool)>();
    private readonly IList<(string ClockId, Artifacts.TimeControl Control)> _clockDefinitions = [];
    private readonly Dictionary<string, (string ClockId, Artifacts.TimeControl Control)> _clockDefinitionsById = new(StringComparer.Ordinal);

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
        // Hot path compile: explicit loops avoid multiple enumerations + LINQ allocation.
        Tile? sourceTile = null;
        Tile? destinationTile = null;
        foreach (var tile in tiles)
        {
            if (sourceTile is null && string.Equals(tile.Id, relation.FromTileId))
            {
                sourceTile = tile;
            }

            if (destinationTile is null && string.Equals(tile.Id, relation.ToTileId))
            {
                destinationTile = tile;
            }

            if (sourceTile is not null && destinationTile is not null)
            {
                break;
            }
        }

        if (sourceTile is null)
        {
            throw new InvalidOperationException($"Source tile '{relation.FromTileId}' not found for relation.");
        }

        if (destinationTile is null)
        {
            throw new InvalidOperationException($"Destination tile '{relation.ToTileId}' not found for relation.");
        }

        Direction? direction = null;
        foreach (var dir in directions)
        {
            if (string.Equals(dir.Id, relation.DirectionId))
            {
                direction = dir;
                break;
            }
        }

        if (direction is null)
        {
            throw new InvalidOperationException($"Direction '{relation.DirectionId}' not found for relation.");
        }

        return new TileRelation(sourceTile, destinationTile, direction);
    }

    private Artifact CreateDice(DiceDefinition dice)
    {
        return new Dice(dice.DiceId);
    }

    private static Artifact CreatePiece(PieceDefinition piece, IEnumerable<PieceDirectionPatternDefinition> pattern, IEnumerable<Direction> directions, IEnumerable<Player> players)
    {
        // Hot path compile: eliminate LINQ for deterministic low-allocation assembly.
        Player? player = null;
        if (!string.IsNullOrEmpty(piece.PlayerId))
        {
            foreach (var p in players)
            {
                if (string.Equals(p.Id, piece.PlayerId))
                {
                    player = p;
                    break;
                }
            }
        }

        if (player is null)
        {
            throw new InvalidOperationException($"Piece definition '{piece.PieceId}' references unknown player '{piece.PlayerId}'.");
        }

        // Collect pattern definitions for this piece.
        var patternDefs = new List<PieceDirectionPatternDefinition>();
        foreach (var pd in pattern)
        {
            if (pd.PieceId == piece.PieceId)
            {
                patternDefs.Add(pd);
            }
        }

        // Transform to concrete IPattern list.
        var compiled = new List<IPattern>(patternDefs.Count);
        foreach (var pd in patternDefs)
        {
            compiled.Add(CreatePattern(pd, directions));
        }

        return new Piece(piece.PieceId, player, compiled, piece.Metadata);
    }

    private static IPattern CreatePattern(PieceDirectionPatternDefinition piece, IEnumerable<Direction> directions)
    {
        // Hot path compile: explicit direction resolution without LINQ.
        var patternDirections = new List<Direction>();
        foreach (var directionId in piece.DirectionIds)
        {
            Direction? found = null;
            foreach (var dir in directions)
            {
                if (string.Equals(dir.Id, directionId))
                {
                    found = dir;
                    break;
                }
            }

            if (found is null)
            {
                throw new InvalidOperationException($"Pattern references unknown direction '{directionId}'.");
            }

            patternDirections.Add(found);
        }

        if (patternDirections.Count == 0)
        {
            return new NullPattern();
        }

        if (patternDirections.Count == 1)
        {
            return new DirectionPattern(patternDirections[0], piece.IsRepeatable);
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
        ArgumentNullException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));

        var player = new PlayerDefinition(this).WithId(playerId);
        _playerDefinitions.Add(player);
        _playerDefinitionsById[player.PlayerId] = player;

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
        ArgumentNullException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));

        // Defer validation (existence, uniqueness) until compile to allow out-of-order declarations.
        _activePlayerAssignments.Add((playerId, isActive));
    }

    /// <summary>
    /// Adds a game clock with time control configuration.
    /// </summary>
    /// <param name="clockId">Unique identifier for the clock.</param>
    /// <param name="control">Time control configuration (initial time, increment, delay, bonus).</param>
    /// <remarks>
    /// Creates a <see cref="Artifacts.GameClock"/> artifact and initializes a <see cref="States.ClockState"/>
    /// with the configured initial time for all players. Clock start/stop/flag events must be sent explicitly
    /// by the application or through game rules.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when a clock with the same ID has already been added.</exception>
    protected void WithClock(string clockId, Artifacts.TimeControl control)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(clockId, nameof(clockId));
        ArgumentNullException.ThrowIfNull(control, nameof(control));

        if (_clockDefinitionsById.ContainsKey(clockId))
        {
            throw new InvalidOperationException($"Duplicate clock definition '{clockId}'.");
        }

        var definition = (clockId, control);
        _clockDefinitions.Add(definition);
        _clockDefinitionsById[clockId] = definition;
    }

    /// <summary>
    /// Gets an existing player definition for further configuration.
    /// </summary>
    /// <param name="playerId">Identifier of the previously added player.</param>
    /// <returns>Player definition.</returns>
    protected PlayerDefinition WithPlayer(string playerId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));

        if (!_playerDefinitionsById.TryGetValue(playerId, out var def))
        {
            throw new InvalidOperationException($"Unknown player definition '{playerId}'.");
        }

        return def;
    }

    /// <summary>
    /// Adds a new tile definition.
    /// </summary>
    /// <param name="tileId">Tile identifier.</param>
    /// <returns>Tile definition.</returns>
    protected TileDefinition AddTile(string tileId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tileId, nameof(tileId));

        var tile = new TileDefinition(this).WithId(tileId);
        _tileDefinitions.Add(tile);
        _tileDefinitionsById[tile.TileId] = tile;

        return tile;
    }

    /// <summary>
    /// Gets an existing tile definition for additional configuration.
    /// </summary>
    /// <param name="tileId">Tile identifier.</param>
    /// <returns>Tile definition.</returns>
    protected TileDefinition WithTile(string tileId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tileId, nameof(tileId));

        if (!_tileDefinitionsById.TryGetValue(tileId, out var def))
        {
            throw new InvalidOperationException($"Unknown tile definition '{tileId}'.");
        }

        return def;
    }

    /// <summary>
    /// Adds a direction definition.
    /// </summary>
    /// <param name="directionId">Direction identifier.</param>
    /// <returns>Direction definition.</returns>
    protected DirectionDefinition AddDirection(string directionId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(directionId, nameof(directionId));

        var direction = new DirectionDefinition(this).WithId(directionId);
        _directionDefinitions.Add(direction);
        _directionDefinitionsById[direction.DirectionId] = direction;

        return direction;
    }

    /// <summary>
    /// Adds a dice definition.
    /// </summary>
    /// <param name="diceId">Dice identifier.</param>
    /// <returns>Dice definition.</returns>
    protected DiceDefinition AddDice(string diceId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(diceId, nameof(diceId));

        var dice = new DiceDefinition(this).WithId(diceId);
        _diceDefinitions.Add(dice);
        _diceDefinitionsById[dice.DiceId] = dice;

        return dice;
    }

    /// <summary>
    /// Gets an existing dice definition.
    /// </summary>
    /// <param name="diceId">Dice identifier.</param>
    /// <returns>Dice definition.</returns>
    protected DiceDefinition WithDice(string diceId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(diceId, nameof(diceId));

        if (!_diceDefinitionsById.TryGetValue(diceId, out var def))
        {
            throw new InvalidOperationException($"Unknown dice definition '{diceId}'.");
        }

        return def;
    }

    /// <summary>
    /// Adds a piece definition.
    /// </summary>
    /// <param name="pieceId">Piece identifier.</param>
    /// <returns>Piece definition.</returns>
    protected PieceDefinition AddPiece(string pieceId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(pieceId, nameof(pieceId));

        var piece = new PieceDefinition(this).WithId(pieceId);
        _pieceDefinitions.Add(piece);
        _pieceDefinitionsById[piece.PieceId] = piece;

        return piece;
    }

    /// <summary>
    /// Gets an existing piece definition.
    /// </summary>
    /// <param name="pieceId">Piece identifier.</param>
    /// <returns>Piece definition.</returns>
    protected PieceDefinition WithPiece(string pieceId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(pieceId, nameof(pieceId));

        if (!_pieceDefinitionsById.TryGetValue(pieceId, out var def))
        {
            throw new InvalidOperationException($"Unknown piece definition '{pieceId}'.");
        }

        return def;
    }

    /// <summary>
    /// Adds a generic artifact definition (custom runtime component backed by an <see cref="Artifact"/> subtype).
    /// </summary>
    /// <param name="artifactId">Artifact identifier.</param>
    /// <returns>Artifact definition.</returns>
    protected ArtifactDefinition AddArtifact(string artifactId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(artifactId, nameof(artifactId));

        var artifact = new ArtifactDefinition(this).WithId(artifactId);
        _artifactDefinitions.Add(artifact);
        _artifactDefinitionsById[artifact.ArtifactId] = artifact;

        return artifact;
    }

    /// <summary>
    /// Gets an existing artifact definition.
    /// </summary>
    /// <param name="artifactId">Artifact identifier.</param>
    /// <returns>Artifact definition.</returns>
    protected ArtifactDefinition WithArtifact(string artifactId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(artifactId, nameof(artifactId));

        if (!_artifactDefinitionsById.TryGetValue(artifactId, out var def))
        {
            throw new InvalidOperationException($"Unknown artifact definition '{artifactId}'.");
        }

        return def;
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
        ArgumentNullException.ThrowIfNullOrWhiteSpace(diceId, nameof(diceId));

        _diceState.Add(diceId, value);
    }

    internal void AddPieceOnTile(string pieceId, string tileId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(pieceId, nameof(pieceId));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tileId, nameof(tileId));

        _piecePositions.Add(pieceId, tileId);
    }
    /// <summary>
    /// Adds a game phase definition which can attach rules, conditions and event handlers.
    /// </summary>
    /// <param name="label">Human readable label (not used functionally).</param>
    /// <returns>Phase definition fluent object.</returns>
    protected IGamePhaseDefinition AddGamePhase(string label) // label not used for anything, just to document in builder
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(label, nameof(label));

        var gamePhase = new GamePhaseDefinition(this, label);
        _gamePhaseDefinitions.Add(gamePhase);

        return gamePhase;
    }

    /// <summary>
    /// Adds multiple tiles in a grid pattern with automatic ID generation.
    /// </summary>
    /// <param name="width">Width of the grid (columns).</param>
    /// <param name="height">Height of the grid (rows).</param>
    /// <param name="tileIdFactory">Function to generate tile IDs from (x, y) coordinates.</param>
    /// <param name="configureTile">Optional action to configure each tile with its coordinates.</param>
    /// <returns>This builder for fluent chaining.</returns>
    protected GameBuilder AddGridTiles(
        int width,
        int height,
        Func<int, int, string> tileIdFactory,
        Action<TileDefinition, int, int>? configureTile = null)
    {
        ArgumentNullException.ThrowIfNull(tileIdFactory, nameof(tileIdFactory));

        if (width <= 0)
        {
            throw new ArgumentException("Width must be positive", nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentException("Height must be positive", nameof(height));
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tileId = tileIdFactory(x, y);
                var tile = AddTile(tileId);
                configureTile?.Invoke(tile, x, y);
            }
        }

        return this;
    }

    /// <summary>
    /// Adds multiple tiles in a ring pattern with automatic ID generation.
    /// </summary>
    /// <param name="count">Number of tiles in the ring.</param>
    /// <param name="tileIdFactory">Function to generate tile IDs from position (0 to count-1).</param>
    /// <param name="configureTile">Optional action to configure each tile with its position.</param>
    /// <returns>This builder for fluent chaining.</returns>
    protected GameBuilder AddRingTiles(
        int count,
        Func<int, string> tileIdFactory,
        Action<TileDefinition, int>? configureTile = null)
    {
        ArgumentNullException.ThrowIfNull(tileIdFactory, nameof(tileIdFactory));

        if (count <= 0)
        {
            throw new ArgumentException("Count must be positive", nameof(count));
        }

        for (int i = 0; i < count; i++)
        {
            var tileId = tileIdFactory(i);
            var tile = AddTile(tileId);
            configureTile?.Invoke(tile, i);
        }

        return this;
    }

    /// <summary>
    /// Adds multiple pieces with automatic ID generation.
    /// </summary>
    /// <param name="count">Number of pieces to create.</param>
    /// <param name="pieceIdFactory">Function to generate piece IDs from index (0 to count-1).</param>
    /// <param name="configurePiece">Action to configure each piece with its index.</param>
    /// <returns>This builder for fluent chaining.</returns>
    protected GameBuilder AddMultiplePieces(
        int count,
        Func<int, string> pieceIdFactory,
        Action<PieceDefinition, int> configurePiece)
    {
        ArgumentNullException.ThrowIfNull(pieceIdFactory, nameof(pieceIdFactory));
        ArgumentNullException.ThrowIfNull(configurePiece, nameof(configurePiece));

        if (count <= 0)
        {
            throw new ArgumentException("Count must be positive", nameof(count));
        }

        for (int i = 0; i < count; i++)
        {
            var pieceId = pieceIdFactory(i);
            var piece = AddPiece(pieceId);
            configurePiece(piece, i);
        }

        return this;
    }

    /// <summary>
    /// Calculates the next position in a ring (wrapping around).
    /// </summary>
    /// <param name="current">Current position.</param>
    /// <param name="ringSize">Total size of the ring.</param>
    /// <returns>Next position (wraps to 0 if at end).</returns>
    protected static int NextInRing(int current, int ringSize)
    {
        if (ringSize <= 0)
        {
            throw new ArgumentException("Ring size must be positive", nameof(ringSize));
        }

        return (current + 1) % ringSize;
    }

    /// <summary>
    /// Calculates the previous position in a ring (wrapping around).
    /// </summary>
    /// <param name="current">Current position.</param>
    /// <param name="ringSize">Total size of the ring.</param>
    /// <returns>Previous position (wraps to ringSize-1 if at start).</returns>
    protected static int PreviousInRing(int current, int ringSize)
    {
        if (ringSize <= 0)
        {
            throw new ArgumentException("Ring size must be positive", nameof(ringSize));
        }

        return (current - 1 + ringSize) % ringSize;
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

        // Add clock artifacts
        var clocks = new List<Artifacts.GameClock>();
        foreach (var (clockId, control) in _clockDefinitions)
        {
            var clock = new Artifacts.GameClock(clockId, control);
            clocks.Add(clock);
            artifacts.Add(clock);
        }

        // Add synthetic artifacts for extras states (one per extras type) so they participate in hashing & diffs deterministically.
        var extrasArtifacts = new Dictionary<Type, Artifact>();
        foreach (var kv in _extrasStates)
        {
            var t = kv.Key;
            // stable id includes full type name for uniqueness across modules
            var id = $"extras-{t.FullName}";
            var art = new ExtrasArtifact(id);
            extrasArtifacts[t] = art;
            artifacts.Add(art);
        }

        // Turn timeline artifact (single instance) - always enabled (graduated feature)
        var turnArtifact = new TurnArtifact("turn-timeline");
        artifacts.Add(turnArtifact);

        if (BoardId is null)
        {
            throw new InvalidOperationException("BoardId must be configured before building the game.");
        }

        var board = new Board(BoardId, relations);
        // Assemble artifact sequence in deterministic order: pieces -> dice -> custom artifacts (including extras, turn)
        var allArtifacts = new Artifact[pieces.Length + dice.Length + artifacts.Count];
        var index = 0;

        for (var i = 0; i < pieces.Length; i++)
        {
            allArtifacts[index++] = pieces[i];
        }

        for (var i = 0; i < dice.Length; i++)
        {
            allArtifacts[index++] = dice[i];
        }

        for (var i = 0; i < artifacts.Count; i++)
        {
            allArtifacts[index++] = artifacts[i];
        }

        var game = new Game(board, players, allArtifacts);

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
        if (_activePlayerAssignments.Count > 0)
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
            var activeCount = 0;
            foreach (var assignment in _activePlayerAssignments)
            {
                if (assignment.IsActive)
                {
                    activeCount++;
                }
            }

            if (activeCount != 1)
            {
                throw new InvalidOperationException($"Exactly one active player must be declared; found {activeCount}.");
            }
        }

        // Materialize extras states (non-generic wrapper avoids reflection during initial compile)
        foreach (var kv in _extrasStates)
        {
            var t = kv.Key;
            var extras = kv.Value;
            if (extrasArtifacts.TryGetValue(t, out var art))
            {
                baseStates.Add(new ExtrasState(art, extras, t));
            }
        }

        // Materialize clock states with initial time for all players
        foreach (var clock in clocks)
        {
            var remainingTime = new Dictionary<Player, TimeSpan>();
            foreach (var player in game.Players)
            {
                remainingTime[player] = clock.Control.InitialTime;
            }

            baseStates.Add(new States.ClockState(clock, remainingTime));
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

        if (_gamePhaseDefinitions.Count > 0)
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

        // Always use compiled patterns (graduated feature)
        var table = Flows.Patterns.PatternCompiler.Compile(game);
        // BoardShape (graduated) has superseded the adjacency cache
        var resolver = new Flows.Patterns.CompiledPatternResolver(table, game.Board, null, shape);
        var adapter = new Internal.Paths.CompiledPathResolverAdapter(resolver);
        Internal.Paths.IPathResolver pathResolver = adapter;

        // Acceleration context selection (bitboards always enabled for boards ≤128 tiles - graduated feature)
        Internal.Acceleration.IAccelerationContext accelerationContext;
        var tileCount = game.Board.Tiles.Count();
        if (tileCount <= 128)
        {
            var pieceLayout = Internal.Layout.PieceMapLayout.Build(game);
            var pieceSnapshot = Internal.Layout.PieceMapSnapshot.Build(pieceLayout, initialGameState, shape);
            var bbLayout = Internal.Layout.BitboardLayout.Build(game);
            var bbSnapshot = Internal.Layout.BitboardSnapshot.Build(bbLayout, initialGameState, shape);
            // Bitboard occupancy index now built directly from layout + snapshot without wrapper service.
            var occupancy = new Internal.Occupancy.BitboardOccupancyIndex(bbLayout, bbSnapshot, shape, game, initialGameState);
            // Ensure initial snapshot is bound so IsEmpty/IsOwnedBy reflect initial piece placement.
            (occupancy as Internal.Acceleration.IBitboardBackedOccupancy)?.BindSnapshot(bbSnapshot);
            var slidingGen = Internal.Attacks.SlidingAttackGenerator.Build(shape);
            var attackServices = slidingGen; // implements IAttackRays
            accelerationContext = new Internal.Acceleration.BitboardAccelerationContext(bbLayout, bbSnapshot, pieceLayout, pieceSnapshot, shape, topology, occupancy, attackServices);
        }
        else
        {
            var occupancy = new Internal.Occupancy.NaiveOccupancyIndex(game, initialGameState);
            var slidingGen = Internal.Attacks.SlidingAttackGenerator.Build(shape);
            accelerationContext = new Internal.Acceleration.NaiveAccelerationContext(occupancy, slidingGen);
        }

        // Sliding fast-path decorator layering (always enabled - graduated feature)
        var slidingFastPath = Internal.Attacks.SlidingAttackGenerator.Build(shape);
        pathResolver = new Internal.Paths.SlidingFastPathResolver(shape, slidingFastPath, accelerationContext.Occupancy, pathResolver);

        var capabilities = new EngineCapabilities(topology, pathResolver, accelerationContext);
        var engine = new GameEngine(game, gamePhaseRoot, decisionPlan, _observer, capabilities);

        // GameProgress no longer carries snapshots explicitly (acceleration context retains internal state)
        _initialGameProgress = new GameProgress(engine, initialGameState, Flows.Events.EventChain.Empty);

        return _initialGameProgress;
    }

    /// <summary>
    /// Registers an immutable extras state record captured in the initial <see cref="GameState"/> (e.g., castling rights, ko info).
    /// </summary>
    /// <typeparam name="T">Record / class type representing extras.</typeparam>
    /// <param name="extras">Instance (must be reference type).</param>
    protected void WithState<T>(T extras) where T : class
    {
        ArgumentNullException.ThrowIfNull(extras, nameof(extras));

        // TODO: Revisit Extras state/artifact design (naming + potential consolidation). Consider exposing a more explicit
        // registration API to distinguish engine-level capabilities from per-game auxiliary state. (Tracked from user note)
        var t = typeof(T);
        if (_extrasStates.ContainsKey(t))
        {
            throw new InvalidOperationException($"Extras state of type '{t.FullName}' already registered.");
        }

        _extrasStates[t] = extras;
    }
}