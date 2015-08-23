using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Builders;

namespace Veggerby.Boards.Tests.Core.Builders
{
    public class BackgammonBoardDefinitionBuilderTests
    {
        [TestFixture]
        public class Compile
        {
            [Test]
            public void Should_return_valid_backgammon_board_definition()
            {
                // arrange
                var builder = new BackgammonBoardDefinitionBuilder();

                // act
                var actual = builder.Compile();

                // assert
                Assert.AreEqual("backgammon", actual.BoardId);

                for (int i = 1; i <= 24; i++)
                {
                    var tile = actual.GetTile($"point-{i}");
                    var clockwise = actual.GetTileRelation($"point-{i}", $"point-{i + 1}");
                    var counterclockwise = actual.GetTileRelation($"point-{i}", $"point-{i - 1}");
                    Assert.IsNotNull(tile);

                    if (i < 24)
                    {
                        Assert.IsNotNull(clockwise);
                        Assert.AreSame(tile, clockwise.SourceTile);
                        Assert.AreEqual($"point-{i + 1}", clockwise.DestinationTile.TileId);
                        Assert.AreEqual("clockwise", clockwise.Direction.DirectionId);
                        Assert.AreEqual(1, clockwise.Distance);
                    }
                    else
                    {
                        Assert.IsNull(clockwise);
                    }

                    if (i > 1)
                    {
                        Assert.IsNotNull(counterclockwise);
                        Assert.AreSame(tile, counterclockwise.SourceTile);
                        Assert.AreEqual($"point-{i - 1}", counterclockwise.DestinationTile.TileId);
                        Assert.AreEqual("counterclockwise", counterclockwise.Direction.DirectionId);
                        Assert.AreEqual(1, counterclockwise.Distance);
                    }
                    else
                    {
                        Assert.IsNull(counterclockwise);
                    }
                }

                Assert.IsNotNull(actual.GetTile("bar"));
                Assert.IsNotNull(actual.GetTile("home-white"));
                Assert.IsNotNull(actual.GetTile("home-black"));
            }
        }
    }
}
