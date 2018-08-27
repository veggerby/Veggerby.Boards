using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Rules.Algorithms;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Rules.Algorithms
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

                actual.ShouldNotBeNull();
                actual.Distance.ShouldBe(4);
                actual.Tiles.Select(x => x.Id).ShouldBe(new[] { "tile-0", "tile-1", "tile-2", "tile-3", "tile-4" });
                actual.Directions.ShouldBe(new[] { Direction.Clockwise, Direction.Clockwise, Direction.Clockwise, Direction.Clockwise });
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

                actual.ShouldNotBeNull();
                actual.Distance.ShouldBe(5);
                actual.Tiles.Select(x => x.Id).ShouldBe(new[] { "tile-0", "tile-24" });
                actual.Directions.ShouldBe(new[] { Direction.Across });
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

                actual.ShouldNotBeNull();
                actual.Distance.ShouldBe(7);
                actual.Tiles.Select(x => x.Id).ShouldBe(new[] { "tile-0", "tile-24", "tile-25", "tile-26" });
                actual.Directions.ShouldBe(new[] { Direction.Across, Direction.Clockwise, Direction.Clockwise });
            }
        }

        public class Visit_AnyPattern
        {
            [Fact]
            public void Should_get_valid_path()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-31");
                var to = board.GetTile("tile-27");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new AnyPattern();

                // act
                pattern.Accept(visitor);

                // assert
                var actual = visitor.ResultPath;
                actual.Distance.ShouldBe(9);
                actual.Tiles.Select(x => x.Id).ShouldBe(new[] { "tile-31", "tile-0", "tile-24", "tile-25", "tile-26", "tile-27" });
                actual.Directions.ShouldBe(new [] { Direction.Clockwise, Direction.Across, Direction.Clockwise, Direction.Clockwise, Direction.Clockwise });
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

                actual.ShouldBeNull();
            }
        }
    }
}