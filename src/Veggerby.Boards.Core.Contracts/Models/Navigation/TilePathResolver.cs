using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Core.Contracts.Models.Navigation
{
    public class TilePathResolver
    {
        public IEnumerable<TilePath> ResolvePaths(BoardDefinition boardDefinition, string sourceTileId,
            string destinationTileId)
        {
            var sourceTile = boardDefinition.GetTile(sourceTileId);

            if (sourceTile == null)
            {
                throw new ApplicationException("source tile not found");
            }

            var destinationTile = boardDefinition.GetTile(destinationTileId);

            if (destinationTile == null)
            {
                throw new ApplicationException("destination tile not found");
            }

            var tilepath = new TilePath(sourceTile, sourceTile, null);

            return ResolvePathSteps(boardDefinition, tilepath, destinationTileId)
                .OrderBy(x => x.GetDistance())
                .ToList();
        }

        private IEnumerable<TilePath> ResolvePathSteps(BoardDefinition boardDefinition, TilePath currentPath,
            string destinationTileId)
        {
            if (string.Equals(currentPath.DestinationTileDefinition.TileId, destinationTileId))
            {
                return new [] { currentPath };
            }

            var relations = boardDefinition.GetTileRelationsFromSource(currentPath.DestinationTileDefinition.TileId);

            var results = new List<TilePath>();

            foreach (var relation in relations)
            {
                // avoid infinite recursion
                if (!currentPath.HasPassedOver(relation.DestinationTile))
                {
                    results.AddRange(ResolvePathSteps(boardDefinition, currentPath.Append(relation), destinationTileId));
                }
            }

            return results;
        } 
    }
}