using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core;

public class GameBuilderClockTests
{
    private class TestGameBuilderWithClock : GameBuilder
    {
        private readonly Action<TestGameBuilderWithClock>? _buildAction;

        public TestGameBuilderWithClock(Action<TestGameBuilderWithClock>? buildAction = null)
        {
            _buildAction = buildAction;
        }

        protected override void Build()
        {
            BoardId = "test-board";
            AddPlayer("player1");
            AddPlayer("player2");
            
            // Add minimal board structure (required)
            AddTile("tile1");
            AddTile("tile2");
            AddDirection("dir1");
            WithTile("tile1").WithRelationTo("tile2").InDirection("dir1").Done();

            _buildAction?.Invoke(this);
        }

        public void AddClock(string clockId, TimeControl control)
        {
            WithClock(clockId, control);
        }
    }

    public class WithClock_Tests
    {
        [Fact]
        public void Should_create_clock_artifact()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("main-clock", new TimeControl
                {
                    InitialTime = TimeSpan.FromMinutes(5),
                    Increment = TimeSpan.FromSeconds(2)
                });
            });

            // act
            var progress = builder.Compile();

            // assert
            var clock = progress.Game.Artifacts.OfType<GameClock>().SingleOrDefault();
            clock.Should().NotBeNull();
            clock!.Id.Should().Be("main-clock");
            clock.Control.InitialTime.Should().Be(TimeSpan.FromMinutes(5));
            clock.Control.Increment.Should().Be(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Should_initialize_clock_state_with_initial_time()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("main-clock", new TimeControl
                {
                    InitialTime = TimeSpan.FromMinutes(5),
                    Increment = TimeSpan.FromSeconds(2)
                });
            });

            // act
            var progress = builder.Compile();

            // assert
            var clockState = progress.State.GetStates<ClockState>().SingleOrDefault();
            clockState.Should().NotBeNull();
            clockState!.RemainingTime.Should().HaveCount(2);
            clockState.RemainingTime.Values.Should().AllSatisfy(time => time.Should().Be(TimeSpan.FromMinutes(5)));
            clockState.ActivePlayer.Should().BeNull();
            clockState.TurnStartedAt.Should().BeNull();
        }

        [Fact]
        public void Should_throw_when_duplicate_clock_id()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("main-clock", new TimeControl { InitialTime = TimeSpan.FromMinutes(5) });
            });

            // act
            var action = () => builder.Compile();

            // This will fail during Build() when adding the duplicate
            builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("main-clock", new TimeControl { InitialTime = TimeSpan.FromMinutes(5) });
                b.AddClock("main-clock", new TimeControl { InitialTime = TimeSpan.FromMinutes(3) });
            });

            // assert
            action = () => builder.Compile();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Duplicate clock definition 'main-clock'*");
        }

        [Fact]
        public void Should_throw_when_clock_id_is_null()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock(null!, new TimeControl { InitialTime = TimeSpan.FromMinutes(5) });
            });

            // act
            var action = () => builder.Compile();

            // assert
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("clockId");
        }

        [Fact]
        public void Should_throw_when_clock_id_is_empty()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("", new TimeControl { InitialTime = TimeSpan.FromMinutes(5) });
            });

            // act
            var action = () => builder.Compile();

            // assert
            action.Should().Throw<ArgumentException>()
                .WithParameterName("clockId");
        }

        [Fact]
        public void Should_throw_when_control_is_null()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("main-clock", null!);
            });

            // act
            var action = () => builder.Compile();

            // assert
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("control");
        }

        [Fact]
        public void Should_support_multiple_clocks()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("clock1", new TimeControl { InitialTime = TimeSpan.FromMinutes(5) });
                b.AddClock("clock2", new TimeControl { InitialTime = TimeSpan.FromMinutes(3) });
            });

            // act
            var progress = builder.Compile();

            // assert
            var clocks = progress.Game.Artifacts.OfType<GameClock>().ToList();
            clocks.Should().HaveCount(2);
            clocks.Select(c => c.Id).Should().Contain(new[] { "clock1", "clock2" });

            var clockStates = progress.State.GetStates<ClockState>().ToList();
            clockStates.Should().HaveCount(2);
        }

        [Fact]
        public void Should_initialize_clock_state_for_all_players()
        {
            // arrange
            var builder = new TestGameBuilderWithClock(b =>
            {
                b.AddClock("main-clock", new TimeControl { InitialTime = TimeSpan.FromMinutes(5) });
            });

            // act
            var progress = builder.Compile();

            // assert
            var clockState = progress.State.GetStates<ClockState>().Single();
            var player1 = progress.Game.Players.First(p => p.Id == "player1");
            var player2 = progress.Game.Players.First(p => p.Id == "player2");

            clockState.RemainingTime.Should().ContainKey(player1);
            clockState.RemainingTime.Should().ContainKey(player2);
            clockState.RemainingTime[player1].Should().Be(TimeSpan.FromMinutes(5));
            clockState.RemainingTime[player2].Should().Be(TimeSpan.FromMinutes(5));
        }
    }
}
