using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns;

public class PatternVisitorTests
{
    public class Visit
    {
        [Fact]
        public void Should_visit_any_pattern()
        {
            // arrange
            var pattern = new AnyPattern();
            var visitor = new SimplePatternVisitor();

            // act
            pattern.Accept(visitor);
            var actual = visitor.Type;

            // assert
            actual.Should().Be(typeof(AnyPattern));
        }

        [Fact]
        public void Should_visit_direction_pattern()
        {
            // arrange
            var pattern = new DirectionPattern(Direction.Clockwise);
            var visitor = new SimplePatternVisitor();

            // act
            pattern.Accept(visitor);
            var actual = visitor.Type;

            // assert
            actual.Should().Be(typeof(DirectionPattern));
        }

        [Fact]
        public void Should_visit_fixed_pattern()
        {
            // arrange
            var pattern = new FixedPattern([Direction.Clockwise, Direction.North, Direction.Up]);
            var visitor = new SimplePatternVisitor();

            // act
            pattern.Accept(visitor);
            var actual = visitor.Type;

            // assert
            actual.Should().Be(typeof(FixedPattern));
        }

        [Fact]
        public void Should_visit_multidirection_pattern()
        {
            // arrange
            var pattern = new MultiDirectionPattern([Direction.Clockwise, Direction.North, Direction.Up]);
            var visitor = new SimplePatternVisitor();

            // act
            pattern.Accept(visitor);
            var actual = visitor.Type;

            // assert
            actual.Should().Be(typeof(MultiDirectionPattern));
        }

        [Fact]
        public void Should_visit_null_pattern()
        {
            // arrange
            var pattern = new NullPattern();
            var visitor = new SimplePatternVisitor();

            // act
            pattern.Accept(visitor);
            var actual = visitor.Type;

            // assert
            actual.Should().Be(typeof(NullPattern));
        }
    }
}