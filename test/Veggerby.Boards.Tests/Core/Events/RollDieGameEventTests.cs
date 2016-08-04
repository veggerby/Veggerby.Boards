using System;
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
                Assert.Equal(die, actual.Die);
                Assert.Equal(4, actual.NewDieValue);
            }

            [Fact]
            public void Should_throw_null_exception_with_no_die()
            {
                // arrange

                // act
                 var actual = Assert.Throws<ArgumentNullException>(() => new RollDieGameEvent<int>(null, 4));
                
                // assert
                 Assert.Equal("die", actual.ParamName);
            }
        }
    }
}