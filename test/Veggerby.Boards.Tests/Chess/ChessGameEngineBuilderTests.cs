using System;


using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Chess;

public class ChessGameEngineBuilderTests
{
    private Tuple<string, string> Relation(string directionId, string tileId)
    {
        return new Tuple<string, string>(directionId, tileId);
    }

    [Fact]
    public void Should_initialize_game()
    {
        // arrange

        // act
        var actual = new ChessGameBuilder().Compile();

        // assert

        // row 1
        actual.Game.ShouldHaveTileWithRelations("tile-a1",
            Relation("east", "tile-b1"),
            Relation("south-east", "tile-b2"),
            Relation("south", "tile-a2"));

        actual.Game.ShouldHaveTileWithRelations("tile-b1",
            Relation("east", "tile-c1"),
            Relation("south-east", "tile-c2"),
            Relation("south", "tile-b2"),
            Relation("south-west", "tile-a2"),
            Relation("west", "tile-a1"));

        actual.Game.ShouldHaveTileWithRelations("tile-c1",
            Relation("east", "tile-d1"),
            Relation("south-east", "tile-d2"),
            Relation("south", "tile-c2"),
            Relation("south-west", "tile-b2"),
            Relation("west", "tile-b1"));

        actual.Game.ShouldHaveTileWithRelations("tile-d1",
            Relation("east", "tile-e1"),
            Relation("south-east", "tile-e2"),
            Relation("south", "tile-d2"),
            Relation("south-west", "tile-c2"),
            Relation("west", "tile-c1"));

        actual.Game.ShouldHaveTileWithRelations("tile-e1",
            Relation("east", "tile-f1"),
            Relation("south-east", "tile-f2"),
            Relation("south", "tile-e2"),
            Relation("south-west", "tile-d2"),
            Relation("west", "tile-d1"));

        actual.Game.ShouldHaveTileWithRelations("tile-f1",
            Relation("east", "tile-g1"),
            Relation("south-east", "tile-g2"),
            Relation("south", "tile-f2"),
            Relation("south-west", "tile-e2"),
            Relation("west", "tile-e1"));

        actual.Game.ShouldHaveTileWithRelations("tile-g1",
            Relation("east", "tile-h1"),
            Relation("south-east", "tile-h2"),
            Relation("south", "tile-g2"),
            Relation("south-west", "tile-f2"),
            Relation("west", "tile-f1"));

        actual.Game.ShouldHaveTileWithRelations("tile-h1",
            Relation("south", "tile-h2"),
            Relation("south-west", "tile-g2"),
            Relation("west", "tile-g1"));

        // row 2
        actual.Game.ShouldHaveTileWithRelations("tile-a2",
            Relation("north", "tile-a1"),
            Relation("north-east", "tile-b1"),
            Relation("east", "tile-b2"),
            Relation("south-east", "tile-b3"),
            Relation("south", "tile-a3"));

        actual.Game.ShouldHaveTileWithRelations("tile-b2",
            Relation("north", "tile-b1"),
            Relation("north-east", "tile-c1"),
            Relation("east", "tile-c2"),
            Relation("south-east", "tile-c3"),
            Relation("south", "tile-b3"),
            Relation("south-west", "tile-a3"),
            Relation("west", "tile-a2"),
            Relation("north-west", "tile-a1"));

        actual.Game.ShouldHaveTileWithRelations("tile-c2",
            Relation("north", "tile-c1"),
            Relation("north-east", "tile-d1"),
            Relation("east", "tile-d2"),
            Relation("south-east", "tile-d3"),
            Relation("south", "tile-c3"),
            Relation("south-west", "tile-b3"),
            Relation("west", "tile-b2"),
            Relation("north-west", "tile-b1"));

        actual.Game.ShouldHaveTileWithRelations("tile-d2",
            Relation("north", "tile-d1"),
            Relation("north-east", "tile-e1"),
            Relation("east", "tile-e2"),
            Relation("south-east", "tile-e3"),
            Relation("south", "tile-d3"),
            Relation("south-west", "tile-c3"),
            Relation("west", "tile-c2"),
            Relation("north-west", "tile-c1"));

        actual.Game.ShouldHaveTileWithRelations("tile-e2",
            Relation("north", "tile-e1"),
            Relation("north-east", "tile-f1"),
            Relation("east", "tile-f2"),
            Relation("south-east", "tile-f3"),
            Relation("south", "tile-e3"),
            Relation("south-west", "tile-d3"),
            Relation("west", "tile-d2"),
            Relation("north-west", "tile-d1"));

        actual.Game.ShouldHaveTileWithRelations("tile-f2",
            Relation("north", "tile-f1"),
            Relation("north-east", "tile-g1"),
            Relation("east", "tile-g2"),
            Relation("south-east", "tile-g3"),
            Relation("south", "tile-f3"),
            Relation("south-west", "tile-e3"),
            Relation("west", "tile-e2"),
            Relation("north-west", "tile-e1"));

        actual.Game.ShouldHaveTileWithRelations("tile-g2",
            Relation("north", "tile-g1"),
            Relation("north-east", "tile-h1"),
            Relation("east", "tile-h2"),
            Relation("south-east", "tile-h3"),
            Relation("south", "tile-g3"),
            Relation("south-west", "tile-f3"),
            Relation("west", "tile-f2"),
            Relation("north-west", "tile-f1"));

        actual.Game.ShouldHaveTileWithRelations("tile-h2",
            Relation("south", "tile-h3"),
            Relation("south-west", "tile-g3"),
            Relation("west", "tile-g2"),
            Relation("north-west", "tile-g1"),
            Relation("north", "tile-h1"));

        // row 3
        actual.Game.ShouldHaveTileWithRelations("tile-a3",
            Relation("north", "tile-a2"),
            Relation("north-east", "tile-b2"),
            Relation("east", "tile-b3"),
            Relation("south-east", "tile-b4"),
            Relation("south", "tile-a4"));

        actual.Game.ShouldHaveTileWithRelations("tile-b3",
            Relation("north", "tile-b2"),
            Relation("north-east", "tile-c2"),
            Relation("east", "tile-c3"),
            Relation("south-east", "tile-c4"),
            Relation("south", "tile-b4"),
            Relation("south-west", "tile-a4"),
            Relation("west", "tile-a3"),
            Relation("north-west", "tile-a2"));

        actual.Game.ShouldHaveTileWithRelations("tile-c3",
            Relation("north", "tile-c2"),
            Relation("north-east", "tile-d2"),
            Relation("east", "tile-d3"),
            Relation("south-east", "tile-d4"),
            Relation("south", "tile-c4"),
            Relation("south-west", "tile-b4"),
            Relation("west", "tile-b3"),
            Relation("north-west", "tile-b2"));

        actual.Game.ShouldHaveTileWithRelations("tile-d3",
            Relation("north", "tile-d2"),
            Relation("north-east", "tile-e2"),
            Relation("east", "tile-e3"),
            Relation("south-east", "tile-e4"),
            Relation("south", "tile-d4"),
            Relation("south-west", "tile-c4"),
            Relation("west", "tile-c3"),
            Relation("north-west", "tile-c2"));

        actual.Game.ShouldHaveTileWithRelations("tile-e3",
            Relation("north", "tile-e2"),
            Relation("north-east", "tile-f2"),
            Relation("east", "tile-f3"),
            Relation("south-east", "tile-f4"),
            Relation("south", "tile-e4"),
            Relation("south-west", "tile-d4"),
            Relation("west", "tile-d3"),
            Relation("north-west", "tile-d2"));

        actual.Game.ShouldHaveTileWithRelations("tile-f3",
            Relation("north", "tile-f2"),
            Relation("north-east", "tile-g2"),
            Relation("east", "tile-g3"),
            Relation("south-east", "tile-g4"),
            Relation("south", "tile-f4"),
            Relation("south-west", "tile-e4"),
            Relation("west", "tile-e3"),
            Relation("north-west", "tile-e2"));

        actual.Game.ShouldHaveTileWithRelations("tile-g3",
            Relation("north", "tile-g2"),
            Relation("north-east", "tile-h2"),
            Relation("east", "tile-h3"),
            Relation("south-east", "tile-h4"),
            Relation("south", "tile-g4"),
            Relation("south-west", "tile-f4"),
            Relation("west", "tile-f3"),
            Relation("north-west", "tile-f2"));

        actual.Game.ShouldHaveTileWithRelations("tile-h3",
            Relation("south", "tile-h4"),
            Relation("south-west", "tile-g4"),
            Relation("west", "tile-g3"),
            Relation("north-west", "tile-g2"),
            Relation("north", "tile-h2"));

        // row 4
        actual.Game.ShouldHaveTileWithRelations("tile-a4",
            Relation("north", "tile-a3"),
            Relation("north-east", "tile-b3"),
            Relation("east", "tile-b4"),
            Relation("south-east", "tile-b5"),
            Relation("south", "tile-a5"));

        actual.Game.ShouldHaveTileWithRelations("tile-b4",
            Relation("north", "tile-b3"),
            Relation("north-east", "tile-c3"),
            Relation("east", "tile-c4"),
            Relation("south-east", "tile-c5"),
            Relation("south", "tile-b5"),
            Relation("south-west", "tile-a5"),
            Relation("west", "tile-a4"),
            Relation("north-west", "tile-a3"));

        actual.Game.ShouldHaveTileWithRelations("tile-c4",
            Relation("north", "tile-c3"),
            Relation("north-east", "tile-d3"),
            Relation("east", "tile-d4"),
            Relation("south-east", "tile-d5"),
            Relation("south", "tile-c5"),
            Relation("south-west", "tile-b5"),
            Relation("west", "tile-b4"),
            Relation("north-west", "tile-b3"));

        actual.Game.ShouldHaveTileWithRelations("tile-d4",
            Relation("north", "tile-d3"),
            Relation("north-east", "tile-e3"),
            Relation("east", "tile-e4"),
            Relation("south-east", "tile-e5"),
            Relation("south", "tile-d5"),
            Relation("south-west", "tile-c5"),
            Relation("west", "tile-c4"),
            Relation("north-west", "tile-c3"));

        actual.Game.ShouldHaveTileWithRelations("tile-e4",
            Relation("north", "tile-e3"),
            Relation("north-east", "tile-f3"),
            Relation("east", "tile-f4"),
            Relation("south-east", "tile-f5"),
            Relation("south", "tile-e5"),
            Relation("south-west", "tile-d5"),
            Relation("west", "tile-d4"),
            Relation("north-west", "tile-d3"));

        actual.Game.ShouldHaveTileWithRelations("tile-f4",
            Relation("north", "tile-f3"),
            Relation("north-east", "tile-g3"),
            Relation("east", "tile-g4"),
            Relation("south-east", "tile-g5"),
            Relation("south", "tile-f5"),
            Relation("south-west", "tile-e5"),
            Relation("west", "tile-e4"),
            Relation("north-west", "tile-e3"));

        actual.Game.ShouldHaveTileWithRelations("tile-g4",
            Relation("north", "tile-g3"),
            Relation("north-east", "tile-h3"),
            Relation("east", "tile-h4"),
            Relation("south-east", "tile-h5"),
            Relation("south", "tile-g5"),
            Relation("south-west", "tile-f5"),
            Relation("west", "tile-f4"),
            Relation("north-west", "tile-f3"));

        actual.Game.ShouldHaveTileWithRelations("tile-h4",
            Relation("south", "tile-h5"),
            Relation("south-west", "tile-g5"),
            Relation("west", "tile-g4"),
            Relation("north-west", "tile-g3"),
            Relation("north", "tile-h3"));

        // row 5
        actual.Game.ShouldHaveTileWithRelations("tile-a5",
            Relation("north", "tile-a4"),
            Relation("north-east", "tile-b4"),
            Relation("east", "tile-b5"),
            Relation("south-east", "tile-b6"),
            Relation("south", "tile-a6"));

        actual.Game.ShouldHaveTileWithRelations("tile-b5",
            Relation("north", "tile-b4"),
            Relation("north-east", "tile-c4"),
            Relation("east", "tile-c5"),
            Relation("south-east", "tile-c6"),
            Relation("south", "tile-b6"),
            Relation("south-west", "tile-a6"),
            Relation("west", "tile-a5"),
            Relation("north-west", "tile-a4"));

        actual.Game.ShouldHaveTileWithRelations("tile-c5",
            Relation("north", "tile-c4"),
            Relation("north-east", "tile-d4"),
            Relation("east", "tile-d5"),
            Relation("south-east", "tile-d6"),
            Relation("south", "tile-c6"),
            Relation("south-west", "tile-b6"),
            Relation("west", "tile-b5"),
            Relation("north-west", "tile-b4"));

        actual.Game.ShouldHaveTileWithRelations("tile-d5",
            Relation("north", "tile-d4"),
            Relation("north-east", "tile-e4"),
            Relation("east", "tile-e5"),
            Relation("south-east", "tile-e6"),
            Relation("south", "tile-d6"),
            Relation("south-west", "tile-c6"),
            Relation("west", "tile-c5"),
            Relation("north-west", "tile-c4"));

        actual.Game.ShouldHaveTileWithRelations("tile-e5",
            Relation("north", "tile-e4"),
            Relation("north-east", "tile-f4"),
            Relation("east", "tile-f5"),
            Relation("south-east", "tile-f6"),
            Relation("south", "tile-e6"),
            Relation("south-west", "tile-d6"),
            Relation("west", "tile-d5"),
            Relation("north-west", "tile-d4"));

        actual.Game.ShouldHaveTileWithRelations("tile-f5",
            Relation("north", "tile-f4"),
            Relation("north-east", "tile-g4"),
            Relation("east", "tile-g5"),
            Relation("south-east", "tile-g6"),
            Relation("south", "tile-f6"),
            Relation("south-west", "tile-e6"),
            Relation("west", "tile-e5"),
            Relation("north-west", "tile-e4"));

        actual.Game.ShouldHaveTileWithRelations("tile-g5",
            Relation("north", "tile-g4"),
            Relation("north-east", "tile-h4"),
            Relation("east", "tile-h5"),
            Relation("south-east", "tile-h6"),
            Relation("south", "tile-g6"),
            Relation("south-west", "tile-f6"),
            Relation("west", "tile-f5"),
            Relation("north-west", "tile-f4"));

        actual.Game.ShouldHaveTileWithRelations("tile-h5",
            Relation("south", "tile-h6"),
            Relation("south-west", "tile-g6"),
            Relation("west", "tile-g5"),
            Relation("north-west", "tile-g4"),
            Relation("north", "tile-h4"));

        // row 6
        actual.Game.ShouldHaveTileWithRelations("tile-a6",
            Relation("north", "tile-a5"),
            Relation("north-east", "tile-b5"),
            Relation("east", "tile-b6"),
            Relation("south-east", "tile-b7"),
            Relation("south", "tile-a7"));

        actual.Game.ShouldHaveTileWithRelations("tile-b6",
            Relation("north", "tile-b5"),
            Relation("north-east", "tile-c5"),
            Relation("east", "tile-c6"),
            Relation("south-east", "tile-c7"),
            Relation("south", "tile-b7"),
            Relation("south-west", "tile-a7"),
            Relation("west", "tile-a6"),
            Relation("north-west", "tile-a5"));

        actual.Game.ShouldHaveTileWithRelations("tile-c6",
            Relation("north", "tile-c5"),
            Relation("north-east", "tile-d5"),
            Relation("east", "tile-d6"),
            Relation("south-east", "tile-d7"),
            Relation("south", "tile-c7"),
            Relation("south-west", "tile-b7"),
            Relation("west", "tile-b6"),
            Relation("north-west", "tile-b5"));

        actual.Game.ShouldHaveTileWithRelations("tile-d6",
            Relation("north", "tile-d5"),
            Relation("north-east", "tile-e5"),
            Relation("east", "tile-e6"),
            Relation("south-east", "tile-e7"),
            Relation("south", "tile-d7"),
            Relation("south-west", "tile-c7"),
            Relation("west", "tile-c6"),
            Relation("north-west", "tile-c5"));

        actual.Game.ShouldHaveTileWithRelations("tile-e6",
            Relation("north", "tile-e5"),
            Relation("north-east", "tile-f5"),
            Relation("east", "tile-f6"),
            Relation("south-east", "tile-f7"),
            Relation("south", "tile-e7"),
            Relation("south-west", "tile-d7"),
            Relation("west", "tile-d6"),
            Relation("north-west", "tile-d5"));

        actual.Game.ShouldHaveTileWithRelations("tile-f6",
            Relation("north", "tile-f5"),
            Relation("north-east", "tile-g5"),
            Relation("east", "tile-g6"),
            Relation("south-east", "tile-g7"),
            Relation("south", "tile-f7"),
            Relation("south-west", "tile-e7"),
            Relation("west", "tile-e6"),
            Relation("north-west", "tile-e5"));

        actual.Game.ShouldHaveTileWithRelations("tile-g6",
            Relation("north", "tile-g5"),
            Relation("north-east", "tile-h5"),
            Relation("east", "tile-h6"),
            Relation("south-east", "tile-h7"),
            Relation("south", "tile-g7"),
            Relation("south-west", "tile-f7"),
            Relation("west", "tile-f6"),
            Relation("north-west", "tile-f5"));

        actual.Game.ShouldHaveTileWithRelations("tile-h6",
            Relation("south", "tile-h7"),
            Relation("south-west", "tile-g7"),
            Relation("west", "tile-g6"),
            Relation("north-west", "tile-g5"),
            Relation("north", "tile-h5"));

        // row 7
        actual.Game.ShouldHaveTileWithRelations("tile-a7",
            Relation("north", "tile-a6"),
            Relation("north-east", "tile-b6"),
            Relation("east", "tile-b7"),
            Relation("south-east", "tile-b8"),
            Relation("south", "tile-a8"));

        actual.Game.ShouldHaveTileWithRelations("tile-b7",
            Relation("north", "tile-b6"),
            Relation("north-east", "tile-c6"),
            Relation("east", "tile-c7"),
            Relation("south-east", "tile-c8"),
            Relation("south", "tile-b8"),
            Relation("south-west", "tile-a8"),
            Relation("west", "tile-a7"),
            Relation("north-west", "tile-a6"));

        actual.Game.ShouldHaveTileWithRelations("tile-c7",
            Relation("north", "tile-c6"),
            Relation("north-east", "tile-d6"),
            Relation("east", "tile-d7"),
            Relation("south-east", "tile-d8"),
            Relation("south", "tile-c8"),
            Relation("south-west", "tile-b8"),
            Relation("west", "tile-b7"),
            Relation("north-west", "tile-b6"));

        actual.Game.ShouldHaveTileWithRelations("tile-d7",
            Relation("north", "tile-d6"),
            Relation("north-east", "tile-e6"),
            Relation("east", "tile-e7"),
            Relation("south-east", "tile-e8"),
            Relation("south", "tile-d8"),
            Relation("south-west", "tile-c8"),
            Relation("west", "tile-c7"),
            Relation("north-west", "tile-c6"));

        actual.Game.ShouldHaveTileWithRelations("tile-e7",
            Relation("north", "tile-e6"),
            Relation("north-east", "tile-f6"),
            Relation("east", "tile-f7"),
            Relation("south-east", "tile-f8"),
            Relation("south", "tile-e8"),
            Relation("south-west", "tile-d8"),
            Relation("west", "tile-d7"),
            Relation("north-west", "tile-d6"));

        actual.Game.ShouldHaveTileWithRelations("tile-f7",
            Relation("north", "tile-f6"),
            Relation("north-east", "tile-g6"),
            Relation("east", "tile-g7"),
            Relation("south-east", "tile-g8"),
            Relation("south", "tile-f8"),
            Relation("south-west", "tile-e8"),
            Relation("west", "tile-e7"),
            Relation("north-west", "tile-e6"));

        actual.Game.ShouldHaveTileWithRelations("tile-g7",
            Relation("north", "tile-g6"),
            Relation("north-east", "tile-h6"),
            Relation("east", "tile-h7"),
            Relation("south-east", "tile-h8"),
            Relation("south", "tile-g8"),
            Relation("south-west", "tile-f8"),
            Relation("west", "tile-f7"),
            Relation("north-west", "tile-f6"));

        actual.Game.ShouldHaveTileWithRelations("tile-h7",
            Relation("south", "tile-h8"),
            Relation("south-west", "tile-g8"),
            Relation("west", "tile-g7"),
            Relation("north-west", "tile-g6"),
            Relation("north", "tile-h6"));

        // row 8
        actual.Game.ShouldHaveTileWithRelations("tile-a8",
            Relation("north", "tile-a7"),
            Relation("north-east", "tile-b7"),
            Relation("east", "tile-b8"));

        actual.Game.ShouldHaveTileWithRelations("tile-b8",
            Relation("north", "tile-b7"),
            Relation("north-east", "tile-c7"),
            Relation("east", "tile-c8"),
            Relation("west", "tile-a8"),
            Relation("north-west", "tile-a7"));

        actual.Game.ShouldHaveTileWithRelations("tile-c8",
            Relation("north", "tile-c7"),
            Relation("north-east", "tile-d7"),
            Relation("east", "tile-d8"),
            Relation("west", "tile-b8"),
            Relation("north-west", "tile-b7"));

        actual.Game.ShouldHaveTileWithRelations("tile-d8",
            Relation("north", "tile-d7"),
            Relation("north-east", "tile-e7"),
            Relation("east", "tile-e8"),
            Relation("west", "tile-c8"),
            Relation("north-west", "tile-c7"));

        actual.Game.ShouldHaveTileWithRelations("tile-e8",
            Relation("north", "tile-e7"),
            Relation("north-east", "tile-f7"),
            Relation("east", "tile-f8"),
            Relation("west", "tile-d8"),
            Relation("north-west", "tile-d7"));

        actual.Game.ShouldHaveTileWithRelations("tile-f8",
            Relation("north", "tile-f7"),
            Relation("north-east", "tile-g7"),
            Relation("east", "tile-g8"),
            Relation("west", "tile-e8"),
            Relation("north-west", "tile-e7"));

        actual.Game.ShouldHaveTileWithRelations("tile-g8",
            Relation("north", "tile-g7"),
            Relation("north-east", "tile-h7"),
            Relation("east", "tile-h8"),
            Relation("west", "tile-f8"),
            Relation("north-west", "tile-f7"));

        actual.Game.ShouldHaveTileWithRelations("tile-h8",
            Relation("west", "tile-g8"),
            Relation("north-west", "tile-g7"),
            Relation("north", "tile-h7"));

        // state
        actual.ShouldHavePieceState("white-rook-1", "tile-a1");
        actual.ShouldHavePieceState("white-knight-1", "tile-b1");
        actual.ShouldHavePieceState("white-bishop-1", "tile-c1");
        actual.ShouldHavePieceState("white-king", "tile-d1");
        actual.ShouldHavePieceState("white-queen", "tile-e1");
        actual.ShouldHavePieceState("white-bishop-2", "tile-f1");
        actual.ShouldHavePieceState("white-knight-2", "tile-g1");
        actual.ShouldHavePieceState("white-rook-2", "tile-h1");

        actual.ShouldHavePieceState("white-pawn-1", "tile-a2");
        actual.ShouldHavePieceState("white-pawn-2", "tile-b2");
        actual.ShouldHavePieceState("white-pawn-3", "tile-c2");
        actual.ShouldHavePieceState("white-pawn-4", "tile-d2");
        actual.ShouldHavePieceState("white-pawn-5", "tile-e2");
        actual.ShouldHavePieceState("white-pawn-6", "tile-f2");
        actual.ShouldHavePieceState("white-pawn-7", "tile-g2");
        actual.ShouldHavePieceState("white-pawn-8", "tile-h2");

        actual.ShouldHavePieceState("black-pawn-1", "tile-a7");
        actual.ShouldHavePieceState("black-pawn-2", "tile-b7");
        actual.ShouldHavePieceState("black-pawn-3", "tile-c7");
        actual.ShouldHavePieceState("black-pawn-4", "tile-d7");
        actual.ShouldHavePieceState("black-pawn-5", "tile-e7");
        actual.ShouldHavePieceState("black-pawn-6", "tile-f7");
        actual.ShouldHavePieceState("black-pawn-7", "tile-g7");
        actual.ShouldHavePieceState("black-pawn-8", "tile-h7");

        actual.ShouldHavePieceState("black-rook-1", "tile-a8");
        actual.ShouldHavePieceState("black-knight-1", "tile-b8");
        actual.ShouldHavePieceState("black-bishop-1", "tile-c8");
        actual.ShouldHavePieceState("black-king", "tile-d8");
        actual.ShouldHavePieceState("black-queen", "tile-e8");
        actual.ShouldHavePieceState("black-bishop-2", "tile-f8");
        actual.ShouldHavePieceState("black-knight-2", "tile-g8");
        actual.ShouldHavePieceState("black-rook-2", "tile-h8");
    }
}