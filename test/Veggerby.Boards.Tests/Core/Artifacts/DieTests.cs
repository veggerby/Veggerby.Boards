using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class DieTests
    {
        public class Roll
        {
            [Fact]
            public void Should_return_die_value()
            {
                // arrange
                var die = new Die<int>("die", new StaticDieValueGenerator(3));

                // act
                var actual = die.Roll(null);

                // assert
                actual.ShouldBe(3);
            }

            [Fact]
            public void Should_throw_when_rolling_die_with_state_from_other_die()
            {
                // arrange
                var die = new Die<int>("die", new StaticDieValueGenerator(3));
                var other = new Die<int>("other", new StaticDieValueGenerator(3));

                // act + assert
                var actual = Should.Throw<ArgumentException>(() => die.Roll(new DieState<int>(other, 3)));

                // assert
                actual.ParamName.ShouldBe("currentState");
            }
        }
    }
}