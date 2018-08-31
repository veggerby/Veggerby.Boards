using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Utils;
using Xunit;

namespace Veggerby.Boards.Tests.Chess
{
    public class ChessGameBuilderTests
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
            actual.ShouldHaveTileWithRelations("tile-a1",
                Relation("east",       "tile-b1"),
                Relation("south-east", "tile-b2"),
                Relation("south",      "tile-a2"));

            actual.ShouldHaveTileWithRelations("tile-b1",
                Relation("east",       "tile-c1"),
                Relation("south-east", "tile-c2"),
                Relation("south",      "tile-b2"),
                Relation("south-west", "tile-a2"),
                Relation("west",       "tile-a1"));

            actual.ShouldHaveTileWithRelations("tile-c1",
                Relation("east",       "tile-d1"),
                Relation("south-east", "tile-d2"),
                Relation("south",      "tile-c2"),
                Relation("south-west", "tile-b2"),
                Relation("west",       "tile-b1"));

            actual.ShouldHaveTileWithRelations("tile-d1",
                Relation("east",       "tile-e1"),
                Relation("south-east", "tile-e2"),
                Relation("south",      "tile-d2"),
                Relation("south-west", "tile-c2"),
                Relation("west",       "tile-c1"));

            actual.ShouldHaveTileWithRelations("tile-e1",
                Relation("east",       "tile-f1"),
                Relation("south-east", "tile-f2"),
                Relation("south",      "tile-e2"),
                Relation("south-west", "tile-d2"),
                Relation("west",       "tile-d1"));

            actual.ShouldHaveTileWithRelations("tile-f1",
                Relation("east",       "tile-g1"),
                Relation("south-east", "tile-g2"),
                Relation("south",      "tile-f2"),
                Relation("south-west", "tile-e2"),
                Relation("west",       "tile-e1"));

            actual.ShouldHaveTileWithRelations("tile-g1",
                Relation("east",       "tile-h1"),
                Relation("south-east", "tile-h2"),
                Relation("south",      "tile-g2"),
                Relation("south-west", "tile-f2"),
                Relation("west",       "tile-f1"));

            actual.ShouldHaveTileWithRelations("tile-h1",
                Relation("south",      "tile-h2"),
                Relation("south-west", "tile-g2"),
                Relation("west",       "tile-g1"));

            // row 2
            actual.ShouldHaveTileWithRelations("tile-a2",
                Relation("north",      "tile-a1"),
                Relation("north-east", "tile-b1"),
                Relation("east",       "tile-b2"),
                Relation("south-east", "tile-b3"),
                Relation("south",      "tile-a3"));

            actual.ShouldHaveTileWithRelations("tile-b2",
                Relation("north",      "tile-b1"),
                Relation("north-east", "tile-c1"),
                Relation("east",       "tile-c2"),
                Relation("south-east", "tile-c3"),
                Relation("south",      "tile-b3"),
                Relation("south-west", "tile-a3"),
                Relation("west",       "tile-a2"),
                Relation("north-west", "tile-a1"));

            actual.ShouldHaveTileWithRelations("tile-c2",
                Relation("north",      "tile-c1"),
                Relation("north-east", "tile-d1"),
                Relation("east",       "tile-d2"),
                Relation("south-east", "tile-d3"),
                Relation("south",      "tile-c3"),
                Relation("south-west", "tile-b3"),
                Relation("west",       "tile-b2"),
                Relation("north-west", "tile-b1"));

            actual.ShouldHaveTileWithRelations("tile-d2",
                Relation("north",      "tile-d1"),
                Relation("north-east", "tile-e1"),
                Relation("east",       "tile-e2"),
                Relation("south-east", "tile-e3"),
                Relation("south",      "tile-d3"),
                Relation("south-west", "tile-c3"),
                Relation("west",       "tile-c2"),
                Relation("north-west", "tile-c1"));

            actual.ShouldHaveTileWithRelations("tile-e2",
                Relation("north",      "tile-e1"),
                Relation("north-east", "tile-f1"),
                Relation("east",       "tile-f2"),
                Relation("south-east", "tile-f3"),
                Relation("south",      "tile-e3"),
                Relation("south-west", "tile-d3"),
                Relation("west",       "tile-d2"),
                Relation("north-west", "tile-d1"));

            actual.ShouldHaveTileWithRelations("tile-f2",
                Relation("north",      "tile-f1"),
                Relation("north-east", "tile-g1"),
                Relation("east",       "tile-g2"),
                Relation("south-east", "tile-g3"),
                Relation("south",      "tile-f3"),
                Relation("south-west", "tile-e3"),
                Relation("west",       "tile-e2"),
                Relation("north-west", "tile-e1"));

