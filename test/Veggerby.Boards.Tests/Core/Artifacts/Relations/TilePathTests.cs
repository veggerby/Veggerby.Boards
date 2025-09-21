using System;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations;

public class TilePathTests
{
    public class Constructor
    {
        [Fact]
        public void Should_create_tilepath()
        {
            // arrange
            var board = new TestBoard();
            var tile1 = board.GetTile("tile-1");
            var tile2 = board.GetTile("tile-2");
            var tile3 = board.GetTile("tile-3");
            var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
            var relation2to3 = board.GetTileRelation(tile2, Direction.Clockwise);

            // act
            var actual = new TilePath([relation1to2, relation2to3]);

            // assert
            actual.Relations.Should().Equal([relation1to2, relation2to3]);
            actual.Tiles.Should().Equal([tile1, tile2, tile3]);
            actual.Directions.Should().Equal([Direction.Clockwise, Direction.Clockwise]);
            actual.From.Should().Be(tile1);
            actual.To.Should().Be(tile3);
            actual.Distance.Should().Be(6);
        }

        [Fact]
        public void Should_throw_null_relations()
        {
            // arrange
            // act
            var actual = () => new TilePath(null);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("relations");
        }

        [Fact]
        public void Should_throw_empty_relations()
        {
            // arrange
            // act
            var actual = () => new TilePath(Enumerable.Empty<TileRelation>());

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("relations");
        }

        [Fact]
        public void Should_throw_null_relation_in_list()
        {
            // arrange
            // act
            var actual = () => new TilePath([null]);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("relations");
        }

        [Fact]
        public void Should_throw_relations_are_not_connected()
        {
            // arrange
            var board = new TestBoard();
            var tile1 = board.GetTile("tile-1");
            var tile2 = board.GetTile("tile-2");
            var tile3 = board.GetTile("tile-3");
            var tile4 = board.GetTile("tile-4");
            var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
            var relation3to4 = board.GetTileRelation(tile3, Direction.Clockwise);

            // act
            var actual = () => new TilePath([relation1to2, relation3to4]);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("relations");
        }
    }

    public class _ToString
    {
        [Fact]
        public void Should_return_expected()
        {
            // arrange
            var board = new TestBoard();
            var tile1 = board.GetTile("tile-1");
            var tile2 = board.GetTile("tile-2");
            var tile3 = board.GetTile("tile-3");
            var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
            var relation2to3 = board.GetTileRelation(tile2, Direction.Clockwise);
            var path = new TilePath([relation1to2, relation2to3]);

            // act
            var actual = path.ToString();

            // assert
            actual.Should().Be("Path: Tile tile-1 Direction clockwise Tile tile-2 Direction clockwise Tile tile-3");
        }
    }


    public class Add
    {
        [Fact]
        public void Should_add_step_to_tilepath()
        {
            // arrange
            var board = new TestBoard();
            var tile1 = board.GetTile("tile-1");
            var tile2 = board.GetTile("tile-2");
            var tile3 = board.GetTile("tile-3");
            var tile4 = board.GetTile("tile-4");
            var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
            var relation2to3 = board.GetTileRelation(tile2, Direction.Clockwise);
            var relation3to4 = board.GetTileRelation(tile3, Direction.Clockwise);
            var path = new TilePath([relation1to2, relation2to3]);

            // act
            var actual = path.Add(relation3to4);

            // assert
            actual.Relations.Should().Equal([relation1to2, relation2to3, relation3to4]);
            actual.Tiles.Should().Equal([tile1, tile2, tile3, tile4]);
            actual.Directions.Should().Equal([Direction.Clockwise, Direction.Clockwise, Direction.Clockwise]);
            actual.From.Should().Be(tile1);
            actual.To.Should().Be(tile4);
            actual.Distance.Should().Be(9);
        }
    }

    public class Create
    {
        [Fact]
        public void Should_create_tilepath_from_nothing()
        {
            // arrange
            var board = new TestBoard();
            var tile1 = board.GetTile("tile-1");
            var tile2 = board.GetTile("tile-2");
            var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);

            // act
            var actual = TilePath.Create(null, relation1to2);

            // assert
            actual.Relations.Should().Equal([relation1to2]);
            actual.Tiles.Should().Equal([tile1, tile2]);
            actual.Directions.Should().Equal([Direction.Clockwise]);
            actual.From.Should().Be(tile1);
            actual.To.Should().Be(tile2);
            actual.Distance.Should().Be(3);
        }

        [Fact]
        public void Should_create_tilepath_from_existing()
        {
            // arrange
            var board = new TestBoard();
            var tile1 = board.GetTile("tile-1");
            var tile2 = board.GetTile("tile-2");
            var tile3 = board.GetTile("tile-3");
            var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
            var relation2to3 = board.GetTileRelation(tile2, Direction.Clockwise);
            var path = new TilePath([relation1to2]);

            // act
            var actual = TilePath.Create(path, relation2to3);

            // assert
            actual.Relations.Should().Equal([relation1to2, relation2to3]);
            actual.Tiles.Should().Equal([tile1, tile2, tile3]);
            actual.Directions.Should().Equal([Direction.Clockwise, Direction.Clockwise]);
            actual.From.Should().Be(tile1);
            actual.To.Should().Be(tile3);
            actual.Distance.Should().Be(6);
        }
    }
}