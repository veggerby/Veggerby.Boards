using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class StartClockStateMutatorTests
{
    public class MutateState
    {
        [Fact]
        public void Should_start_clock_for_player()
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

            var clockState = new ClockState(clock, remainingTime);
            var state = GameState.New(new[] { clockState });
            var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);

            var @event = new StartClockEvent(clock, player1, timestamp);
            var mutator = new StartClockStateMutator();

            // act
            var actual = mutator.MutateState(null!, state, @event);

            // assert
            var newClockState = actual.GetStates<ClockState>().Single();

            newClockState.ActivePlayer.Should().Be(player1);
            newClockState.TurnStartedAt.Should().Be(timestamp);
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
            var player1 = new Player("player1");
            var state = GameState.New(Enumerable.Empty<IArtifactState>());
            var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);

            var @event = new StartClockEvent(clock, player1, timestamp);
            var mutator = new StartClockStateMutator();

            // act
            var actual = mutator.MutateState(null!, state, @event);

            // assert
            actual.Should().Be(state);
        }
    }
}
