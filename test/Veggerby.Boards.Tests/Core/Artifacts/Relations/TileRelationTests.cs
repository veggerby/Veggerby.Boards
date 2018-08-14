using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
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
                actual.From.ShouldBe(from);
                actual.To.ShouldBe(to);
                actual.Direction.ShouldBe(Direction.Clockwise);
                actual.Distance.ShouldBe(1);
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
                actual.From.ShouldBe(from);
                actual.To.ShouldBe(to);
                actual.Direction.ShouldBe(Direction.CounterClockwise);
                actual.Distance.ShouldBe(5);
            }

            [Fact]
            public void Should_throw_null_from()
            {
                var to = new Tile("tile-2");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new TileRelation(null, to, Direction.CounterClockwise, 5));

                // assert
                actual.ParamName.ShouldBe("from");
            }

            [Fact]
            public void Should_throw_null_to()
            {
                var from = new Tile("tile-1");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new TileRelation(from, null, Direction.CounterClockwise, 5));

                // assert
                actual.ParamName.ShouldBe("to");
            }

            [Fact]
            public void Should_throw_null_direction()
            {
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new TileRelation(from, to, null, 5));

                // assert
                actual.ParamName.ShouldBe("direction");
            }

            [Fact]
            public void Should_throw_zero_distance()
            {
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => new TileRelation(from, to, Direction.Clockwise, 0));

                // assert
                actual.ParamName.ShouldBe("distance");
            }

            [Fact]
            public void Should_throw_negative_distance()
            {
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => new TileRelation(from, to, Direction.Clockwise, -1));

                // assert
                actual.ParamName.ShouldBe("distance");
            }
        }
    }
}