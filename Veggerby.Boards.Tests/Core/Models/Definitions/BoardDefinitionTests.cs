using NUnit.Framework;
using Veggerby.Boards.Tests.Core.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Definitions
{
    public class BoardDefinitionTests
    {
        [TestFixture]
        public class GetTile
        {
            [Test]
            public void Should_return_tile_definition_that_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTile("tile1");

                // assert
                Assert.AreSame("tile1", actual.TileId);
            }

            [Test]
            public void Should_return_null_for_tile_definition_that_does_not_exist()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTile("bogus");

                // assert
                Assert.IsNull(actual);
            }
        }

        [TestFixture]
        public class GetTileRelation
        {
            [Test]
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
                Assert.AreSame(tile1, actual.SourceTile);
                Assert.AreSame(tile2, actual.DestinationTile);
            }

            [Test]
            public void Should_return_null_for_tile_definition_that_does_not_exist_no_tile_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelation("foo", "bar");

                // assert
                Assert.IsNull(actual);
            }

            [Test]
            public void Should_return_null_for_tile_definition_that_does_not_exist_source_tile_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelation("tile1", "bar");

                // assert
                Assert.IsNull(actual);
            }


            [Test]
            public void Should_return_null_for_tile_definition_that_does_not_exist_destination_tile_exists()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelation("tile1", "bar");

                // assert
                Assert.IsNull(actual);
            }
        }
    }
}
