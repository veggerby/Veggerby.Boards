﻿using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts
{
    public abstract class GameBuilder
    {
        private class TileDefinitionSettings
        {
            public string TileId { get; set; }
        }

        private class PlayerDefinitionSettings
        {
            public string PlayerId { get; set; }
        }

        private class DirectionDefinitionSettings
        {
            public string DirectionId { get; set; }
        }

        private class TileRelationDefinitionSettings
        {
            public string SourceTileId { get; set; }
            public string DestinationTileId { get; set; }
            public string DirectionId { get; set; }
        }

        private class PieceDefinitionSettings
        {
            public string PieceId { get; set; }
            public string PlayerId { get; set; }
        }

        private class PieceDirectionPatternDefinitionSettings
        {
            public string PieceId { get; set; }
            public bool IsRepeatable { get; set; }
            public string[] DirectionIds { get; set; }
        }

        protected string BoardId { get; set; }
        private readonly IList<PlayerDefinitionSettings> _playerDefinitions = new List<PlayerDefinitionSettings>();
        private readonly IList<TileDefinitionSettings> _tileDefinitions = new List<TileDefinitionSettings>();
        private readonly IList<DirectionDefinitionSettings> _directionDefinitions = new List<DirectionDefinitionSettings>();
        private readonly IList<TileRelationDefinitionSettings> _tileRelationDefinitions = new List<TileRelationDefinitionSettings>();
        private readonly IList<PieceDefinitionSettings> _pieceDefinitions = new List<PieceDefinitionSettings>();
        private readonly IList<PieceDirectionPatternDefinitionSettings> _pieceDirectionPatternDefinitions = new List<PieceDirectionPatternDefinitionSettings>();

        public abstract void Build();

        private Game _game;

        public Game Compile()
        {
            if (_game != null)
            {
                return _game;
            }

            Build();
            var players = _playerDefinitions.Select(x => new Player(x.PlayerId));
            var tiles = _tileDefinitions.Select(CreateTile).ToArray();
            var directions = _directionDefinitions.Select(CreateTileRelationDirection).ToArray();
            var relations = _tileRelationDefinitions.Select(x => CreateTileRelaton(x, tiles, directions)).ToArray();
            var pieces = _pieceDefinitions.Select(x => CreatePiece(x, _pieceDirectionPatternDefinitions, directions, players)).ToArray();

            var board = new Board(BoardId, relations);
            _game = new Game(board, players, pieces);

            return _game;
        }

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
            var sourceTile = tiles.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.SourceTileId));
            var destinationTile = tiles.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.DestinationTileId));
            var direction = directions.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.DirectionId));
            return new TileRelation(sourceTile, destinationTile, direction);
        }

        private Piece CreatePiece(PieceDefinitionSettings pieceDefinitionSettings, IEnumerable<PieceDirectionPatternDefinitionSettings> pieceDirectionPatternDefinitions, IEnumerable<Direction> directions, IEnumerable<Player> players)
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

        protected void AddPlayerDefinition(string playerId)
        {
            _playerDefinitions.Add(new PlayerDefinitionSettings { PlayerId = playerId });
        }

        protected void AddTileDefinition(string tileId)
        {
            _tileDefinitions.Add(new TileDefinitionSettings { TileId = tileId });
        }
        protected void AddDirectionDefinition(string directionId)
        {
            _directionDefinitions.Add(new DirectionDefinitionSettings { DirectionId = directionId });
        }

        protected void AddTileRelationDefinition(string sourceTileId, string destinationTileId, string directionId)
        {
            _tileRelationDefinitions.Add(new TileRelationDefinitionSettings { SourceTileId = sourceTileId, DestinationTileId = destinationTileId, DirectionId = directionId });
        }

        protected void AddPieceDirectionPatternDefinition(string pieceId, bool isRepeatable, params string[] directions)
        {
            _pieceDirectionPatternDefinitions.Add(new PieceDirectionPatternDefinitionSettings { PieceId = pieceId, IsRepeatable = isRepeatable, DirectionIds = directions });
        }

        protected void AddPieceDefinition(string pieceId, string playerId = null)
        {
            _pieceDefinitions.Add(new PieceDefinitionSettings { PieceId = pieceId, PlayerId = playerId });
        }
    }
}