            actual.ShouldHaveTileWithRelations("tile-g2",
                Relation("north",      "tile-g1"),
                Relation("north-east", "tile-h1"),
                Relation("east",       "tile-h2"),
                Relation("south-east", "tile-h3"),
                Relation("south",      "tile-g3"),
                Relation("south-west", "tile-f3"),
                Relation("west",       "tile-f2"),
                Relation("north-west", "tile-f1"));

            actual.ShouldHaveTileWithRelations("tile-h2",
                Relation("south",      "tile-h3"),
                Relation("south-west", "tile-g3"),
                Relation("west",       "tile-g2"),
                Relation("north-west", "tile-g1"),
                Relation("north",      "tile-h1"));

            // row 3
            actual.ShouldHaveTileWithRelations("tile-a3",
                Relation("north",      "tile-a2"),
                Relation("north-east", "tile-b2"),
                Relation("east",       "tile-b3"),
                Relation("south-east", "tile-b4"),
                Relation("south",      "tile-a4"));

            actual.ShouldHaveTileWithRelations("tile-b3",
                Relation("north",      "tile-b2"),
                Relation("north-east", "tile-c2"),
                Relation("east",       "tile-c3"),
                Relation("south-east", "tile-c4"),
                Relation("south",      "tile-b4"),
                Relation("south-west", "tile-a4"),
                Relation("west",       "tile-a3"),
                Relation("north-west", "tile-a2"));

            actual.ShouldHaveTileWithRelations("tile-c3",
                Relation("north",      "tile-c2"),
                Relation("north-east", "tile-d2"),
                Relation("east",       "tile-d3"),
                Relation("south-east", "tile-d4"),
                Relation("south",      "tile-c4"),
                Relation("south-west", "tile-b4"),
                Relation("west",       "tile-b3"),
                Relation("north-west", "tile-b2"));

            actual.ShouldHaveTileWithRelations("tile-d3",
                Relation("north",      "tile-d2"),
                Relation("north-east", "tile-e2"),
                Relation("east",       "tile-e3"),
                Relation("south-east", "tile-e4"),
                Relation("south",      "tile-d4"),
                Relation("south-west", "tile-c4"),
                Relation("west",       "tile-c3"),
                Relation("north-west", "tile-c2"));

            actual.ShouldHaveTileWithRelations("tile-e3",
                Relation("north",      "tile-e2"),
                Relation("north-east", "tile-f2"),
                Relation("east",       "tile-f3"),
                Relation("south-east", "tile-f4"),
                Relation("south",      "tile-e4"),
                Relation("south-west", "tile-d4"),
                Relation("west",       "tile-d3"),
                Relation("north-west", "tile-d2"));

            actual.ShouldHaveTileWithRelations("tile-f3",
                Relation("north",      "tile-f2"),
                Relation("north-east", "tile-g2"),
                Relation("east",       "tile-g3"),
                Relation("south-east", "tile-g4"),
                Relation("south",      "tile-f4"),
                Relation("south-west", "tile-e4"),
                Relation("west",       "tile-e3"),
                Relation("north-west", "tile-e2"));

            actual.ShouldHaveTileWithRelations("tile-g3",
                Relation("north",      "tile-g2"),
                Relation("north-east", "tile-h2"),
                Relation("east",       "tile-h3"),
                Relation("south-east", "tile-h4"),
                Relation("south",      "tile-g4"),
                Relation("south-west", "tile-f4"),
                Relation("west",       "tile-f3"),
                Relation("north-west", "tile-f2"));

            actual.ShouldHaveTileWithRelations("tile-h3",
                Relation("south",      "tile-h4"),
                Relation("south-west", "tile-g4"),
                Relation("west",       "tile-g3"),
                Relation("north-west", "tile-g2"),
                Relation("north",      "tile-h2"));

            // row 4
            actual.ShouldHaveTileWithRelations("tile-a4",
                Relation("north",      "tile-a3"),
                Relation("north-east", "tile-b3"),
                Relation("east",       "tile-b4"),
                Relation("south-east", "tile-b5"),
                Relation("south",      "tile-a5"));

            actual.ShouldHaveTileWithRelations("tile-b4",
                Relation("north",      "tile-b3"),
                Relation("north-east", "tile-c3"),
                Relation("east",       "tile-c4"),
                Relation("south-east", "tile-c5"),
                Relation("south",      "tile-b5"),
                Relation("south-west", "tile-a5"),
                Relation("west",       "tile-a4"),
                Relation("north-west", "tile-a3"));

