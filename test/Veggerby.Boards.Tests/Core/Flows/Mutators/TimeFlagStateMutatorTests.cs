using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class TimeFlagStateMutatorTests
{
    public class MutateState
    {
        [Fact]
        public void Should_end_game_when_player_runs_out_of_time()
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
            var @event = new TimeFlagEvent(player1);
            var mutator = new TimeFlagStateMutator(game);

            // act
            var actual = mutator.MutateState(null!, state, @event);

            // assert
            actual.GetStates<GameEndedState>().Should().ContainSingle();

            var outcome = actual.GetStates<StandardGameOutcome>().Single();

            outcome.TerminalCondition.Should().Be("TimeExpired");
            outcome.PlayerResults.Should().HaveCount(2);

            var winner = outcome.PlayerResults.First(r => r.Outcome == OutcomeType.Win);
            var loser = outcome.PlayerResults.First(r => r.Outcome == OutcomeType.Loss);

            winner.Player.Should().Be(player2);
            loser.Player.Should().Be(player1);

            loser.Metrics.Should().ContainKey("Reason");
            loser.Metrics!["Reason"].Should().Be("TimeExpired");
        }

        [Fact]
        public void Should_return_unchanged_state_when_game_already_ended()
        {
            // arrange
            var player1 = new Player("player1");
            var player2 = new Player("player2");
            var tile = new Tile("tile");
            var direction = new Direction("self");
            var relation = new TileRelation(tile, tile, direction);
            var board = new Board("board", new[] { relation });
            var game = new Game(board, new[] { player1, player2 }, new Artifact[] { player1, player2 });
            var state = GameState.New(new IArtifactState[] { new GameEndedState() });
            var @event = new TimeFlagEvent(player1);
            var mutator = new TimeFlagStateMutator(game);

            // act
            var actual = mutator.MutateState(null!, state, @event);

            // assert
            actual.Should().Be(state);
        }
    }
}
