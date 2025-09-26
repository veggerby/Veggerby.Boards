using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Utils;

public static class TestExtensions
{
    public static void ShouldHavePieceState(this GameProgress progress, string pieceId, string tileId)
    {
        var piece = progress.Game.GetPiece(pieceId);
        piece.Should().NotBeNull($"PieceState - piece {pieceId} not found");

        var tile = progress.Game.GetTile(tileId);
        tile.Should().NotBeNull($"PieceState - tile {tileId} not found");

        var state = progress.State.GetState<PieceState>(piece);

        state.Should().NotBeNull("PieceState not found");
        state.CurrentTile.Should().Be(tile, "PieceState with wrong tile");
    }

    public static void ShouldHaveTileWithRelations(this Game game, string tileId, params Tuple<string, string>[] relations)
    {
        var tile = game.GetTile(tileId);
        tile.Should().NotBeNull($"Game - tile {tileId} not found");

        var tileRelations = game.Board.TileRelations.Where(x => x.From.Equals(tile));
        tileRelations.Count().Should().Be(relations.Count(), $"Game - tile {tileId} has incorrect number of relations");

        tileRelations
            .Join(relations, x => x.Direction.Id, x => x.Item1, (tileRelation, relationItem) => new { TileRelation = tileRelation, RelationItem = relationItem })
            .Should().OnlyContain(x => x.TileRelation.To.Id.Equals(x.RelationItem.Item2), $"Game - tile {tileId} a relation has unexpected To tile");
    }
}