using System;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Chess;

public class ChessGameEngineBuilderTests
{
    private static Tuple<string, string> Relation(string directionId, string tileId)
    {
        return new Tuple<string, string>(directionId, tileId);
    }

    [Fact]
    public void Should_initialize_game()
    {
        var actual = new ChessGameBuilder().Compile();

        // local helper for partial relation presence (does not assert full set)
        void ExpectRelation(string fromId, string directionId, string toId)
        {
            var from = actual.Game.GetTile(fromId);
            var rel = actual.Game.Board.TileRelations.FirstOrDefault(r => r.From.Equals(from) && r.Direction.Id == directionId);
            rel.Should().NotBeNull($"Expected relation {directionId} from {fromId} -> {toId}");
            rel!.To.Id.Should().Be(toId, $"Relation {directionId} from {fromId} should target {toId}");
        }

        // Spot-check orientation on corners and a few interior tiles
        ExpectRelation("tile-a1", "north", "tile-a2");
        ExpectRelation("tile-a1", "east", "tile-b1");
        ExpectRelation("tile-a1", "north-east", "tile-b2");

        ExpectRelation("tile-h1", "north", "tile-h2");
        ExpectRelation("tile-h1", "west", "tile-g1");
        ExpectRelation("tile-h1", "north-west", "tile-g2");

        ExpectRelation("tile-a8", "south", "tile-a7");
        ExpectRelation("tile-a8", "east", "tile-b8");
        ExpectRelation("tile-a8", "south-east", "tile-b7");

        ExpectRelation("tile-h8", "south", "tile-h7");
        ExpectRelation("tile-h8", "west", "tile-g8");
        ExpectRelation("tile-h8", "south-west", "tile-g7");

        ExpectRelation("tile-d4", "north", "tile-d5");
        ExpectRelation("tile-d4", "south", "tile-d3");
        ExpectRelation("tile-d4", "east", "tile-e4");
        ExpectRelation("tile-d4", "west", "tile-c4");

        // piece state assertions
        // Standard placement: queen on d-file, king on e-file
        string[] whiteBack = { "white-rook-1:a1", "white-knight-1:b1", "white-bishop-1:c1", "white-queen:d1", "white-king:e1", "white-bishop-2:f1", "white-knight-2:g1", "white-rook-2:h1" };
        foreach (var entry in whiteBack)
        {
            var parts = entry.Split(':');
            actual.ShouldHavePieceState(parts[0], $"tile-{parts[1]}");
        }
        for (int i = 1; i <= 8; i++)
        {
            var fileChar = (char)('a' + i - 1);
            actual.ShouldHavePieceState($"white-pawn-{i}", $"tile-{fileChar}2");
        }

        string[] blackBack = { "black-rook-1:a8", "black-knight-1:b8", "black-bishop-1:c8", "black-queen:d8", "black-king:e8", "black-bishop-2:f8", "black-knight-2:g8", "black-rook-2:h8" };
        foreach (var entry in blackBack)
        {
            var parts = entry.Split(':');
            actual.ShouldHavePieceState(parts[0], $"tile-{parts[1]}");
        }
        for (int i = 1; i <= 8; i++)
        {
            var fileChar = (char)('a' + i - 1);
            actual.ShouldHavePieceState($"black-pawn-{i}", $"tile-{fileChar}7");
        }
    }
}