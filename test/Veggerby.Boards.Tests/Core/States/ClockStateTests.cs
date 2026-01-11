using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.States;

public class ClockStateTests
{
    public class Constructor
    {
        [Fact]
        public void Should_create_clock_state()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(5),
                [player2] = TimeSpan.FromMinutes(5)
            };

            // act
            var actual = new ClockState(clock, remainingTime);

            // assert
            actual.Clock.Should().Be(clock);
            actual.RemainingTime.Should().Equal(remainingTime);
            actual.ActivePlayer.Should().BeNull();
            actual.TurnStartedAt.Should().BeNull();
        }

        [Fact]
        public void Should_throw_when_clock_is_null()
        {
            // arrange
            var remainingTime = new Dictionary<Player, TimeSpan>();

            // act
            var actual = () => new ClockState(null!, remainingTime);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("clock");
        }

        [Fact]
        public void Should_throw_when_remaining_time_is_null()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);

            // act
            var actual = () => new ClockState(clock, null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("remainingTime");
        }
    }

    public class StartTurn
    {
        [Fact]
        public void Should_start_turn()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(5),
                [player2] = TimeSpan.FromMinutes(5)
            };

            var state = new ClockState(clock, remainingTime);
            var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);

            // act
            var actual = state.StartTurn(player1, timestamp);

            // assert
            actual.ActivePlayer.Should().Be(player1);
            actual.TurnStartedAt.Should().Be(timestamp);
        }

        [Fact]
        public void Should_throw_when_player_is_null()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var remainingTime = new Dictionary<Player, TimeSpan>();

            var state = new ClockState(clock, remainingTime);
            var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);

            // act
            var actual = () => state.StartTurn(null!, timestamp);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("player");
        }

        [Fact]
        public void Should_throw_when_turn_already_active()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(5),
                [player2] = TimeSpan.FromMinutes(5)
            };

            var state = new ClockState(clock, remainingTime);
            var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);

            state = state.StartTurn(player1, timestamp);

            // act
            var actual = () => state.StartTurn(player2, timestamp);

            // assert
            actual.Should().Throw<InvalidOperationException>()
                .WithMessage("*turn already active*");
        }
    }

    public class EndTurn
    {
        [Fact]
        public void Should_end_turn_and_deduct_time()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(5),
                [player2] = TimeSpan.FromMinutes(5)
            };

            var state = new ClockState(clock, remainingTime);
            var startTimestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
            var endTimestamp = startTimestamp.AddSeconds(10);

            state = state.StartTurn(player1, startTimestamp);

            // act
            var actual = state.EndTurn(endTimestamp);

            // assert
            actual.ActivePlayer.Should().BeNull();
            actual.TurnStartedAt.Should().BeNull();
            actual.RemainingTime[player1].Should().Be(TimeSpan.FromMinutes(5).Subtract(TimeSpan.FromSeconds(10)));
        }

        [Fact]
        public void Should_apply_fischer_increment()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5),
                Increment = TimeSpan.FromSeconds(2)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(5),
                [player2] = TimeSpan.FromMinutes(5)
            };

            var state = new ClockState(clock, remainingTime);
            var startTimestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
            var endTimestamp = startTimestamp.AddSeconds(10);

            state = state.StartTurn(player1, startTimestamp);

            // act
            var actual = state.EndTurn(endTimestamp);

            // assert
            // Initial 5:00 - 10s elapsed + 2s increment = 4:52
            actual.RemainingTime[player1].Should().Be(TimeSpan.FromMinutes(5).Subtract(TimeSpan.FromSeconds(10)).Add(TimeSpan.FromSeconds(2)));
        }

        [Fact]
        public void Should_throw_when_no_active_turn()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var remainingTime = new Dictionary<Player, TimeSpan>();

            var state = new ClockState(clock, remainingTime);
            var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);

            // act
            var actual = () => state.EndTurn(timestamp);

            // assert
            actual.Should().Throw<InvalidOperationException>()
                .WithMessage("*no active turn*");
        }
    }

    public class IsTimeExpired
    {
        [Fact]
        public void Should_return_true_when_time_expired()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.Zero
            };

            var state = new ClockState(clock, remainingTime);

            // act
            var actual = state.IsTimeExpired(player1);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_return_true_when_time_negative()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromSeconds(-5)
            };

            var state = new ClockState(clock, remainingTime);

            // act
            var actual = state.IsTimeExpired(player1);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_return_false_when_time_remaining()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromSeconds(30)
            };

            var state = new ClockState(clock, remainingTime);

            // act
            var actual = state.IsTimeExpired(player1);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_throw_when_player_is_null()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var remainingTime = new Dictionary<Player, TimeSpan>();

            var state = new ClockState(clock, remainingTime);

            // act
            var actual = () => state.IsTimeExpired(null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("player");
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_be_equal_when_same_state()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(5)
            };

            var state1 = new ClockState(clock, remainingTime);
            var state2 = new ClockState(clock, remainingTime);

            // act
            var actual = state1.Equals((IArtifactState)state2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_be_equal_when_different_remaining_time()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var player1 = new Player("player1");

            var remainingTime1 = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(5)
            };

            var remainingTime2 = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(3)
            };

            var state1 = new ClockState(clock, remainingTime1);
            var state2 = new ClockState(clock, remainingTime2);

            // act
            var actual = state1.Equals((IArtifactState)state2);

            // assert
            actual.Should().BeFalse();
        }
    }
}
