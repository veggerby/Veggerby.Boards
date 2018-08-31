using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Utils
{
    public static class TestExtensions
    {
        public static void ShouldHavePieceState(this GameState gameState, string pieceId, string tileId)
        {
            var piece = gameState.Game.GetPiece(pieceId);
            piece.ShouldNotBeNull($"PieceState - piece {pieceId} not found");

            var tile = gameState.Game.GetTile(tileId);
            tile.ShouldNotBeNull($"PieceState - tile {tileId} not found");

            var state = gameState.GetState<PieceState>(piece);

            state.ShouldNotBeNull("PieceState not found");
            state.CurrentTile.ShouldBe(tile, "PieceState with wrong tile");
        }

        public static void ShouldHaveTileWithRelations(this Game game, string tileId, params Tuple<string, string>[] relations)
        {
            var tile = game.GetTile(tileId);
            tile.ShouldNotBeNull($"Game - tile {tileId} not found");

            var tileRelations = game.Board.TileRelations.Where(x => x.From.Equals(tile));
            tileRelations.Count().ShouldBe(relations.Count(), $"Game - tile {tileId} has incorrect number of relations");

            tileRelations
                .Join(relations, x => x.Direction.Id, x => x.Item1, (tileRelation, relationItem) => new { TileRelation = tileRelation, RelationItem = relationItem })
                .ShouldAllBe(x => x.TileRelation.To.Id.Equals(x.RelationItem.Item2), $"Game - tile {tileId} a relation has unexpected To tile");
        }
    }
}