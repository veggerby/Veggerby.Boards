using System.Linq;
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

        [TestFixture]
        public class GetTileRelationsFromSource
        {
            [Test]
            public void Should_return_valid_tile_relations()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelationsFromSource("tile2");

                // assert
                Assert.AreEqual(2, actual.Count());
                Assert.IsTrue(actual.Any(x => x.DestinationTile.TileId == "tile1"), "No tile with destination tile1 found");
                Assert.IsTrue(actual.Any(x => x.DestinationTile.TileId == "tile3"), "No tile with destination tile2 found");
            }

            [Test]
            public void Should_return_empty_tile_relations()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // act
                var actual = boardDefinition.GetTileRelationsFromSource("bar");

                // assert
                Assert.IsEmpty(actual);
            }
        }
    }
}
