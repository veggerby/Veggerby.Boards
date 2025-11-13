using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class RandomDiceValueGeneratorTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize()
        {
            // arrange

            // act

            // assert

            var actual = new RandomDiceValueGenerator(1, 6);

            // assert
            actual.MinValue.Should().Be(1);
            actual.MaxValue.Should().Be(6);
        }

        [Fact]
        public void Should_initialize_with_min_max()
        {
            // arrange

            // act

            // assert

            var actual = new RandomDiceValueGenerator(6, 1);

            // assert
            actual.MinValue.Should().Be(1);
            actual.MaxValue.Should().Be(6);
        }
    }

    public class GetValue
    {
        [Theory]
        [Repeat(20)]
        public void Should_return_random_value(Guid id)
        {
            // arrange
            var dice = new Dice("dice");
            var state = new DiceState<int>(dice, 2);
            var generator = new RandomDiceValueGenerator(1, 6);

            // act
            var actual = generator.GetValue(state);

            // assert
            actual.Should().BeInRange(1, 6);
            id.Should().Be(id); // to avoid warning
        }
    }
}
