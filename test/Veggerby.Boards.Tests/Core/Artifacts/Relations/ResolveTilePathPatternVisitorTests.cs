using System;

using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations;

public class ResolveTilePathPatternVisitorTests
{
    public class Create
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
            actual.Should().NotBeNull();
            actual.Board.Should().Be(board);
            actual.From.Should().Be(from);
            actual.To.Should().Be(to);
            actual.ResultPath.Should().BeNull();
        }

        [Fact]
        public void Should_throw_null_board()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-1");
            var to = board.GetTile("tile-2");

            // act
            var actual = () => new ResolveTilePathPatternVisitor(null, from, to);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("board");
        }

        [Fact]
        public void Should_throw_null_from_tile()
        {
            // arrange
            var board = new TestBoard();
            var to = board.GetTile("tile-2");

            // act
            var actual = () => new ResolveTilePathPatternVisitor(board, null, to);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("from");
        }

        [Fact]
        public void Should_throw_null_to_tile()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-1");

            // act
            var actual = () => new ResolveTilePathPatternVisitor(board, from, null);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("to");
        }

        [Fact]
        public void Should_throw_from_and_to_identical()
        {
            // arrange
            var board = new TestBoard();
            var tile = board.GetTile("tile-1");

            // act
            var actual = () => new ResolveTilePathPatternVisitor(board, tile, tile);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("to").WithMessage("*To cannot be the same af From*");
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
            visitor.ResultPath.Should().BeNull();
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
            visitor.ResultPath.Should().NotBeNull();
            visitor.ResultPath.From.Should().Be(from);
            visitor.ResultPath.To.Should().Be(to);
            visitor.ResultPath.Directions.Should().OnlyContain(x => Direction.Clockwise.Equals(x));
            visitor.ResultPath.Distance.Should().Be(21);
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
            visitor.ResultPath.Should().BeNull();
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
            visitor.ResultPath.Should().BeNull();
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
            visitor.ResultPath.Should().BeNull();
        }

        [Fact]
        public void Should_visit_fixed_pattern_and_resolve()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-0");
            var to = board.GetTile("tile-14"); // should be across 0-12, and clockwise 12-13 and 13-14
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new FixedPattern([Direction.Across, Direction.Clockwise, Direction.Clockwise]);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().NotBeNull();
            visitor.ResultPath.From.Should().Be(from);
            visitor.ResultPath.To.Should().Be(to);
            visitor.ResultPath.Directions.Should().Equal([Direction.Across, Direction.Clockwise, Direction.Clockwise]);
            visitor.ResultPath.Distance.Should().Be(8);
        }

        [Fact]
        public void Should_visit_fixed_pattern_and_not_resolve_if_too_short()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-0");
            var to = board.GetTile("tile-8");
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new FixedPattern([Direction.Across, Direction.Clockwise, Direction.Clockwise]);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().BeNull();
        }

        [Fact]
        public void Should_visit_fixed_pattern_and_not_resolve_if_direction_not_from_a_tile()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-0");
            var to = board.GetTile("tile-8");
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new FixedPattern([Direction.Across, Direction.Clockwise, Direction.Across]);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().BeNull();
        }

        [Fact]
        public void Should_visit_multi_direction_pattern_and_resolve()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-1");
            var to = board.GetTile("tile-8");
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new MultiDirectionPattern([Direction.Clockwise, Direction.Across], true);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().NotBeNull();
            visitor.ResultPath.From.Should().Be(from);
            visitor.ResultPath.To.Should().Be(to);
            visitor.ResultPath.Directions.Should().OnlyContain(x => Direction.Clockwise.Equals(x));
            visitor.ResultPath.Distance.Should().Be(21);
        }

        [Fact]
        public void Should_visit_multi_direction_pattern_and_resolve_shortest_path()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-4");
            var to = board.GetTile("tile-0");
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new MultiDirectionPattern([Direction.Clockwise, Direction.Across], true);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().NotBeNull();
            visitor.ResultPath.From.Should().Be(from);
            visitor.ResultPath.To.Should().Be(to);
            visitor.ResultPath.Directions.Should().OnlyContain(x => Direction.Across.Equals(x));
            visitor.ResultPath.Distance.Should().Be(2);
        }

        [Fact]
        public void Should_visit_multi_direction_pattern_and_not_resolve_when_not_repeatable()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-1");
            var to = board.GetTile("tile-8");
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new MultiDirectionPattern([Direction.Clockwise, Direction.Across], false);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().BeNull();
        }

        [Fact]
        public void Should_visit_multi_direction_pattern_and_not_resolve_when_wrong_direction()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-1");
            var to = board.GetTile("tile-8");
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new MultiDirectionPattern([Direction.South, Direction.North], true);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().BeNull();
        }

        [Fact]
        public void Should_visit_multi_direction_pattern_and_not_resolve_when_cross_self()
        {
            // arrange
            var board = new TestBoard();
            var from = board.GetTile("tile-1");
            var to = board.GetTile("tile-x");
            var visitor = new ResolveTilePathPatternVisitor(board, from, to);
            var pattern = new MultiDirectionPattern([Direction.Clockwise, Direction.Across], true);

            // act
            pattern.Accept(visitor);

            // assert
            visitor.ResultPath.Should().BeNull();
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
            visitor.ResultPath.Should().NotBeNull();
            visitor.ResultPath.From.Should().Be(from);
            visitor.ResultPath.To.Should().Be(to);
            visitor.ResultPath.Directions.Should().Equal([Direction.Clockwise, Direction.Clockwise]);
            visitor.ResultPath.Distance.Should().Be(6);
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
            visitor.ResultPath.Should().NotBeNull();
            visitor.ResultPath.From.Should().Be(from);
            visitor.ResultPath.To.Should().Be(to);
            visitor.ResultPath.Directions.Should().Equal([Direction.Clockwise, Direction.Up, Direction.Across, Direction.Clockwise]);
            visitor.ResultPath.Distance.Should().Be(9); // 3-4-12-8-9
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
            visitor.ResultPath.Should().BeNull();
        }
    }
}