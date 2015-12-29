using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Models.Navigation;
using Veggerby.Boards.Tests.Core.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Navigation
{
    public class TilePathTests
    {
        public class Get_SourceAndDestinationTileDefinition
        {
            [Fact]
            public void Should_return_tile_definitions_single_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");

                // act
                var tilePath = new TilePath(tile1, tile2, new[] {relation1To2});

                // assert
                Assert.Same(tile1, tilePath.SourceTileDefinition);
                Assert.Same(tile2, tilePath.DestinationTileDefinition);
            }

            [Fact]
            public void Should_return_tile_definitions_two_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile3 = boardDefinition.GetTile("tile3");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation2To3 = boardDefinition.GetTileRelation("tile2", "tile3");

                // act
                var tilePath = new TilePath(tile1, tile3, new[] {relation1To2, relation2To3});

                // assert
                Assert.Same(tile1, tilePath.SourceTileDefinition);
                Assert.Same(tile3, tilePath.DestinationTileDefinition);
            }

            [Fact]
            public void Should_return_tile_definitions_no_step_path_empty_array()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");

                // act
                var tilePath = new TilePath(tile1, tile1, Enumerable.Empty<TileRelationDefinition>().ToArray());

                // assert
                Assert.Same(tile1, tilePath.SourceTileDefinition);
                Assert.Same(tile1, tilePath.DestinationTileDefinition);
            }

            [Fact]
            public void Should_return_tile_definitions_no_step_path_null_array()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");

                // act
                var tilePath = new TilePath(tile1, tile1, null);

                // assert
                Assert.Same(tile1, tilePath.SourceTileDefinition);
                Assert.Same(tile1, tilePath.DestinationTileDefinition);
            }


            [Fact]
            public async Task Should_throw_application_exception_when_source_of_relations_is_not_the_one_specified()
            {
                await Assert.ThrowsAsync<TileException>(
                    Method_that_should_throw_application_exception_when_source_of_relations_is_not_the_one_specified);
            }

            public Task Method_that_should_throw_application_exception_when_source_of_relations_is_not_the_one_specified()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation2To3 = boardDefinition.GetTileRelation("tile2", "tile3");

                // act
                var tilePath = new TilePath(tile2, tile3, new[] {relation1To2, relation2To3});
                
                return Task.FromResult(0);
            }

            [Fact]
            public async Task Should_throw_application_exception_when_destination_of_relations_is_not_the_one_specified()
            {
                await Assert.ThrowsAsync<TileException>(
                    Method_that_should_throw_application_exception_when_destination_of_relations_is_not_the_one_specified);
            }

            public Task
                Method_that_should_throw_application_exception_when_destination_of_relations_is_not_the_one_specified()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation2To3 = boardDefinition.GetTileRelation("tile2", "tile3");

                // act
                var tilePath = new TilePath(tile1, tile2, new[] {relation1To2, relation2To3});
                
                return Task.FromResult(0);                
            }
        }

        public class HasPassedOver
        {
            [Fact]
            public void Should_return_tile_definitions_single_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var tilePath = new TilePath(tile1, tile2, new[] {relation1To2});

                // act
                var actual1 = tilePath.HasPassedOver(tile1);
                var actual2 = tilePath.HasPassedOver(tile2);
                var actual3 = tilePath.HasPassedOver(tile3);

                // assert
                Assert.True(actual1);
                Assert.True(actual2);
                Assert.False(actual3);
            }

            [Fact]
            public void Should_return_tile_definitions_two_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation2To3 = boardDefinition.GetTileRelation("tile2", "tile3");
                var tilePath = new TilePath(tile1, tile3, new[] {relation1To2, relation2To3});

                // act
                var actual1 = tilePath.HasPassedOver(tile1);
                var actual2 = tilePath.HasPassedOver(tile2);
                var actual3 = tilePath.HasPassedOver(tile3);

                // assert
                Assert.True(actual1);
                Assert.True(actual2);
                Assert.True(actual3);
            }

            [Fact]
            public void Should_return_tile_definitions_no_step_path_empty_array()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var tilePath = new TilePath(tile1, tile1, Enumerable.Empty<TileRelationDefinition>().ToArray());

                // act
                var actual1 = tilePath.HasPassedOver(tile1);
                var actual2 = tilePath.HasPassedOver(tile2);
                var actual3 = tilePath.HasPassedOver(tile3);

                // assert
                Assert.True(actual1);
                Assert.False(actual2);
                Assert.False(actual3);
            }
        }

        public class GetTileDefinitions
        {
            [Fact]
            public void Should_return_tile_definitions_single_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var tilePath = new TilePath(tile1, tile2, new[] { relation1To2 });

                // act
                var actual = tilePath.GetTileDefinitions();

                // assert
                Assert.Equal(2, actual.Count());
                Assert.Same(tile1, actual.First());
                Assert.Same(tile2, actual.Last());
            }

            [Fact]
            public void Should_return_tile_definitions_two_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation2To3 = boardDefinition.GetTileRelation("tile2", "tile3");
                var tilePath = new TilePath(tile1, tile3, new[] { relation1To2, relation2To3 });

                // act
                var actual = tilePath.GetTileDefinitions();

                // assert
                Assert.Equal(3, actual.Count());
                Assert.Same(tile1, actual.First());
                Assert.Same(tile2, actual.Skip(1).First());
                Assert.Same(tile3, actual.Skip(2).First());
            }

            [Fact]
            public void Should_return_tile_definitions_no_step_path_empty_array()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tilePath = new TilePath(tile1, tile1, Enumerable.Empty<TileRelationDefinition>().ToArray());

                // act
                var actual = tilePath.GetTileDefinitions();

                // assert
                Assert.Equal(1, actual.Count());
                Assert.Same(tile1, actual.Single());
            }
        }

        public class GetDistance
        {
            [Fact]
            public void Should_return_distance_single_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var tilePath = new TilePath(tile1, tile2, new[] { relation1To2 });

                // act
                var actual = tilePath.GetDistance();

                // assert
                Assert.Equal(1, actual);
            }

            [Fact]
            public void Should_return_distance_two_step_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile3 = boardDefinition.GetTile("tile3");
                var relation1To2 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation2To3 = boardDefinition.GetTileRelation("tile2", "tile3");
                var tilePath = new TilePath(tile1, tile3, new[] { relation1To2, relation2To3 });

                // act
                var actual = tilePath.GetDistance();

                // assert
                Assert.Equal(2, actual);
            }

            [Fact]
            public void Should_return_distance_no_step_path_empty_array()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tilePath = new TilePath(tile1, tile1, Enumerable.Empty<TileRelationDefinition>().ToArray());

                // act
                var actual = tilePath.GetDistance();

                // assert
                Assert.Equal(0, actual);
            }

            [Fact]
            public void Should_return_distance_two_step_path_with_non_default_distance()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                // explicitly load source and destination to avoid potential issues if source and definitions come from "a different place"
                var tile1 = boardDefinition.GetTile("tile1");
                var tile3 = boardDefinition.GetTile("tile3");
                var relation3To2 = boardDefinition.GetTileRelation("tile3", "tile2");
                var relation2To1 = boardDefinition.GetTileRelation("tile2", "tile1");
                var tilePath = new TilePath(tile3, tile1, new[] { relation3To2, relation2To1 });

                // act
                var actual = tilePath.GetDistance();

                // assert
                Assert.Equal(4, actual);
            }

        }
    }
}