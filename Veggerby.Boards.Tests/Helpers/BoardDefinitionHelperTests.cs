using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Veggerby.Boards.Tests.Helpers
{
    public class BoardDefinitionHelperTests
    {
        [TestFixture]
        public class GetBoardDefinition
        {
            [Test]
            public void Should_return_correct_tile_definitions()
            {
                // arrange

                // act
                // should be 1->2->3 - clockwise
                // and 3->2->1 - counter clock wise

                var boardDefinition = BoardDefinitionHelper.GetBoardDefinition();

                // assert
                Assert.AreEqual("board", boardDefinition.BoardId);
                Assert.AreEqual(3, boardDefinition.Tiles.Count());

                var tile1 = boardDefinition.Tiles.First();
                var tile2 = boardDefinition.Tiles.Skip(1).First();
                var tile3 = boardDefinition.Tiles.Skip(2).First();

                Assert.AreEqual("tile1", tile1.TileId);
                Assert.AreEqual("tile2", tile2.TileId);
                Assert.AreEqual("tile3", tile3.TileId);

                Assert.AreEqual(1, tile1.RelationsDefinition.Count());
                Assert.AreEqual(2, tile2.RelationsDefinition.Count());
                Assert.AreEqual(1, tile3.RelationsDefinition.Count());

                Assert.AreEqual("clockWise", tile1.RelationsDefinition.First().Direction.DirectionId);
                Assert.AreEqual("clockWise", tile2.RelationsDefinition.First().Direction.DirectionId);
                Assert.AreEqual("counterClockWise", tile2.RelationsDefinition.Last().Direction.DirectionId);
                Assert.AreEqual("counterClockWise", tile3.RelationsDefinition.First().Direction.DirectionId);

                Assert.AreSame(tile1, tile1.RelationsDefinition.First().SourceTile); // 1 -> 2
                Assert.AreSame(tile2, tile2.RelationsDefinition.First().SourceTile); // 2 -> 3
                Assert.AreSame(tile2, tile2.RelationsDefinition.Last().SourceTile);  // 2 -> 1
                Assert.AreSame(tile3, tile3.RelationsDefinition.First().SourceTile); // 3 -> 2

                Assert.AreSame(tile2, tile1.RelationsDefinition.First().DestinationTile); // 1 -> 2
                Assert.AreSame(tile3, tile2.RelationsDefinition.First().DestinationTile); // 2 -> 3
                Assert.AreSame(tile1, tile2.RelationsDefinition.Last().DestinationTile);  // 2 -> 1
                Assert.AreSame(tile2, tile3.RelationsDefinition.First().DestinationTile); // 3 -> 2

            }
        }
    }
}