            actual.ShouldHaveTileWithRelations("tile-c4",
                Relation("north",      "tile-c3"),
                Relation("north-east", "tile-d3"),
                Relation("east",       "tile-d4"),
                Relation("south-east", "tile-d5"),
                Relation("south",      "tile-c5"),
                Relation("south-west", "tile-b5"),
                Relation("west",       "tile-b4"),
                Relation("north-west", "tile-b3"));

            actual.ShouldHaveTileWithRelations("tile-d4",
                Relation("north",      "tile-d3"),
                Relation("north-east", "tile-e3"),
                Relation("east",       "tile-e4"),
                Relation("south-east", "tile-e5"),
                Relation("south",      "tile-d5"),
                Relation("south-west", "tile-c5"),
                Relation("west",       "tile-c4"),
                Relation("north-west", "tile-c3"));

            actual.ShouldHaveTileWithRelations("tile-e4",
                Relation("north",      "tile-e3"),
                Relation("north-east", "tile-f3"),
                Relation("east",       "tile-f4"),
                Relation("south-east", "tile-f5"),
                Relation("south",      "tile-e5"),
                Relation("south-west", "tile-d5"),
                Relation("west",       "tile-d4"),
                Relation("north-west", "tile-d3"));

            actual.ShouldHaveTileWithRelations("tile-f4",
                Relation("north",      "tile-f3"),
                Relation("north-east", "tile-g3"),
                Relation("east",       "tile-g4"),
                Relation("south-east", "tile-g5"),
                Relation("south",      "tile-f5"),
                Relation("south-west", "tile-e5"),
                Relation("west",       "tile-e4"),
                Relation("north-west", "tile-e3"));

            actual.ShouldHaveTileWithRelations("tile-g4",
                Relation("north",      "tile-g3"),
                Relation("north-east", "tile-h3"),
                Relation("east",       "tile-h4"),
                Relation("south-east", "tile-h5"),
                Relation("south",      "tile-g5"),
                Relation("south-west", "tile-f5"),
                Relation("west",       "tile-f4"),
                Relation("north-west", "tile-f3"));

            actual.ShouldHaveTileWithRelations("tile-h4",
                Relation("south",      "tile-h5"),
                Relation("south-west", "tile-g5"),
                Relation("west",       "tile-g4"),
                Relation("north-west", "tile-g3"),
                Relation("north",      "tile-h3"));

            // row 5
            actual.ShouldHaveTileWithRelations("tile-a5",
                Relation("north",      "tile-a4"),
                Relation("north-east", "tile-b4"),
                Relation("east",       "tile-b5"),
                Relation("south-east", "tile-b6"),
                Relation("south",      "tile-a6"));

            actual.ShouldHaveTileWithRelations("tile-b5",
                Relation("north",      "tile-b4"),
                Relation("north-east", "tile-c4"),
                Relation("east",       "tile-c5"),
                Relation("south-east", "tile-c6"),
                Relation("south",      "tile-b6"),
                Relation("south-west", "tile-a6"),
                Relation("west",       "tile-a5"),
                Relation("north-west", "tile-a4"));

            actual.ShouldHaveTileWithRelations("tile-c5",
                Relation("north",      "tile-c4"),
                Relation("north-east", "tile-d4"),
                Relation("east",       "tile-d5"),
                Relation("south-east", "tile-d6"),
                Relation("south",      "tile-c6"),
                Relation("south-west", "tile-b6"),
                Relation("west",       "tile-b5"),
                Relation("north-west", "tile-b4"));

            actual.ShouldHaveTileWithRelations("tile-d5",
                Relation("north",      "tile-d4"),
                Relation("north-east", "tile-e4"),
                Relation("east",       "tile-e5"),
                Relation("south-east", "tile-e6"),
                Relation("south",      "tile-d6"),
                Relation("south-west", "tile-c6"),
                Relation("west",       "tile-c5"),
                Relation("north-west", "tile-c4"));

            actual.ShouldHaveTileWithRelations("tile-e5",
                Relation("north",      "tile-e4"),
                Relation("north-east", "tile-f4"),
                Relation("east",       "tile-f5"),
                Relation("south-east", "tile-f6"),
                Relation("south",      "tile-e6"),
                Relation("south-west", "tile-d6"),
                Relation("west",       "tile-d5"),
                Relation("north-west", "tile-d4"));

            actual.ShouldHaveTileWithRelations("tile-f5",
                Relation("north",      "tile-f4"),
                Relation("north-east", "tile-g4"),
                Relation("east",       "tile-g5"),
                Relation("south-east", "tile-g6"),
                Relation("south",      "tile-f6"),
                Relation("south-west", "tile-e6"),
                Relation("west",       "tile-e5"),
                Relation("north-west", "tile-e4"));

