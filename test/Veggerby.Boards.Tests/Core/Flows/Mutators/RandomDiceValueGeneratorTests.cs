using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Utils;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators
{
    public class RandomDiceValueGeneratorTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize()
            {
                // arrange
                // act
                var actual = new RandomDiceValueGenerator(1, 6);

                // assert
                actual.MinValue.ShouldBe(1);
                actual.MaxValue.ShouldBe(6);
            }

            [Fact]
            public void Should_initialize_with_min_max()
            {
                // arrange
                // act
                var actual = new RandomDiceValueGenerator(6, 1);

                // assert
                actual.MinValue.ShouldBe(1);
                actual.MaxValue.ShouldBe(6);
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
                actual.ShouldBeInRange(1, 6);
                id.ShouldNotBeNull(); // dummy to avoid warning
            }
        }
    }
}