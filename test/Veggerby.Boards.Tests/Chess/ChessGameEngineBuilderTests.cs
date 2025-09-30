using System;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Utils;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

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

        // piece state assertions (standard chess initial placement)
        (string PieceId, string TileId)[] whiteBack =
        [
            (WhiteRook1, A1),
            (WhiteKnight1, B1),
            (WhiteBishop1, C1),
            (WhiteQueen, D1),
            (WhiteKing, E1),
            (WhiteBishop2, F1),
            (WhiteKnight2, G1),
            (WhiteRook2, H1)
        ];
        foreach (var (pieceId, tileId) in whiteBack)
        {
            actual.ShouldHavePieceState(pieceId, tileId);
        }

        string[] whitePawns = [WhitePawn1, WhitePawn2, WhitePawn3, WhitePawn4, WhitePawn5, WhitePawn6, WhitePawn7, WhitePawn8];
        string[] whitePawnTiles = [A2, B2, C2, D2, E2, F2, G2, H2];
        for (int i = 0; i < whitePawns.Length; i++)
        {
            actual.ShouldHavePieceState(whitePawns[i], whitePawnTiles[i]);
        }

        (string PieceId, string TileId)[] blackBack =
        [
            (BlackRook1, A8),
            (BlackKnight1, B8),
            (BlackBishop1, C8),
            (BlackQueen, D8),
            (BlackKing, E8),
            (BlackBishop2, F8),
            (BlackKnight2, G8),
            (BlackRook2, H8)
        ];
        foreach (var (pieceId, tileId) in blackBack)
        {
            actual.ShouldHavePieceState(pieceId, tileId);
        }

        string[] blackPawns = [BlackPawn1, BlackPawn2, BlackPawn3, BlackPawn4, BlackPawn5, BlackPawn6, BlackPawn7, BlackPawn8];
        string[] blackPawnTiles = [A7, B7, C7, D7, E7, F7, G7, H7];
        for (int i = 0; i < blackPawns.Length; i++)
        {
            actual.ShouldHavePieceState(blackPawns[i], blackPawnTiles[i]);
        }
    }
}