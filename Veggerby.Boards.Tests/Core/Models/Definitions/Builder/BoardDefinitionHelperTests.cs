using System.Linq;
using NUnit.Framework;

namespace Veggerby.Boards.Tests.Core.Models.Definitions.Builder
{
    public class BoardDefinitionBuilderTests
    {
        [TestFixture]
        public class Compile
        {
            [Test]
            public void Should_return_correct_tile_definitions()
            {
                // arrange

                // act
                // should be 1->2->3 - clockwise
                // and 3->2->1 - counter clock wise

                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // assert
                Assert.AreEqual("board", boardDefinition.BoardId);

                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var foo = boardDefinition.GetTile("foo");

                Assert.AreEqual("tile1", tile1.TileId);
                Assert.AreEqual("tile2", tile2.TileId);
                Assert.AreEqual("tile3", tile3.TileId);
                Assert.IsNull(foo);

                var relation12 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation23 = boardDefinition.GetTileRelation("tile2", "tile3");
                var relation32 = boardDefinition.GetTileRelation("tile3", "tile2");
                var relation21 = boardDefinition.GetTileRelation("tile2", "tile1");
                var bar = boardDefinition.GetTileRelation("foo", "bar");

                // 1-2
                Assert.AreSame(tile1, relation12.SourceTile);
                Assert.AreSame(tile2, relation12.DestinationTile);
                Assert.AreEqual("clockWise", relation12.Direction.DirectionId);
                Assert.AreEqual(1, relation12.Distance);

                // 2-3
                Assert.AreSame(tile2, relation23.SourceTile);
                Assert.AreSame(tile3, relation23.DestinationTile);
                Assert.AreEqual("clockWise", relation23.Direction.DirectionId);
                Assert.AreEqual(1, relation23.Distance);

                // 3-2
                Assert.AreSame(tile3, relation32.SourceTile);
                Assert.AreSame(tile2, relation32.DestinationTile);
                Assert.AreEqual("counterClockWise", relation32.Direction.DirectionId);
                Assert.AreEqual(2, relation32.Distance);

                // 2-1
                Assert.AreSame(tile2, relation21.SourceTile);
                Assert.AreSame(tile1, relation21.DestinationTile);
                Assert.AreEqual("counterClockWise", relation21.Direction.DirectionId);
                Assert.AreEqual(2, relation21.Distance);
            }
        }
    }
}
