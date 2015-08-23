using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Contracts.Models.Definitions.Builder
{
    public abstract class BoardDefinitionBuilder
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
            public int Distance { get; set; }
        }

        private class PieceDefinitionSettings
        {
            public string PieceId { get; set; }
        }

        protected string BoardId { get; set; }

        private readonly IList<TileDefinitionSettings> _tileDefinitions = new List<TileDefinitionSettings>();
        private readonly IList<DirectionDefinitionSettings> _directionDefinitions = new List<DirectionDefinitionSettings>();
        private readonly IList<TileRelationDefinitionSettings> _tileRelationDefinitions = new List<TileRelationDefinitionSettings>();
        private readonly IList<PieceDefinitionSettings> _pieceDefinitions = new List<PieceDefinitionSettings>();

        public abstract void Build();

        private BoardDefinition _boardDefinition;

        public BoardDefinition Compile()
        {
            if (_boardDefinition != null)
            {
                return _boardDefinition;
            }

            Build();
            var tiles = _tileDefinitions.Select(CreateTileDefinition).ToArray();
            var directions = _directionDefinitions.Select(CreateDirectionDefinition).ToArray();
            var relations = _tileRelationDefinitions.Select(x => CreateTileRelationDefinition(x, tiles, directions)).ToArray();
            var pieces = _pieceDefinitions.Select(CreatePieceDefinition).ToArray();
            _boardDefinition = new BoardDefinition(BoardId, tiles, relations, pieces);
            return _boardDefinition;
        }

        private TileDefinition CreateTileDefinition(TileDefinitionSettings tileDefinitionSettings)
        {
            return new TileDefinition(tileDefinitionSettings.TileId);
        }

        private DirectionDefinition CreateDirectionDefinition(DirectionDefinitionSettings directionDefinitionSettings)
        {
            return new DirectionDefinition(directionDefinitionSettings.DirectionId);
        }

        private TileRelationDefinition CreateTileRelationDefinition(TileRelationDefinitionSettings tileRelationDefinitionSettings, IEnumerable<TileDefinition> tiles, IEnumerable<DirectionDefinition> directions)
        {
            var sourceTile = tiles.Single(x => string.Equals(x.TileId, tileRelationDefinitionSettings.SourceTileId));
            var destinationTile = tiles.Single(x => string.Equals(x.TileId, tileRelationDefinitionSettings.DestinationTileId));
            var direction = directions.Single(x => string.Equals(x.DirectionId, tileRelationDefinitionSettings.DirectionId));
            return new TileRelationDefinition(sourceTile, destinationTile, direction, tileRelationDefinitionSettings.Distance);
        }

        private PieceDefinition CreatePieceDefinition(PieceDefinitionSettings pieceDefinitionSettings)
        {
            return new PieceDefinition(pieceDefinitionSettings.PieceId);
        }

        protected void AddTileDefinition(string tileId)
        {
            _tileDefinitions.Add(new TileDefinitionSettings { TileId = tileId });
        }
        protected void AddDirectionDefinition(string directionId)
        {
            _directionDefinitions.Add(new DirectionDefinitionSettings { DirectionId = directionId });
        }

        protected void AddTileRelationDefinition(string sourceTileId, string destinationTileId, string directionId, int distance = 1)
        {
            _tileRelationDefinitions.Add(new TileRelationDefinitionSettings { SourceTileId = sourceTileId, DestinationTileId = destinationTileId, DirectionId = directionId, Distance = distance });
        }

        protected void AddPieceDefinition(string pieceId)
        {
            _pieceDefinitions.Add(new PieceDefinitionSettings {PieceId = pieceId});
        }
    }
}
