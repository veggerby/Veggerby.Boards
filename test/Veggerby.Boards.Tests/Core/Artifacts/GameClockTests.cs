using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Tests.Core.Artifacts;

public class GameClockTests
{
    public class Constructor
    {
        [Fact]
        public void Should_create_game_clock()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            // act
            var actual = new GameClock("clock", control);

            // assert
            actual.Id.Should().Be("clock");
            actual.Control.Should().Be(control);
        }

        [Fact]
        public void Should_throw_when_id_is_null()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            // act
            var actual = () => new GameClock(null!, control);

            // assert
            actual.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_throw_when_control_is_null()
        {
            // arrange

            // act
            var actual = () => new GameClock("clock", null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("control");
        }
    }
}
