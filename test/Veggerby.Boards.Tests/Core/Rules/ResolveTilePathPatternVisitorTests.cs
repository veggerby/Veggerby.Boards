using System.Linq;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Rules
{
    public class ResolveTilePathPatternVisitorTests
    {
        public class Visit_DirectionPattern
        {
            [Fact]
            public void Should_get_valid_path()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-4");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new DirectionPattern(Direction.Clockwise);

                // act
                pattern.Accept(visitor);
                
                // assert
                var actual = visitor.ResultPath;

                Assert.NotNull(actual);
                Assert.Equal(4, actual.Distance);
                Assert.Equal(new[] { "tile-0", "tile-1", "tile-2", "tile-3", "tile-4" }, actual.Tiles.Select(x => x.Id));
                Assert.Equal(new[] { "clockwise", "clockwise", "clockwise", "clockwise" }, actual.Directions.Select(x => x.Id));
            }
        }

        public class Visit_MultiDirectionPattern
        {
            [Fact]
            public void Should_get_valid_path()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-24");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new MultiDirectionPattern(new [] { Direction.Across });

                // act
                pattern.Accept(visitor);
                
                // assert
                var actual = visitor.ResultPath;

                Assert.NotNull(actual);
                Assert.Equal(5, actual.Distance);
                Assert.Equal(new[] { "tile-0", "tile-24" }, actual.Tiles.Select(x => x.Id));
                Assert.Equal(new[] { "across" }, actual.Directions.Select(x => x.Id));
            }
        }

        public class Visit_FixedPattern
        {
            [Fact]
            public void Should_get_valid_path()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-26");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new FixedPattern(new [] { Direction.Across, Direction.Clockwise, Direction.Clockwise });

                // act
                pattern.Accept(visitor);
                
                // assert
                var actual = visitor.ResultPath;

                Assert.NotNull(actual);
                Assert.Equal(7, actual.Distance);
                Assert.Equal(new[] { "tile-0", "tile-24", "tile-25", "tile-26" }, actual.Tiles.Select(x => x.Id));
                Assert.Equal(new[] { "across", "clockwise", "clockwise" }, actual.Directions.Select(x => x.Id));
            }
        }

        public class Visit_AnyPattern
        {
            [Fact]
            public void Should_get_valid_path()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-26");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new AnyPattern();

                // act
                pattern.Accept(visitor);

                // assert
                var actual = visitor.ResultPath;
                Assert.Equal(7, actual.Distance);
                Assert.Equal(new[] { "tile-0", "tile-24", "tile-25", "tile-26" }, actual.Tiles.Select(x => x.Id));
                Assert.Equal(new[] { "across", "clockwise", "clockwise" }, actual.Directions.Select(x => x.Id));
            }
        }

        public class Visit_NullPattern
        {
            [Fact]
            public void Should_get_valid_path()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-26");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new NullPattern();

                // act
                pattern.Accept(visitor);
                
                // assert
                var actual = visitor.ResultPath;

                Assert.Null(actual);
            }
        }
    }
}