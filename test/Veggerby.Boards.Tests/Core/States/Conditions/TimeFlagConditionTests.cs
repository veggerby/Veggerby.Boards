using System;
using System.Collections.Generic;
using System.Linq;

using NSubstitute;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.States.Conditions;

public class TimeFlagConditionTests
{
    public class Evaluate
    {
        [Fact]
        public void Should_return_valid_when_player_time_expired()
        {
            // arrange
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.Zero,
                [player2] = TimeSpan.FromMinutes(5)
            };

            var clockState = new ClockState(clock, remainingTime);

            var tile = new Tile("tile");
            var direction = new Direction("self");
            var relation = new TileRelation(tile, tile, direction);
            var board = new Board("board", new[] { relation });
            var game = new Game(board, new[] { player1, player2 }, new Artifact[] { player1, player2 });

            var state = GameState.New(new[] { clockState });

            var condition = new TimeFlagCondition(game);

            // act
            var actual = condition.Evaluate(state);

            // assert
            actual.Result.Should().Be(ConditionResult.Valid);
        }

        [Fact]
        public void Should_return_ignore_when_all_players_have_time()
        {
            // arrange
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var control = new TimeControl
            {
                InitialTime = TimeSpan.FromMinutes(5)
            };

            var clock = new GameClock("clock", control);

            var remainingTime = new Dictionary<Player, TimeSpan>
            {
                [player1] = TimeSpan.FromMinutes(3),
                [player2] = TimeSpan.FromMinutes(5)
            };

            var clockState = new ClockState(clock, remainingTime);

            var tile = new Tile("tile");
            var direction = new Direction("self");
            var relation = new TileRelation(tile, tile, direction);
            var board = new Board("board", new[] { relation });
            var game = new Game(board, new[] { player1, player2 }, new Artifact[] { player1, player2 });

            var state = GameState.New(new[] { clockState });

            var condition = new TimeFlagCondition(game);

            // act
            var actual = condition.Evaluate(state);

            // assert
            actual.Result.Should().Be(ConditionResult.Ignore);
            actual.Reason.Should().Be("All players have time remaining");
        }

        [Fact]
        public void Should_return_ignore_when_no_clock_configured()
        {
            // arrange
            var player1 = new Player("player1");
            var player2 = new Player("player2");

            var tile = new Tile("tile");
            var direction = new Direction("self");
            var relation = new TileRelation(tile, tile, direction);
            var board = new Board("board", new[] { relation });
            var game = new Game(board, new[] { player1, player2 }, new Artifact[] { player1, player2 });

            var state = GameState.New(Enumerable.Empty<IArtifactState>());

            var condition = new TimeFlagCondition(game);

            // act
            var actual = condition.Evaluate(state);

            // assert
            actual.Result.Should().Be(ConditionResult.Ignore);
            actual.Reason.Should().Be("No clock configured");
        }
    }
}
