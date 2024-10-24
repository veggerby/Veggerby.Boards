﻿using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Builder.Artifacts;
using Veggerby.Boards.Core.Builder.Phases;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core;

public abstract class GameBuilder
{
    protected string BoardId { get; set; }
    private readonly IList<PlayerDefinition> _playerDefinitions = new List<PlayerDefinition>();
    private readonly IList<TileDefinition> _tileDefinitions = new List<TileDefinition>();
    private readonly IList<DirectionDefinition> _directionDefinitions = new List<DirectionDefinition>();
    private readonly IList<DiceDefinition> _diceDefinitions = new List<DiceDefinition>();
    private readonly IList<TileRelationDefinition> _tileRelationDefinitions = new List<TileRelationDefinition>();
    private readonly IList<PieceDefinition> _pieceDefinitions = new List<PieceDefinition>();
    private readonly IList<PieceDirectionPatternDefinition> _pieceDirectionPatternDefinitions = new List<PieceDirectionPatternDefinition>();
    private readonly IList<ArtifactDefinition> _artifactDefinitions = new List<ArtifactDefinition>();
    private readonly IList<GamePhaseDefinition> _gamePhaseDefinitions = new List<GamePhaseDefinition>();

    private Tile CreateTile(TileDefinition tile)
    {
        return new Tile(tile.TileId);
    }

    private Direction CreateTileRelationDirection(DirectionDefinition direction)
    {
        return new Direction(direction.DirectionId);
    }

    private TileRelation CreateTileRelaton(TileRelationDefinition relation, IEnumerable<Tile> tiles, IEnumerable<Direction> directions)
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

    private IPattern CreatePattern(PieceDirectionPatternDefinition piece, IEnumerable<Direction> directions)
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

    private Artifact CreateArtifact(ArtifactDefinition artifact)
    {
        return artifact.Factory(artifact.ArtifactId);
    }

    protected PlayerDefinition AddPlayer(string playerId)
    {
        var player = new PlayerDefinition(this).WithId(playerId);
        _playerDefinitions.Add(player);
        return player;
    }

    protected PlayerDefinition WithPlayer(string playerId)
    {
        return _playerDefinitions.Single(x => string.Equals(x.PlayerId, playerId));
    }

    protected TileDefinition AddTile(string tileId)
    {
        var tile = new TileDefinition(this).WithId(tileId);
        _tileDefinitions.Add(tile);
        return tile;
    }

    protected TileDefinition WithTile(string tileId)
    {
        return _tileDefinitions.Single(x => string.Equals(x.TileId, tileId));
    }

    protected DirectionDefinition AddDirection(string directionId)
    {
        var direction = new DirectionDefinition(this).WithId(directionId);
        _directionDefinitions.Add(direction);
        return direction;
    }

    protected DiceDefinition AddDice(string diceId)
    {
        var dice = new DiceDefinition(this).WithId(diceId);
        _diceDefinitions.Add(dice);
        return dice;
    }

    protected DiceDefinition WithDice(string diceId)
    {
        return _diceDefinitions.Single(x => string.Equals(x.DiceId, diceId));
    }

    protected PieceDefinition AddPiece(string pieceId)
    {
        var piece = new PieceDefinition(this).WithId(pieceId);
        _pieceDefinitions.Add(piece);
        return piece;
    }

    protected PieceDefinition WithPiece(string pieceId)
    {
        return _pieceDefinitions.Single(x => string.Equals(x.PieceId, pieceId));
    }

    protected ArtifactDefinition AddArtifact(string artifactId)
    {
        var artifact = new ArtifactDefinition(this).WithId(artifactId);
        _artifactDefinitions.Add(artifact);
        return artifact;
    }

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

    protected IGamePhaseDefinition AddGamePhase(string label) // label not used for anything, just to document in builder
    {
        var gamePhase = new GamePhaseDefinition(this, label);
        _gamePhaseDefinitions.Add(gamePhase);
        return gamePhase;
    }

    protected abstract void Build();

    private GameProgress _initialGameProgress;

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
        var relations = _tileRelationDefinitions.Select(x => CreateTileRelaton(x, tiles, directions)).ToArray();
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

        var initialGameState = GameState.New(pieceStates.Concat(diceStates).ToList());

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