            actual.ShouldHaveTileWithRelations("tile-g5",
                Relation("north",      "tile-g4"),
                Relation("north-east", "tile-h4"),
                Relation("east",       "tile-h5"),
                Relation("south-east", "tile-h6"),
                Relation("south",      "tile-g6"),
                Relation("south-west", "tile-f6"),
                Relation("west",       "tile-f5"),
                Relation("north-west", "tile-f4"));

            actual.ShouldHaveTileWithRelations("tile-h5",
                Relation("south",      "tile-h6"),
                Relation("south-west", "tile-g6"),
                Relation("west",       "tile-g5"),
                Relation("north-west", "tile-g4"),
                Relation("north",      "tile-h4"));

            // row 6
            actual.ShouldHaveTileWithRelations("tile-a6",
                Relation("north",      "tile-a5"),
                Relation("north-east", "tile-b5"),
                Relation("east",       "tile-b6"),
                Relation("south-east", "tile-b7"),
                Relation("south",      "tile-a7"));

            actual.ShouldHaveTileWithRelations("tile-b6",
                Relation("north",      "tile-b5"),
                Relation("north-east", "tile-c5"),
                Relation("east",       "tile-c6"),
                Relation("south-east", "tile-c7"),
                Relation("south",      "tile-b7"),
                Relation("south-west", "tile-a7"),
                Relation("west",       "tile-a6"),
                Relation("north-west", "tile-a5"));

            actual.ShouldHaveTileWithRelations("tile-c6",
                Relation("north",      "tile-c5"),
                Relation("north-east", "tile-d5"),
                Relation("east",       "tile-d6"),
                Relation("south-east", "tile-d7"),
                Relation("south",      "tile-c7"),
                Relation("south-west", "tile-b7"),
                Relation("west",       "tile-b6"),
                Relation("north-west", "tile-b5"));

            actual.ShouldHaveTileWithRelations("tile-d6",
                Relation("north",      "tile-d5"),
                Relation("north-east", "tile-e5"),
                Relation("east",       "tile-e6"),
                Relation("south-east", "tile-e7"),
                Relation("south",      "tile-d7"),
                Relation("south-west", "tile-c7"),
                Relation("west",       "tile-c6"),
                Relation("north-west", "tile-c5"));

            actual.ShouldHaveTileWithRelations("tile-e6",
                Relation("north",      "tile-e5"),
                Relation("north-east", "tile-f5"),
                Relation("east",       "tile-f6"),
                Relation("south-east", "tile-f7"),
                Relation("south",      "tile-e7"),
                Relation("south-west", "tile-d7"),
                Relation("west",       "tile-d6"),
                Relation("north-west", "tile-d5"));

            actual.ShouldHaveTileWithRelations("tile-f6",
                Relation("north",      "tile-f5"),
                Relation("north-east", "tile-g5"),
                Relation("east",       "tile-g6"),
                Relation("south-east", "tile-g7"),
                Relation("south",      "tile-f7"),
                Relation("south-west", "tile-e7"),
                Relation("west",       "tile-e6"),
                Relation("north-west", "tile-e5"));

            actual.ShouldHaveTileWithRelations("tile-g6",
                Relation("north",      "tile-g5"),
                Relation("north-east", "tile-h5"),
                Relation("east",       "tile-h6"),
                Relation("south-east", "tile-h7"),
                Relation("south",      "tile-g7"),
                Relation("south-west", "tile-f7"),
                Relation("west",       "tile-f6"),
                Relation("north-west", "tile-f5"));

            actual.ShouldHaveTileWithRelations("tile-h6",
                Relation("south",      "tile-h7"),
                Relation("south-west", "tile-g7"),
                Relation("west",       "tile-g6"),
                Relation("north-west", "tile-g5"),
                Relation("north",      "tile-h5"));

            // row 7
            actual.ShouldHaveTileWithRelations("tile-a7",
                Relation("north",      "tile-a6"),
                Relation("north-east", "tile-b6"),
                Relation("east",       "tile-b7"),
                Relation("south-east", "tile-b8"),
                Relation("south",      "tile-a8"));

            actual.ShouldHaveTileWithRelations("tile-b7",
                Relation("north",      "tile-b6"),
                Relation("north-east", "tile-c6"),
                Relation("east",       "tile-c7"),
                Relation("south-east", "tile-c8"),
                Relation("south",      "tile-b8"),
                Relation("south-west", "tile-a8"),
                Relation("west",       "tile-a7"),
                Relation("north-west", "tile-a6"));

