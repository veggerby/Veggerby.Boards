using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Veggerby.Boards.Core.Contracts.Builders;
using Veggerby.Boards.Core.Contracts.Models.Navigation;
using Veggerby.Boards.Tests.Core.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Navigation
{
    public class TilePathResolverTests
    {
        public class ResolvePaths
        {
            [Fact]
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
                Assert.Equal(1, paths.Count());
                var path = paths.Single();
                Assert.Same(tile1, path.SourceTileDefinition);
                Assert.Same(tile1, path.DestinationTileDefinition);
                Assert.Equal(1, path.GetTileDefinitions().Count());
                Assert.Same(tile1, path.GetTileDefinitions().Single());
            }

            [Fact]
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
                Assert.Equal(1, paths.Count());
                var path = paths.Single();
                Assert.Same(tile1, path.SourceTileDefinition);
                Assert.Same(tile2, path.DestinationTileDefinition);
                Assert.Equal(2, path.GetTileDefinitions().Count());
                Assert.Same(tile1, path.GetTileDefinitions().First());
                Assert.Same(tile2, path.GetTileDefinitions().Last());
            }

            [Fact]
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
                Assert.Equal(1, paths.Count());
                var path = paths.Single();
                Assert.Same(tile1, path.SourceTileDefinition);
                Assert.Same(tile3, path.DestinationTileDefinition);
                Assert.Equal(3, path.GetTileDefinitions().Count());
                Assert.Same(tile1, path.GetTileDefinitions().First());
                Assert.Same(tile2, path.GetTileDefinitions().Skip(1).First());
                Assert.Same(tile3, path.GetTileDefinitions().Skip(2).First());
            }

            [Fact]
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
                Assert.Empty(paths);
            }

            [Fact]
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
                Assert.Equal(2, paths.Count());

                var path1 = paths.First();
                var path2 = paths.Last();

                Assert.Equal(2, path1.GetTileDefinitions().Count());
                Assert.Equal(tile1, path1.GetTileDefinitions().First());
                Assert.Equal(tile2, path1.GetTileDefinitions().Last());

                Assert.Equal(3, path2.GetTileDefinitions().Count());
                Assert.Equal(tile1, path2.GetTileDefinitions().First());
                Assert.Equal(tile3, path2.GetTileDefinitions().Skip(1).First());
                Assert.Equal(tile2, path2.GetTileDefinitions().Skip(2).First());
            }

            [Fact]
            public async Task Should_throw_exception_with_invalid_sourceTileId()
            {
                // assert
                await Assert.ThrowsAsync<ApplicationException>(Method_should_throw_exception_with_invalid_sourceTileId);
            }

            public Task Method_should_throw_exception_with_invalid_sourceTileId()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "bogus", "bar");

                return Task.FromResult(0);
            }

            [Fact]
            public async Task Should_throw_exception_with_invalid_destinationTileId()
            {
                // assert
                await Assert.ThrowsAsync<ApplicationException>(Method_should_throw_exception_with_invalid_destinationTileId);
            }

            public Task Method_should_throw_exception_with_invalid_destinationTileId()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();
                var boardDefinition = builder.Compile();

                var tilepathResolver = new TilePathResolver();

                // act
                var paths = tilepathResolver.ResolvePaths(boardDefinition, "bar", "bogus");
                
                return Task.FromResult(0);
            }
        }
    }
}
