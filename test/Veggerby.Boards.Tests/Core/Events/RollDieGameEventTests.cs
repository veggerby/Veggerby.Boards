using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Events
{
    public class RollDieGameEventTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                var die = new Die<int>("die", new StaticDieValueGenerator(3));

                // act
                var actual = new RollDieGameEvent<int>(die, 4);
                
                // assert
                actual.Die.ShouldBe(die);
                actual.NewDieValue.ShouldBe(4);
            }

            [Fact]
            public void Should_throw_null_exception_with_no_die()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new RollDieGameEvent<int>(null, 4));

                // assert
                actual.ParamName.ShouldBe("die");
            }
        }
    }
}