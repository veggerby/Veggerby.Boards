using System;
using System.Linq;

using Veggerby.Boards.Chess;
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
        ExpectRelation(ChessIds.Tiles.A1, Constants.Directions.North, ChessIds.Tiles.A2);
        ExpectRelation(ChessIds.Tiles.A1, Constants.Directions.East, ChessIds.Tiles.B1);
        ExpectRelation(ChessIds.Tiles.A1, Constants.Directions.NorthEast, ChessIds.Tiles.B2);

        ExpectRelation(ChessIds.Tiles.H1, Constants.Directions.North, ChessIds.Tiles.H2);
        ExpectRelation(ChessIds.Tiles.H1, Constants.Directions.West, ChessIds.Tiles.G1);
        ExpectRelation(ChessIds.Tiles.H1, Constants.Directions.NorthWest, ChessIds.Tiles.G2);

        ExpectRelation(ChessIds.Tiles.A8, Constants.Directions.South, ChessIds.Tiles.A7);
        ExpectRelation(ChessIds.Tiles.A8, Constants.Directions.East, ChessIds.Tiles.B8);
        ExpectRelation(ChessIds.Tiles.A8, Constants.Directions.SouthEast, ChessIds.Tiles.B7);

        ExpectRelation(ChessIds.Tiles.H8, Constants.Directions.South, ChessIds.Tiles.H7);
        ExpectRelation(ChessIds.Tiles.H8, Constants.Directions.West, ChessIds.Tiles.G8);
        ExpectRelation(ChessIds.Tiles.H8, Constants.Directions.SouthWest, ChessIds.Tiles.G7);

        ExpectRelation(ChessIds.Tiles.D4, Constants.Directions.North, ChessIds.Tiles.D5);
        ExpectRelation(ChessIds.Tiles.D4, Constants.Directions.South, ChessIds.Tiles.D3);
        ExpectRelation(ChessIds.Tiles.D4, Constants.Directions.East, ChessIds.Tiles.E4);
        ExpectRelation(ChessIds.Tiles.D4, Constants.Directions.West, ChessIds.Tiles.C4);

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