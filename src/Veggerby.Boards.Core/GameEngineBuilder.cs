using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core
{
    public abstract class GameEngineBuilder
    {
        public abstract class DefinitionSettingsBase
        {
            protected GameEngineBuilder Builder { get; }

            public DefinitionSettingsBase(GameEngineBuilder builder)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }

                Builder = builder;
            }
        }

        #region Game Builder
        public class TileDefinitionSettings : DefinitionSettingsBase
        {
            public TileDefinitionSettings(GameEngineBuilder builder) : base(builder)
            {
            }

            public string TileId { get; private set; }

            public TileDefinitionSettings WithId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                TileId = id;
                return this;
            }

            public TileRelationDefinitionSettings WithRelationTo(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                var relation = new TileRelationDefinitionSettings(Builder, this)
                    .FromTile(this.TileId)
                    .ToTile(id);

                Builder._tileRelationDefinitions.Add(relation);
                return relation;
            }


            public TileRelationDefinitionSettings WithRelationFrom(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                var relation = new TileRelationDefinitionSettings(Builder, this)
                    .FromTile(id)
                    .ToTile(this.TileId);

                Builder._tileRelationDefinitions.Add(relation);
                return relation;
            }
        }

        public class PlayerDefinitionSettings : DefinitionSettingsBase
        {
            public PlayerDefinitionSettings(GameEngineBuilder builder) : base(builder)
            {
            }

            public string PlayerId { get; private set; }

            public PlayerDefinitionSettings WithId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                PlayerId = id;
                return this;
            }
        }

        public class DirectionDefinitionSettings : DefinitionSettingsBase
        {
            public DirectionDefinitionSettings(GameEngineBuilder builder) : base(builder)
            {
            }

            public string DirectionId { get; private set; }

            public DirectionDefinitionSettings WithId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                DirectionId = id;
                return this;
            }
        }

        public class DiceDefinitionSettings : DefinitionSettingsBase
        {
            public DiceDefinitionSettings(GameEngineBuilder builder) : base(builder)
            {
            }

            public string DiceId { get; private set; }

            public DiceDefinitionSettings WithId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                DiceId = id;
                return this;
            }

            public DiceDefinitionSettings HasNoValue()
            {
                Builder._diceState.Add(DiceId, null);
                return this;
            }

            public DiceDefinitionSettings HasValue(int value)
            {
                Builder._diceState.Add(DiceId, value);
                return this;
            }
        }

        public class TileRelationDefinitionSettings : DefinitionSettingsBase
        {
            private readonly TileDefinitionSettings _tileDefintion;

            public TileRelationDefinitionSettings(GameEngineBuilder builder, TileDefinitionSettings tileDefintion) : base(builder)
            {
                _tileDefintion = tileDefintion;
            }

            public string FromTileId { get; private set; }
            public string ToTileId { get; private set; }
            public string DirectionId { get; private set; }
            public int Distance { get; private set; }

            public TileRelationDefinitionSettings FromTile(string from)
            {
                if (string.IsNullOrEmpty(from))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(from));
                }

                FromTileId = from;
                return this;
            }

            public TileRelationDefinitionSettings ToTile(string to)
            {
                if (string.IsNullOrEmpty(to))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(to));
                }

                ToTileId = to;
                return this;
            }

            public TileRelationDefinitionSettings InDirection(string direction)
            {
                if (string.IsNullOrEmpty(direction))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(direction));
                }

                DirectionId = direction;
                return this;
            }

            public TileRelationDefinitionSettings WithDistance(int distance)
            {
                Distance = distance;
                return this;
            }

            public TileDefinitionSettings Done()
            {
                return _tileDefintion;
            }
        }

        public class PieceDefinitionSettings : DefinitionSettingsBase
        {
            public PieceDefinitionSettings(GameEngineBuilder builder) : base(builder)
            {
            }

            public string PieceId { get; private set; }
            public string PlayerId { get; private set; }

            public PieceDefinitionSettings WithId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                PieceId = id;
                return this;
            }

            public PieceDefinitionSettings WithOwner(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                PlayerId = id;
                return this;
            }

            public PieceDefinitionSettings OnTile(string tileId)
            {
                if (string.IsNullOrEmpty(tileId))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(tileId));
                }

                Builder._piecePositions.Add(PieceId, tileId);
                return this;
            }

            public PieceDirectionPatternDefinitionSettings HasDirection(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                var direction = new PieceDirectionPatternDefinitionSettings(Builder, this).WithDirection(id);
                Builder._pieceDirectionPatternDefinitions.Add(direction);
                return direction;
            }

            public PieceDefinitionSettings HasPattern(params string[] ids)
            {
                var direction = new PieceDirectionPatternDefinitionSettings(Builder, this).WithDirection(ids);
                Builder._pieceDirectionPatternDefinitions.Add(direction);
                return this;
            }
        }

        public class PieceDirectionPatternDefinitionSettings : DefinitionSettingsBase
        {
            private readonly PieceDefinitionSettings _pieceDefinition;

            public PieceDirectionPatternDefinitionSettings(GameEngineBuilder builder, PieceDefinitionSettings pieceDefinition) : base(builder)
            {
                _pieceDefinition = pieceDefinition;
                IsRepeatable = false;
            }

            public string PieceId => _pieceDefinition.PieceId;
            public bool IsRepeatable { get; private set; }
            public IEnumerable<string> DirectionIds { get; private set; }

            public PieceDefinitionSettings CanRepeat()
            {
                IsRepeatable = true;
                return _pieceDefinition;
            }

            public PieceDefinitionSettings DoesNotRepeat()
            {
                IsRepeatable = false;
                return _pieceDefinition;
            }

            public PieceDirectionPatternDefinitionSettings WithDirection(params string[] directions)
            {
                if (directions == null)
                {
                    throw new ArgumentNullException(nameof(directions));
                }

                if (!directions.Any())
                {
                    throw new ArgumentException("Must provide at least one direction", nameof(directions));
                }

                if (directions.Any(x => string.IsNullOrEmpty(x)))
                {
                    throw new ArgumentException("All directions must be non-null and non-empty", nameof(directions));
                }

                DirectionIds = (DirectionIds ?? Enumerable.Empty<string>()).Concat(directions).ToList();
                return this;
            }

            public PieceDefinitionSettings Done()
            {
                return _pieceDefinition;
            }
        }

        public class ArtifactDefinitionSettings : DefinitionSettingsBase
        {
            public ArtifactDefinitionSettings(GameEngineBuilder builder) : base(builder)
            {
            }

            public string ArtifactId { get; private set; }

            public Func<string, Artifact> Factory { get; private set; }

            public ArtifactDefinitionSettings WithId(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Value cannot be null or empty", nameof(id));
                }

                ArtifactId = id;
                return this;
            }

            public ArtifactDefinitionSettings OfType<T>() where T : Artifact, new()
            {
                Factory = id => new T();
                return this;
            }

            public ArtifactDefinitionSettings WithFactory<T>(Func<string, T> factory) where T : Artifact
            {
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
                return this;
            }
        }

        public class GamePhaseDefinitionSettings : DefinitionSettingsBase
        {
            public GamePhaseDefinitionSettings(GameEngineBuilder builder) : base(builder)
            {
                RuleCompositeMode = CompositeMode.All;
                ConditionFactory = game => null;
            }

            public int? Number { get; private set; }
            public Func<Game, IGameStateCondition> ConditionFactory { get; private set; }
            public IEnumerable<Func<Game, IGameEventRule>> RuleFactories { get; private set; }
            public CompositeMode RuleCompositeMode { get; internal set; }

            public GamePhaseDefinitionSettings WithNumber(int number)
            {
                Number = number;
                return this;
            }

            public GamePhaseDefinitionSettings WithCondition<T>() where T : IGameStateCondition, new()
            {
                ConditionFactory = game => new T();
                return this;
            }

            public GamePhaseDefinitionSettings WithCondition(Func<Game, IGameStateCondition> factory)
            {
                ConditionFactory = factory ?? throw new ArgumentNullException(nameof(factory));
                return this;
            }

            private void AddRuleFactory(params Func<Game, IGameEventRule>[] factories)
            {
                RuleFactories = (RuleFactories ?? Enumerable.Empty<Func<Game, IGameEventRule>>()).Concat(factories).ToList();
            }

            public GamePhaseDefinitionSettings WithRule<T>() where T : IGameEventRule, new()
            {
                AddRuleFactory(game => new T());
                return this;
            }

            public GamePhaseDefinitionSettings WithRule(Func<Game, IGameEventRule> factory)
            {
                if (factory == null)
                {
                    throw new ArgumentNullException(nameof(factory));
                }

                AddRuleFactory(factory);
                return this;
            }

            public GamePhaseDefinitionSettings WithRule<T>(Func<GameState, T, ConditionResponse> ruleCheck, IStateMutator<T> onAfterEvent) where T : IGameEvent
            {
                AddRuleFactory(game => SimpleGameEventRule<T>.New(ruleCheck, null, onAfterEvent));
                return this;
            }

            public GamePhaseDefinitionSettings WithRule<T>(IStateMutator<T> onAfterEvent) where T : IGameEvent
            {
                WithRule((state, @event) => ConditionResponse.Valid, onAfterEvent);
                return this;
            }

            public GamePhaseDefinitionSettings AnyRule()
            {
                RuleCompositeMode = CompositeMode.Any;
                return this;
            }

            public GamePhaseDefinitionSettings AllRules()
            {
                RuleCompositeMode = CompositeMode.All;
                return this;
            }
        }

        protected string BoardId { get; set; }
        private readonly IList<PlayerDefinitionSettings> _playerDefinitions = new List<PlayerDefinitionSettings>();
        private readonly IList<TileDefinitionSettings> _tileDefinitions = new List<TileDefinitionSettings>();
        private readonly IList<DirectionDefinitionSettings> _directionDefinitions = new List<DirectionDefinitionSettings>();
        private readonly IList<DiceDefinitionSettings> _diceDefinitions = new List<DiceDefinitionSettings>();
        private readonly IList<TileRelationDefinitionSettings> _tileRelationDefinitions = new List<TileRelationDefinitionSettings>();
        private readonly IList<PieceDefinitionSettings> _pieceDefinitions = new List<PieceDefinitionSettings>();
        private readonly IList<PieceDirectionPatternDefinitionSettings> _pieceDirectionPatternDefinitions = new List<PieceDirectionPatternDefinitionSettings>();
        private readonly IList<ArtifactDefinitionSettings> _artifactDefinitions = new List<ArtifactDefinitionSettings>();
        private readonly IList<GamePhaseDefinitionSettings> _gamePhaseDefinitions = new List<GamePhaseDefinitionSettings>();

        private Tile CreateTile(TileDefinitionSettings tileDefinitionSettings)
        {
            return new Tile(tileDefinitionSettings.TileId);
        }

        private Direction CreateTileRelationDirection(DirectionDefinitionSettings directionDefinitionSettings)
        {
            return new Direction(directionDefinitionSettings.DirectionId);
        }

        private TileRelation CreateTileRelaton(TileRelationDefinitionSettings tileRelationDefinitionSettings, IEnumerable<Tile> tiles, IEnumerable<Direction> directions)
        {
            var sourceTile = tiles.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.FromTileId));
            var destinationTile = tiles.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.ToTileId));
            var direction = directions.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.DirectionId));
            return new TileRelation(sourceTile, destinationTile, direction);
        }

        private Artifact CreateDice(DiceDefinitionSettings diceDefinitionSettings)
        {
            return new RegularDice(diceDefinitionSettings.DiceId);
        }

        private Artifact CreatePiece(PieceDefinitionSettings pieceDefinitionSettings, IEnumerable<PieceDirectionPatternDefinitionSettings> pieceDirectionPatternDefinitions, IEnumerable<Direction> directions, IEnumerable<Player> players)
        {
            var player = !string.IsNullOrEmpty(pieceDefinitionSettings.PlayerId)
                ? players.SingleOrDefault(x => string.Equals(x.Id, pieceDefinitionSettings.PlayerId))
                : null;

            var patterns = pieceDirectionPatternDefinitions.Where(x => x.PieceId == pieceDefinitionSettings.PieceId).ToList();

            return new Piece(pieceDefinitionSettings.PieceId, player, patterns.Select(x => CreatePattern(x, directions)));
        }

        private IPattern CreatePattern(PieceDirectionPatternDefinitionSettings pieceDefinitionSettings, IEnumerable<Direction> directions)
        {
            var patternDirections = pieceDefinitionSettings.DirectionIds.Select(directionId => directions.Single(x => string.Equals(x.Id, directionId))).ToList();

            if (!patternDirections.Any())
            {
                return new NullPattern();
            }

            if (patternDirections.Count() == 1)
            {
                return new DirectionPattern(patternDirections.Single(), pieceDefinitionSettings.IsRepeatable);
            }

            return pieceDefinitionSettings.IsRepeatable
                ? new MultiDirectionPattern(patternDirections, true)
                : (IPattern)new FixedPattern(patternDirections);
        }

        private Artifact CreateArtifact(ArtifactDefinitionSettings artifactDefinitionSettings)
        {
            return artifactDefinitionSettings.Factory(artifactDefinitionSettings.ArtifactId);
        }

        private GamePhase CreateGamePhase(int number, GamePhaseDefinitionSettings gamePhaseDefinitionSettings, CompositeGamePhase parent, Game game)
        {
            IGameEventRule rule = null;

            if (gamePhaseDefinitionSettings.RuleFactories == null || !gamePhaseDefinitionSettings.RuleFactories.Any())
            {
                throw new ArgumentException("There must be at least one rule defined", nameof(gamePhaseDefinitionSettings));
            }

            if (gamePhaseDefinitionSettings.RuleFactories.Count() == 1)
            {
                rule = gamePhaseDefinitionSettings.RuleFactories.Single()(game);
            }
            else
            {
                rule = CompositeGameEventRule.CreateCompositeRule(
                    gamePhaseDefinitionSettings.RuleCompositeMode,
                    gamePhaseDefinitionSettings.RuleFactories.Select(factory => factory(game)).ToArray());
            }

            return GamePhase.New(
                gamePhaseDefinitionSettings.Number ?? number,
                gamePhaseDefinitionSettings.ConditionFactory(game) ?? new NullGameStateCondition(),
                rule,
                parent);
        }

        protected PlayerDefinitionSettings AddPlayer(string playerId)
        {
            var player = new PlayerDefinitionSettings(this).WithId(playerId);
            _playerDefinitions.Add(player);
            return player;
        }

        protected PlayerDefinitionSettings WithPlayer(string playerId)
        {
            return _playerDefinitions.Single(x => string.Equals(x.PlayerId, playerId));
        }

        protected TileDefinitionSettings AddTile(string tileId)
        {
            var tile = new TileDefinitionSettings(this).WithId(tileId);
            _tileDefinitions.Add(tile);
            return tile;
        }

        protected TileDefinitionSettings WithTile(string tileId)
        {
            return _tileDefinitions.Single(x => string.Equals(x.TileId, tileId));
        }

        protected DirectionDefinitionSettings AddDirection(string directionId)
        {
            var direction = new DirectionDefinitionSettings(this).WithId(directionId);
            _directionDefinitions.Add(direction);
            return direction;
        }

        protected DiceDefinitionSettings AddDice(string diceId)
        {
            var dice = new DiceDefinitionSettings(this).WithId(diceId);
            _diceDefinitions.Add(dice);
            return dice;
        }

        protected DiceDefinitionSettings WithDice(string diceId)
        {
            return _diceDefinitions.Single(x => string.Equals(x.DiceId, diceId));
        }

        protected PieceDefinitionSettings AddPiece(string pieceId)
        {
            var piece = new PieceDefinitionSettings(this).WithId(pieceId);
            _pieceDefinitions.Add(piece);
            return piece;
        }

        protected PieceDefinitionSettings WithPiece(string pieceId)
        {
            return _pieceDefinitions.Single(x => string.Equals(x.PieceId, pieceId));
        }

        protected ArtifactDefinitionSettings AddArtifact(string artifactId)
        {
            var artifact = new ArtifactDefinitionSettings(this).WithId(artifactId);
            _artifactDefinitions.Add(artifact);
            return artifact;
        }

        protected ArtifactDefinitionSettings WithArtifact(string artifactId)
        {
            return _artifactDefinitions.Single(x => string.Equals(x.ArtifactId, artifactId));
        }

        #endregion

        #region Initial GameState builder

        private IDictionary<string, int?> _diceState = new Dictionary<string, int?>();

        private IDictionary<string, string> _piecePositions = new Dictionary<string, string>();

        #endregion

        #region GamePhase Root builder

        protected GamePhaseDefinitionSettings AddGamePhase(string label) // label not used for anything, just to document in builder
        {
            var gamePhase = new GamePhaseDefinitionSettings(this);
            _gamePhaseDefinitions.Add(gamePhase);
            return gamePhase;
        }

        #endregion

        protected abstract void Build();

        private GameEngine _gameEngine;

        public GameEngine Compile()
        {
            if (_gameEngine != null)
            {
                return _gameEngine;
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
                .Select(x => x.Value == null
                    ? (IArtifactState)new NullDiceState<int>(game.GetArtifact<RegularDice>(x.Key))
                    : (IArtifactState)new DiceState<int>(game.GetArtifact<RegularDice>(x.Key), x.Value.Value))
                .ToList();

            var initialGameState = GameState.New(game, pieceStates.Concat(diceStates).ToList());

            // compile GamePhase root

            GamePhase gamePhaseRoot = null;

            if (_gamePhaseDefinitions.Any())
            {
                var parent = GamePhase.NewParent(1, null, null);

                var number = 1;
                foreach (var gamePhaseDefinition in _gamePhaseDefinitions)
                {
                    var phase = CreateGamePhase(number, gamePhaseDefinition, parent, game);
                    number = Math.Max(number, gamePhaseDefinition.Number ?? number) + 1;
                }

                gamePhaseRoot = parent;
            }
            else
            {
                 gamePhaseRoot = GamePhase.New(1, new States.Conditions.NullGameStateCondition(), SimpleGameEventRule<IGameEvent>.New((state, @event) => ConditionResponse.NotApplicable));
            }

            // combine
            _gameEngine = GameEngine.New(initialGameState, gamePhaseRoot);

            return _gameEngine;
        }
    }
}
