using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.States
{
    public class ArtifactStateEqualityComparerTests
    {

        public class _Equals
        {
            [Fact]
            public void Should_equal_same_artifact_same_value()
            {
                // arrange
                var artifact = new Dice("dice-1");
                var state1 = new DiceState<int>(artifact, 2);
                var state2 = new DiceState<int>(artifact, 2);
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.Equals(state1, state2);

                // assert
                actual.Should().BeTrue();
            }

            [Fact]
            public void Should_equal_same_artifact_different_value()
            {
                // arrange
                var artifact = new Dice("dice-1");
                var state1 = new DiceState<int>(artifact, 2);
                var state2 = new DiceState<int>(artifact, 3);
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.Equals(state1, state2);

                // assert
                actual.Should().BeTrue();
            }

            [Fact]
            public void Should_not_equal_different_artifacts_same_value()
            {
                // arrange
                var artifact1 = new Dice("dice-1");
                var artifact2 = new Dice("dice-2");
                var state1 = new DiceState<int>(artifact1, 2);
                var state2 = new DiceState<int>(artifact2, 2);
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.Equals(state1, state2);

                // assert
                actual.Should().BeFalse();
            }

            [Fact]
            public void Should_not_equal_null_first()
            {
                // arrange
                var artifact = new Dice("dice-1");
                var state = new DiceState<int>(artifact, 2);
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.Equals(null, state);

                // assert
                actual.Should().BeFalse();
            }

            [Fact]
            public void Should_not_equal_null_second()
            {
                // arrange
                var artifact = new Dice("dice-1");
                var state = new DiceState<int>(artifact, 2);
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.Equals(state, null);

                // assert
                actual.Should().BeFalse();
            }

            [Fact]
            public void Should_not_equal_null_both()
            {
                // arrange
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.Equals(null, null);

                // assert
                actual.Should().BeFalse();
            }

            [Fact]
            public void Should_not_equal_different_artifacts()
            {
                // arrange
                var artifact1 = new Dice("dice-1");
                var artifact2 = new Dice("dice-2");
                var state1 = new DiceState<int>(artifact1, 2);
                var state2 = new DiceState<int>(artifact2, 3);
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.Equals(state1, state2);

                // assert
                actual.Should().BeFalse();
            }
        }

        public class _GetHashCode
        {
            [Fact]
            public void Should_return_artifact_hashcode()
            {
                // arrange
                var artifact = new Dice("dice");
                var state = new DiceState<int>(artifact, 5);
                var comparer = new ArtifactStateEqualityComparer();

                // act
                var actual = comparer.GetHashCode(state);

                // assert
                actual.Should().Be(artifact.GetHashCode());
            }
        }
    }
}