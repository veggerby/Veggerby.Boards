using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Events
{
    public class EndTurnGameEventTests
    {
        public class ctor 
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                var player = new Player("player");

                // act
                var actual = new EndTurnGameEvent(player);;
                
                // assert
                actual.NextPlayer.ShouldBe(player);
            }
        }
    }
}