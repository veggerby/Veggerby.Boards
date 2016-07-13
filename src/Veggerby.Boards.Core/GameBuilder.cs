using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core
{
    public abstract class GameBuilder
    {
        private class TileDefinitionSettings
        {
            public string TileId { get; set; }
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
        }

        private class PieceDirectionPatternDefinitionSettings
        {
            public string PieceId { get; set; }
            public bool IsRepeatable { get; set; }
            public string[] DirectionIds { get; set; }
        }

        protected string BoardId { get; set; }
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
            var tiles = _tileDefinitions.Select(CreateTile).ToArray();
            var directions = _directionDefinitions.Select(CreateTileRelationDirection).ToArray();
            var relations = _tileRelationDefinitions.Select(x => CreateTileRelaton(x, tiles, directions)).ToArray();
            var pieces = _pieceDefinitions.Select(x => CreatePiece(x)).ToArray();

            var board = new Board(BoardId, tiles, relations);
            _game = new Game(BoardId, board, pieces);

            return _game;
        }

        private Tile CreateTile(TileDefinitionSettings tileDefinitionSettings)
        {
            return new Tile(tileDefinitionSettings.TileId);
        }

        private TileRelationDirection CreateTileRelationDirection(DirectionDefinitionSettings directionDefinitionSettings)
        {
            return new TileRelationDirection(directionDefinitionSettings.DirectionId);
        }

        private TileRelation CreateTileRelaton(TileRelationDefinitionSettings tileRelationDefinitionSettings, IEnumerable<Tile> tiles, IEnumerable<TileRelationDirection> directions)
        {
            var sourceTile = tiles.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.SourceTileId));
            var destinationTile = tiles.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.DestinationTileId));
            var direction = directions.Single(x => string.Equals(x.Id, tileRelationDefinitionSettings.DirectionId));
            return new TileRelation(sourceTile, destinationTile, direction);
        }

        private Piece CreatePiece(PieceDefinitionSettings pieceDefinitionSettings)
        {
            return new Piece(pieceDefinitionSettings.PieceId);
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

        protected void AddPieceDefinition(string pieceId)
        {
            _pieceDefinitions.Add(new PieceDefinitionSettings { PieceId = pieceId });
        }
    }
}
