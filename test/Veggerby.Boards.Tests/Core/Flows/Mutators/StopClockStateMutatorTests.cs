using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class StopClockStateMutatorTests
{
    public class MutateState
    {
        [Fact]
        public void Should_stop_clock_and_deduct_time()
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

            var startTimestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
            var endTimestamp = startTimestamp.AddSeconds(10);

            var clockState = new ClockState(clock, remainingTime, player1, startTimestamp);

            var state = GameState.New(new[] { clockState });

            var @event = new StopClockEvent(clock, endTimestamp);
            var mutator = new StopClockStateMutator();

            // act
            var actual = mutator.MutateState(null!, state, @event);

            // assert
            var newClockState = actual.GetStates<ClockState>().Single();

            newClockState.ActivePlayer.Should().BeNull();
            newClockState.TurnStartedAt.Should().BeNull();
            newClockState.RemainingTime[player1].Should().Be(TimeSpan.FromMinutes(5).Subtract(TimeSpan.FromSeconds(10)));
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

            var startTimestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
            var endTimestamp = startTimestamp.AddSeconds(10);

            var clockState = new ClockState(clock, remainingTime, player1, startTimestamp);

            var state = GameState.New(new[] { clockState });

            var @event = new StopClockEvent(clock, endTimestamp);
            var mutator = new StopClockStateMutator();

            // act
            var actual = mutator.MutateState(null!, state, @event);

            // assert
            var newClockState = actual.GetStates<ClockState>().Single();

            // Initial 5:00 - 10s elapsed + 2s increment = 4:52
            newClockState.RemainingTime[player1].Should().Be(TimeSpan.FromMinutes(5).Subtract(TimeSpan.FromSeconds(10)).Add(TimeSpan.FromSeconds(2)));
        }

        [Fact]
        public void Should_return_unchanged_state_when_no_clock_state()
        {
            // arrange
            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);
            var state = GameState.New(Enumerable.Empty<IArtifactState>());
            var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
            var @event = new StopClockEvent(clock, timestamp);
            var mutator = new StopClockStateMutator();

            // act
            var actual = mutator.MutateState(null!, state, @event);

            // assert
            actual.Should().Be(state);
        }
    }
}
