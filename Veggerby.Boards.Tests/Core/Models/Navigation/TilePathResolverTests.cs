using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Models.Navigation;
using Veggerby.Boards.Tests.Core.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Navigation
{
    public class TilePathResolverTests
    {
        [TestFixture]
        public class ResolvePaths
        {
            [Test]
            public void Should_return_path_with_self_when_source_and_target_are_identical()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tile1 = boardDefinition.GetTile("tile1");
                var tilepathResolver = new TilePathResolver();
                
                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "tile1", "tile1");

                // assert
                Assert.AreEqual(1, paths.Count());
                var path = paths.Single();
                Assert.AreSame(tile1, path.SourceTileDefinition);
                Assert.AreSame(tile1, path.DestinationTileDefinition);
                Assert.AreEqual(1, path.GetTileDefinitions().Count());
                Assert.AreSame(tile1, path.GetTileDefinitions().Single());
            }

            [Test]
            public void Should_return_path_with_one_steps()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "tile1", "tile2");

                // assert
                Assert.AreEqual(1, paths.Count());
                var path = paths.Single();
                Assert.AreSame(tile1, path.SourceTileDefinition);
                Assert.AreSame(tile2, path.DestinationTileDefinition);
                Assert.AreEqual(2, path.GetTileDefinitions().Count());
                Assert.AreSame(tile1, path.GetTileDefinitions().First());
                Assert.AreSame(tile2, path.GetTileDefinitions().Last());
            }

            [Test]
            public void Should_return_path_with_two_steps()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "tile1", "tile3");

                // assert
                Assert.AreEqual(1, paths.Count());
                var path = paths.Single();
                Assert.AreSame(tile1, path.SourceTileDefinition);
                Assert.AreSame(tile3, path.DestinationTileDefinition);
                Assert.AreEqual(3, path.GetTileDefinitions().Count());
                Assert.AreSame(tile1, path.GetTileDefinitions().First());
                Assert.AreSame(tile2, path.GetTileDefinitions().Skip(1).First());
                Assert.AreSame(tile3, path.GetTileDefinitions().Skip(2).First());
            }

            [Test]
            public void Should_return_no_path()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tile1 = boardDefinition.GetTile("tile1");
                var bar = boardDefinition.GetTile("bar");
                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "tile1", "bar");

                // assert
                Assert.IsEmpty(paths);
            }

            [Test]
            public void Should_return_two_paths()
            {
                // arrange
                var builder = new CircularBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "tile1", "tile2");

                // assert
                Assert.AreEqual(2, paths.Count());

                var path1 = paths.First();
                var path2 = paths.Last();

                Assert.AreEqual(2, path1.GetTileDefinitions().Count());
                Assert.AreEqual(tile1, path1.GetTileDefinitions().First());
                Assert.AreEqual(tile2, path1.GetTileDefinitions().Last());

                Assert.AreEqual(3, path2.GetTileDefinitions().Count());
                Assert.AreEqual(tile1, path2.GetTileDefinitions().First());
                Assert.AreEqual(tile3, path2.GetTileDefinitions().Skip(1).First());
                Assert.AreEqual(tile2, path2.GetTileDefinitions().Skip(2).First());
            }

            [Test]
            public void Should_throw_exception_with_invalid_sourceTileId()
            {
                // assert
                Assert.Throws<ApplicationException>(Method_should_throw_exception_with_invalid_sourceTileId, "source tile is not valid");
            }

            public void Method_should_throw_exception_with_invalid_sourceTileId()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "bogus", "bar");
            }

            [Test]
            public void Should_throw_exception_with_invalid_destinationTileId()
            {
                // assert
                Assert.Throws<ApplicationException>(Method_should_throw_exception_with_invalid_destinationTileId, "destination tile is not valid");
            }

            public void Method_should_throw_exception_with_invalid_destinationTileId()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "bar", "bpgus");
            }
        }
    }
}