            actual.ShouldHaveTileWithRelations("tile-c7",
                Relation("north",      "tile-c6"),
                Relation("north-east", "tile-d6"),
                Relation("east",       "tile-d7"),
                Relation("south-east", "tile-d8"),
                Relation("south",      "tile-c8"),
                Relation("south-west", "tile-b8"),
                Relation("west",       "tile-b7"),
                Relation("north-west", "tile-b6"));

            actual.ShouldHaveTileWithRelations("tile-d7",
                Relation("north",      "tile-d6"),
                Relation("north-east", "tile-e6"),
                Relation("east",       "tile-e7"),
                Relation("south-east", "tile-e8"),
                Relation("south",      "tile-d8"),
                Relation("south-west", "tile-c8"),
                Relation("west",       "tile-c7"),
                Relation("north-west", "tile-c6"));

            actual.ShouldHaveTileWithRelations("tile-e7",
                Relation("north",      "tile-e6"),
                Relation("north-east", "tile-f6"),
                Relation("east",       "tile-f7"),
                Relation("south-east", "tile-f8"),
                Relation("south",      "tile-e8"),
                Relation("south-west", "tile-d8"),
                Relation("west",       "tile-d7"),
                Relation("north-west", "tile-d6"));

            actual.ShouldHaveTileWithRelations("tile-f7",
                Relation("north",      "tile-f6"),
                Relation("north-east", "tile-g6"),
                Relation("east",       "tile-g7"),
                Relation("south-east", "tile-g8"),
                Relation("south",      "tile-f8"),
                Relation("south-west", "tile-e8"),
                Relation("west",       "tile-e7"),
                Relation("north-west", "tile-e6"));

            actual.ShouldHaveTileWithRelations("tile-g7",
                Relation("north",      "tile-g6"),
                Relation("north-east", "tile-h6"),
                Relation("east",       "tile-h7"),
                Relation("south-east", "tile-h8"),
                Relation("south",      "tile-g8"),
                Relation("south-west", "tile-f8"),
                Relation("west",       "tile-f7"),
                Relation("north-west", "tile-f6"));

            actual.ShouldHaveTileWithRelations("tile-h7",
                Relation("south",      "tile-h8"),
                Relation("south-west", "tile-g8"),
                Relation("west",       "tile-g7"),
                Relation("north-west", "tile-g6"),
                Relation("north",      "tile-h6"));

            // row 8
            actual.ShouldHaveTileWithRelations("tile-a8",
                Relation("north",      "tile-a7"),
                Relation("north-east", "tile-b7"),
                Relation("east",       "tile-b8"));

            actual.ShouldHaveTileWithRelations("tile-b8",
                Relation("north",      "tile-b7"),
                Relation("north-east", "tile-c7"),
                Relation("east",       "tile-c8"),
                Relation("west",       "tile-a8"),
                Relation("north-west", "tile-a7"));

            actual.ShouldHaveTileWithRelations("tile-c8",
                Relation("north",      "tile-c7"),
                Relation("north-east", "tile-d7"),
                Relation("east",       "tile-d8"),
                Relation("west",       "tile-b8"),
                Relation("north-west", "tile-b7"));

            actual.ShouldHaveTileWithRelations("tile-d8",
                Relation("north",      "tile-d7"),
                Relation("north-east", "tile-e7"),
                Relation("east",       "tile-e8"),
                Relation("west",       "tile-c8"),
                Relation("north-west", "tile-c7"));

            actual.ShouldHaveTileWithRelations("tile-e8",
                Relation("north",      "tile-e7"),
                Relation("north-east", "tile-f7"),
                Relation("east",       "tile-f8"),
                Relation("west",       "tile-d8"),
                Relation("north-west", "tile-d7"));

            actual.ShouldHaveTileWithRelations("tile-f8",
                Relation("north",      "tile-f7"),
                Relation("north-east", "tile-g7"),
                Relation("east",       "tile-g8"),
                Relation("west",       "tile-e8"),
                Relation("north-west", "tile-e7"));

            actual.ShouldHaveTileWithRelations("tile-g8",
                Relation("north",      "tile-g7"),
                Relation("north-east", "tile-h7"),
                Relation("east",       "tile-h8"),
                Relation("west",       "tile-f8"),
                Relation("north-west", "tile-f7"));

            actual.ShouldHaveTileWithRelations("tile-h8",
                Relation("west",       "tile-g8"),
                Relation("north-west", "tile-g7"),
                Relation("north",      "tile-h7"));
       }
    }
}