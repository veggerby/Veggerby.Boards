using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Veggerby.Boards.Core.Contracts.Builders;

namespace Veggerby.Boards.Tests.Core.Builders
{
    public class BackgammonBoardDefinitionBuilderTests
    {
        public class Compile
        {
            [Fact]
            public void Should_return_valid_backgammon_board_definition()
            {
                // arrange
                var builder = new BackgammonBoardDefinitionBuilder();

                // act
                var actual = builder.Compile();

                // assert
                Assert.Equal("backgammon", actual.BoardId);

                for (int i = 1; i <= 24; i++)
                {
                    var tile = actual.GetTile($"point-{i}");
                    var clockwise = actual.GetTileRelation($"point-{i}", $"point-{i + 1}");
                    var counterclockwise = actual.GetTileRelation($"point-{i}", $"point-{i - 1}");
                    Assert.NotNull(tile);

                    if (i < 24)
                    {
                        Assert.NotNull(clockwise);
                        Assert.Same(tile, clockwise.SourceTile);
                        Assert.Equal($"point-{i + 1}", clockwise.DestinationTile.TileId);
                        Assert.Equal("clockwise", clockwise.Direction.DirectionId);
                        Assert.Equal(1, clockwise.Distance);
                    }
                    else
                    {
                        Assert.Null(clockwise);
                    }

                    if (i > 1)
                    {
                        Assert.NotNull(counterclockwise);
                        Assert.Same(tile, counterclockwise.SourceTile);
                        Assert.Equal($"point-{i - 1}", counterclockwise.DestinationTile.TileId);
                        Assert.Equal("counterclockwise", counterclockwise.Direction.DirectionId);
                        Assert.Equal(1, counterclockwise.Distance);
                    }
                    else
                    {
                        Assert.Null(counterclockwise);
                    }
                }

                Assert.NotNull(actual.GetTile("bar"));
                Assert.NotNull(actual.GetTile("home-white"));
                Assert.NotNull(actual.GetTile("home-black"));

                Assert.NotNull(actual.GetPiece("black"));
                Assert.NotNull(actual.GetPiece("white"));
            }
        }
    }
}
