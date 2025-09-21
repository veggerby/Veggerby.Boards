using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations;

public class TileRelationTests
{
    public class Create
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
            actual.From.Should().Be(from);
            actual.To.Should().Be(to);
            actual.Direction.Should().Be(Direction.Clockwise);
            actual.Distance.Should().Be(1);
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
            actual.From.Should().Be(from);
            actual.To.Should().Be(to);
            actual.Direction.Should().Be(Direction.CounterClockwise);
            actual.Distance.Should().Be(5);
        }

        [Fact]
        public void Should_throw_null_from()
        {
            var to = new Tile("tile-2");

            // act
            var actual = () => new TileRelation(null, to, Direction.CounterClockwise, 5);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("from");
        }

        [Fact]
        public void Should_throw_null_to()
        {
            var from = new Tile("tile-1");

            // act
            var actual = () => new TileRelation(from, null, Direction.CounterClockwise, 5);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("to");
        }

        [Fact]
        public void Should_throw_null_direction()
        {
            var from = new Tile("tile-1");
            var to = new Tile("tile-2");

            // act
            var actual = () => new TileRelation(from, to, null, 5);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("direction");
        }

        [Fact]
        public void Should_throw_zero_distance()
        {
            var from = new Tile("tile-1");
            var to = new Tile("tile-2");

            // act
            var actual = () => new TileRelation(from, to, Direction.Clockwise, 0);

            // assert
            actual.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("distance");
        }

        [Fact]
        public void Should_throw_negative_distance()
        {
            var from = new Tile("tile-1");
            var to = new Tile("tile-2");

            // act
            var actual = () => new TileRelation(from, to, Direction.Clockwise, -1);

            // assert
            actual.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("distance");
        }
    }

    public class _ToString
    {
        [Fact]
        public void Should_return_expected_string()
        {
            // arrange
            var from = new Tile("tile-1");
            var to = new Tile("tile-2");
            var relation = new TileRelation(from, to, Direction.Clockwise);

            // act
            var actual = relation.ToString();

            // assert
            actual.Should().Be("TileRelation Tile tile-1 1xDirection clockwise Tile tile-2");
        }
    }
}