using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations
{
    public class ResolveTilePathPatternVisitorTests
    {
        public class ctor
        {
            [Fact]
            public void Should_create()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-2");

                // act
                var actual = new ResolveTilePathPatternVisitor(board, from, to);

                // assert
                actual.ShouldNotBeNull();
                actual.Board.ShouldBe(board);
                actual.From.ShouldBe(from);
                actual.To.ShouldBe(to);
                actual.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_throw_null_board()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-2");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new ResolveTilePathPatternVisitor(null, from, to));

                // assert
                actual.ParamName.ShouldBe("board");
            }

            [Fact]
            public void Should_throw_null_from_tile()
            {
                // arrange
                var board = new TestBoard();
                var to = board.GetTile("tile-2");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new ResolveTilePathPatternVisitor(board, null, to));

                // assert
                actual.ParamName.ShouldBe("from");
            }

            [Fact]
            public void Should_throw_null_to_tile()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new ResolveTilePathPatternVisitor(board, from, null));

                // assert
                actual.ParamName.ShouldBe("to");
            }

            [Fact]
            public void Should_throw_from_and_to_identical()
            {
                // arrange
                var board = new TestBoard();
                var tile = board.GetTile("tile-1");

                // act
                var actual = Should.Throw<ArgumentException>(() => new ResolveTilePathPatternVisitor(board, tile, tile));

                // assert
                actual.Message.ShouldContain("To cannot be the same af From");
                actual.ParamName.ShouldBe("to");
            }
        }

        public class Visit
        {
            [Fact]
            public void Should_visit_null_pattern()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new NullPattern();

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_direction_pattern_and_resolve()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new DirectionPattern(Direction.Clockwise, true);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldNotBeNull();
                visitor.ResultPath.From.ShouldBe(from);
                visitor.ResultPath.To.ShouldBe(to);
                visitor.ResultPath.Directions.ShouldAllBe(x => Direction.Clockwise.Equals(x));
                visitor.ResultPath.Distance.ShouldBe(21);
            }

            [Fact]
            public void Should_visit_direction_pattern_and_not_resolve_when_not_repeatable()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new DirectionPattern(Direction.Clockwise, false);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_direction_pattern_and_not_resolve_when_wrong_direction()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new DirectionPattern(Direction.South, true);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_direction_pattern_and_not_resolve_when_cross_self()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-x");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new DirectionPattern(Direction.Clockwise, true);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_fixed_pattern_and_resolve()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-14"); // should be across 0-12, and clockwise 12-13 and 13-14
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new FixedPattern(new [] { Direction.Across, Direction.Clockwise, Direction.Clockwise });

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldNotBeNull();
                visitor.ResultPath.From.ShouldBe(from);
                visitor.ResultPath.To.ShouldBe(to);
                visitor.ResultPath.Directions.ShouldBe(new [] { Direction.Across, Direction.Clockwise, Direction.Clockwise });
                visitor.ResultPath.Distance.ShouldBe(8);
            }

            [Fact]
            public void Should_visit_fixed_pattern_and_not_resolve_if_too_short()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new FixedPattern(new [] { Direction.Across, Direction.Clockwise, Direction.Clockwise });

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_fixed_pattern_and_not_resolve_if_direction_not_from_a_tile()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-0");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new FixedPattern(new [] { Direction.Across, Direction.Clockwise, Direction.Across });

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_multi_direction_pattern_and_resolve()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new MultiDirectionPattern(new [] { Direction.Clockwise, Direction.Across }, true);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldNotBeNull();
                visitor.ResultPath.From.ShouldBe(from);
                visitor.ResultPath.To.ShouldBe(to);
                visitor.ResultPath.Directions.ShouldAllBe(x => Direction.Clockwise.Equals(x));
                visitor.ResultPath.Distance.ShouldBe(21);
            }

            [Fact]
            public void Should_visit_multi_direction_pattern_and_resolve_shortest_path()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-4");
                var to = board.GetTile("tile-0");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new MultiDirectionPattern(new [] { Direction.Clockwise, Direction.Across }, true);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldNotBeNull();
                visitor.ResultPath.From.ShouldBe(from);
                visitor.ResultPath.To.ShouldBe(to);
                visitor.ResultPath.Directions.ShouldAllBe(x => Direction.Across.Equals(x));
                visitor.ResultPath.Distance.ShouldBe(2);
            }

            [Fact]
            public void Should_visit_multi_direction_pattern_and_not_resolve_when_not_repeatable()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new MultiDirectionPattern(new [] { Direction.Clockwise, Direction.Across }, false);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_multi_direction_pattern_and_not_resolve_when_wrong_direction()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-8");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new MultiDirectionPattern(new [] { Direction.South, Direction.North }, true);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_multi_direction_pattern_and_not_resolve_when_cross_self()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-x");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new MultiDirectionPattern(new [] { Direction.Clockwise, Direction.Across }, true);

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }

            [Fact]
            public void Should_visit_any_pattern_and_resolve_simple()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-3");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new AnyPattern();

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldNotBeNull();
                visitor.ResultPath.From.ShouldBe(from);
                visitor.ResultPath.To.ShouldBe(to);
                visitor.ResultPath.Directions.ShouldBe(new [] { Direction.Clockwise, Direction.Clockwise });
                visitor.ResultPath.Distance.ShouldBe(6);
            }

            [Fact]
            public void Should_visit_any_pattern_and_resolve_multiple_solutions()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-3");
                var to = board.GetTile("tile-9");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new AnyPattern();

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldNotBeNull();
                visitor.ResultPath.From.ShouldBe(from);
                visitor.ResultPath.To.ShouldBe(to);
                visitor.ResultPath.Directions.ShouldBe(new [] { Direction.Clockwise, Direction.Up, Direction.Across, Direction.Clockwise });
                visitor.ResultPath.Distance.ShouldBe(9); // 3-4-12-8-9
            }

            [Fact]
            public void Should_visit_any_pattern_not_find_result()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-3");
                var to = board.GetTile("tile-x");
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                var pattern = new AnyPattern();

                // act
                pattern.Accept(visitor);

                // assert
                visitor.ResultPath.ShouldBeNull();
            }
        }
    }
}