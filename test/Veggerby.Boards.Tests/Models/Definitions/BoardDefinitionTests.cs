using System.Linq;
using Xunit;
using Veggerby.Boards.Tests.Core.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Definitions
{
    public class BoardDefinitionTests
    {
        public class GetTile
        {
            [Fact]
            public void Should_return_tile_definition_that_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTile("tile1");

                // assert
                Assert.Same("tile1", actual.TileId);
            }

            [Fact]
            public void Should_return_null_for_tile_definition_that_does_not_exist()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTile("bogus");

                // assert
                Assert.Null(actual);
            }
        }

        public class GetTileRelation
        {
            [Fact]
            public void Should_return_tile_relation_that_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var actual = boardDefinition.GetTileRelation("tile1", "tile2");

                // assert
                Assert.Same(tile1, actual.SourceTile);
                Assert.Same(tile2, actual.DestinationTile);
            }

            [Fact]
            public void Should_return_null_for_tile_definition_that_does_not_exist_no_tile_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelation("foo", "bar");

                // assert
                Assert.Null(actual);
            }

            [Fact]
            public void Should_return_null_for_tile_definition_that_does_not_exist_source_tile_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelation("tile1", "bar");

                // assert
                Assert.Null(actual);
            }


            [Fact]
            public void Should_return_null_for_tile_definition_that_does_not_exist_destination_tile_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelation("tile1", "bar");

                // assert
                Assert.Null(actual);
            }
        }

        public class GetTileRelationsFromSource
        {
            [Fact]
            public void Should_return_valid_tile_relations()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelationsFromSource("tile2");

                // assert
                Assert.Equal(2, actual.Count());
                Assert.True(actual.Any(x => x.DestinationTile.TileId == "tile1"), "No tile with destination tile1 found");
                Assert.True(actual.Any(x => x.DestinationTile.TileId == "tile3"), "No tile with destination tile2 found");
            }

            [Fact]
            public void Should_return_empty_tile_relations()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelationsFromSource("bar");

                // assert
                Assert.Empty(actual);
            }
        }

        public class GetPieceDefinition
        {
            [Fact]
            public void Should_return_valid_piece()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetPiece("white");

                // assert
                Assert.Equal("white", actual.PieceId);
            }

            [Fact]
            public void Should_not_return_invalid_piece()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetPiece("bogus");

                // assert
                Assert.Null(actual);
            }
        }
    }
}
