using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Models.Navigation;
using Veggerby.Boards.Tests.Core.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Navigation
{
    public class TilePathTests
    {
        [TestFixture]
        public class Get_SourceAndDestinationTileDefinition
        {
            [Test]
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
                Assert.AreSame(tile1, tilePath.SourceTileDefinition);
                Assert.AreSame(tile2, tilePath.DestinationTileDefinition);
            }

            [Test]
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
                Assert.AreSame(tile1, tilePath.SourceTileDefinition);
                Assert.AreSame(tile3, tilePath.DestinationTileDefinition);
            }

            [Test]
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
                Assert.AreSame(tile1, tilePath.SourceTileDefinition);
                Assert.AreSame(tile1, tilePath.DestinationTileDefinition);
            }

            [Test]
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
                Assert.AreSame(tile1, tilePath.SourceTileDefinition);
                Assert.AreSame(tile1, tilePath.DestinationTileDefinition);
            }


            [Test]
            public void Should_throw_application_exception_when_source_of_relations_is_not_the_one_specified()
            {
                Assert.Throws<ApplicationException>(
                    Method_that_should_throw_application_exception_when_source_of_relations_is_not_the_one_specified);
            }

            public void Method_that_should_throw_application_exception_when_source_of_relations_is_not_the_one_specified
                ()
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
            }

            [Test]
            public void Should_throw_application_exception_when_destination_of_relations_is_not_the_one_specified()
            {
                Assert.Throws<ApplicationException>(
                    Method_that_should_throw_application_exception_when_destination_of_relations_is_not_the_one_specified);
            }

            public void
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
            }
        }

        [TestFixture]
        public class HasPassedOver
        {
            [Test]
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
                Assert.IsTrue(actual1);
                Assert.IsTrue(actual2);
                Assert.IsFalse(actual3);
            }

            [Test]
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
                Assert.IsTrue(actual1);
                Assert.IsTrue(actual2);
                Assert.IsTrue(actual3);
            }

            [Test]
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
                Assert.IsTrue(actual1);
                Assert.IsFalse(actual2);
                Assert.IsFalse(actual3);
            }
        }

        [TestFixture]
        public class GetTileDefinitions
        {
            [Test]
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
                Assert.AreEqual(2, actual.Count());
                Assert.AreSame(tile1, actual.First());
                Assert.AreSame(tile2, actual.Last());
            }

            [Test]
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
                Assert.AreEqual(3, actual.Count());
                Assert.AreSame(tile1, actual.First());
                Assert.AreSame(tile2, actual.Skip(1).First());
                Assert.AreSame(tile3, actual.Skip(2).First());
            }

            [Test]
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
                Assert.AreEqual(1, actual.Count());
                Assert.AreSame(tile1, actual.Single());
            }
        }

        [TestFixture]
        public class GetDistance
        {
            [Test]
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
                Assert.AreEqual(1, actual);
            }

            [Test]
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
                Assert.AreEqual(2, actual);
            }

            [Test]
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
                Assert.AreEqual(0, actual);
            }

            [Test]
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
                Assert.AreEqual(4, actual);
            }

        }
    }
}