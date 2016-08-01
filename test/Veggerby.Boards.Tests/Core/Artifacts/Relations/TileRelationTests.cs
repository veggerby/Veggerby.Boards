using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations
{
    public class TileRelationTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = new TileRelation(from, to, Direction.Clockwise);

                // assert
                Assert.Equal(from, actual.From);
                Assert.Equal(to, actual.To);
                Assert.Equal(Direction.Clockwise, actual.Direction);
                Assert.Equal(1, actual.Distance);
            }

            [Fact]
            public void Should_initialize_from_constructor_explicit_distance()
            {
                // arrange
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = new TileRelation(from, to, Direction.CounterClockwise, 5);

                // assert
                Assert.Equal(from, actual.From);
                Assert.Equal(to, actual.To);
                Assert.Equal(Direction.CounterClockwise, actual.Direction);
                Assert.Equal(5, actual.Distance);
            }

            [Fact]
            public void Should_throw_null_from()
            {
                var to = new Tile("tile-2");

                // act
                var actual = Assert.Throws<ArgumentNullException>(() => new TileRelation(null, to, Direction.CounterClockwise, 5));

                // assert
                Assert.Equal("from", actual.ParamName);
            }

            [Fact]
            public void Should_throw_null_to()
            {
                var from = new Tile("tile-1");

                // act
                var actual = Assert.Throws<ArgumentNullException>(() => new TileRelation(from, null, Direction.CounterClockwise, 5));

                // assert
                Assert.Equal("to", actual.ParamName);
            }

            [Fact]
            public void Should_throw_null_direction()
            {
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = Assert.Throws<ArgumentNullException>(() => new TileRelation(from, to, null, 5));

                // assert
                Assert.Equal("direction", actual.ParamName);
            }

            [Fact]
            public void Should_throw_zero_distance()
            {
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = Assert.Throws<ArgumentOutOfRangeException>(() => new TileRelation(from, to, Direction.Clockwise, 0));

                // assert
                Assert.Equal("distance", actual.ParamName);
            }

            [Fact]
            public void Should_throw_negative_distance()
            {
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = Assert.Throws<ArgumentOutOfRangeException>(() => new TileRelation(from, to, Direction.Clockwise, -1));

                // assert
                Assert.Equal("distance", actual.ParamName);
            }
        }
    }
}