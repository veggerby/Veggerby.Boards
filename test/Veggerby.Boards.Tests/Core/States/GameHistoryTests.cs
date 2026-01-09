using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class GameHistoryTests
{
    public GameProgress InitialProgress
    {
        get;
    }

    public GameHistoryTests()
    {
        var builder = new TestGameBuilder();
        InitialProgress = builder.Compile();
    }

    private static TilePath CreatePath(Tile from, Tile to)
    {
        var relation = new TileRelation(from, to, Direction.Clockwise);
        return new TilePath(new[] { relation });
    }

    public class Constructor : GameHistoryTests
    {
        [Fact]
        public void Should_create_new_history_with_initial_progress()
        {
            // arrange

            // act
            var actual = new GameHistory(InitialProgress);

            // assert
            actual.Should().NotBeNull();
            actual.Current.Should().Be(InitialProgress);
            actual.CurrentIndex.Should().Be(0);
            actual.Length.Should().Be(1);
            actual.CanUndo.Should().BeFalse();
            actual.CanRedo.Should().BeFalse();
        }

        [Fact]
        public void Should_throw_when_initial_progress_is_null()
        {
            // arrange

            // act
            Action act = () => new GameHistory(null!);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }
    }

    public class Undo : GameHistoryTests
    {
        [Fact]
        public void Should_move_back_one_step_in_history()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var history1 = history.Apply(event1);

            // act
            var actual = history1.Undo();

            // assert
            actual.Should().NotBeNull();
            actual.CurrentIndex.Should().Be(0);
            actual.Current.Should().Be(InitialProgress);
            actual.CanUndo.Should().BeFalse();
            actual.CanRedo.Should().BeTrue();
        }

        [Fact]
        public void Should_throw_when_at_start_of_history()
        {
            // arrange
            var history = new GameHistory(InitialProgress);

            // act
            Action act = () => history.Undo();

            // assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot undo: at start of history");
        }

        [Fact]
        public void Should_support_multiple_undo_operations()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);

            // act
            var actual1 = history2.Undo();
            var actual2 = actual1.Undo();

            // assert
            actual1.CurrentIndex.Should().Be(1);
            actual2.CurrentIndex.Should().Be(0);
            actual2.Current.Should().Be(InitialProgress);
        }
    }

    public class Redo : GameHistoryTests
    {
        [Fact]
        public void Should_move_forward_one_step_in_history()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var history1 = history.Apply(event1);
            var historyAfterUndo = history1.Undo();

            // act
            var actual = historyAfterUndo.Redo();

            // assert
            actual.Should().NotBeNull();
            actual.CurrentIndex.Should().Be(1);
            actual.Current.State.Should().Be(history1.Current.State);
            actual.CanUndo.Should().BeTrue();
            actual.CanRedo.Should().BeFalse();
        }

        [Fact]
        public void Should_throw_when_at_end_of_timeline()
        {
            // arrange
            var history = new GameHistory(InitialProgress);

            // act
            Action act = () => history.Redo();

            // assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot redo: at end of history");
        }

        [Fact]
        public void Should_support_multiple_redo_operations()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);
            var historyAfterUndo = history2.Undo().Undo();

            // act
            var actual1 = historyAfterUndo.Redo();
            var actual2 = actual1.Redo();

            // assert
            actual1.CurrentIndex.Should().Be(1);
            actual2.CurrentIndex.Should().Be(2);
            actual2.Current.State.Should().Be(history2.Current.State);
        }
    }

    public class Apply : GameHistoryTests
    {
        [Fact]
        public void Should_append_event_to_history()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));

            // act
            var actual = history.Apply(event1);

            // assert
            actual.Should().NotBeNull();
            actual.Length.Should().Be(2);
            actual.CurrentIndex.Should().Be(1);
            actual.CanUndo.Should().BeTrue();
            actual.CanRedo.Should().BeFalse();
        }

        [Fact]
        public void Should_throw_when_event_is_null()
        {
            // arrange
            var history = new GameHistory(InitialProgress);

            // act
            Action act = () => history.Apply(null!);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_truncate_timeline_when_applying_after_undo()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));
            var alternativeEvent = new MovePieceGameEvent(piece, CreatePath(tile2, tile1));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);
            var historyAfterUndo = history2.Undo();

            // act
            var actual = historyAfterUndo.Apply(alternativeEvent);

            // assert
            actual.Length.Should().Be(3);
            actual.CurrentIndex.Should().Be(2);
            actual.CanRedo.Should().BeFalse();
        }

        [Fact]
        public void Should_support_sequential_applications()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));
            var event3 = new MovePieceGameEvent(piece, CreatePath(tile3, tile1));

            // act
            var actual = history
                .Apply(event1)
                .Apply(event2)
                .Apply(event3);

            // assert
            actual.Length.Should().Be(4);
            actual.CurrentIndex.Should().Be(3);
        }
    }

    public class GoTo : GameHistoryTests
    {
        [Fact]
        public void Should_jump_to_specific_index()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));
            var event3 = new MovePieceGameEvent(piece, CreatePath(tile3, tile1));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);
            var history3 = history2.Apply(event3);

            // act
            var actual = history3.GoTo(1);

            // assert
            actual.CurrentIndex.Should().Be(1);
            actual.Current.State.Should().Be(history1.Current.State);
        }

        [Fact]
        public void Should_throw_when_index_is_negative()
        {
            // arrange
            var history = new GameHistory(InitialProgress);

            // act
            Action act = () => history.GoTo(-1);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Should_throw_when_index_is_out_of_bounds()
        {
            // arrange
            var history = new GameHistory(InitialProgress);

            // act
            Action act = () => history.GoTo(5);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Should_support_jumping_to_start()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var history1 = history.Apply(event1);

            // act
            var actual = history1.GoTo(0);

            // assert
            actual.CurrentIndex.Should().Be(0);
            actual.Current.Should().Be(InitialProgress);
        }

        [Fact]
        public void Should_support_jumping_to_end()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var history1 = history.Apply(event1);
            var historyAfterUndo = history1.Undo();

            // act
            var actual = historyAfterUndo.GoTo(1);

            // assert
            actual.CurrentIndex.Should().Be(1);
            actual.Current.State.Should().Be(history1.Current.State);
        }
    }

    public class GetEventHistory : GameHistoryTests
    {
        [Fact]
        public void Should_return_single_null_event_for_initial_state()
        {
            // arrange
            var history = new GameHistory(InitialProgress);

            // act
            var actual = history.GetEventHistory();

            // assert
            actual.Should().NotBeNull();
            actual.Should().HaveCount(1);
            actual[0].Should().BeOfType<NullGameEvent>();
        }

        [Fact]
        public void Should_return_all_events_up_to_current_position()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);

            // act
            var actual = history2.GetEventHistory();

            // assert
            actual.Should().HaveCount(3);
            actual[0].Should().BeOfType<NullGameEvent>();
            actual[1].Should().Be(event1);
            actual[2].Should().Be(event2);
        }

        [Fact]
        public void Should_return_events_up_to_current_position_after_undo()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);
            var historyAfterUndo = history2.Undo();

            // act
            var actual = historyAfterUndo.GetEventHistory();

            // assert
            actual.Should().HaveCount(2);
            actual[0].Should().BeOfType<NullGameEvent>();
            actual[1].Should().Be(event1);
        }

        [Fact]
        public void Should_return_read_only_list()
        {
            // arrange
            var history = new GameHistory(InitialProgress);

            // act
            var actual = history.GetEventHistory();

            // assert
            actual.Should().BeAssignableTo<IReadOnlyList<IGameEvent>>();
        }
    }

    public class DeterminismTests : GameHistoryTests
    {
        [Fact]
        public void Should_preserve_state_hash_through_undo_redo()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var history1 = history.Apply(event1);
            var originalHash = history1.Current.State.Hash;

            // act
            var historyAfterUndo = history1.Undo();
            var historyAfterRedo = historyAfterUndo.Redo();

            // assert
            historyAfterRedo.Current.State.Hash.Should().Be(originalHash);
        }

        [Fact]
        public void Should_produce_identical_state_after_navigation()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);
            var targetState = history1.Current.State;

            // act
            var navigatedHistory = history2.GoTo(1);

            // assert
            navigatedHistory.Current.State.Should().Be(targetState);
        }
    }

    public class ImmutabilityTests : GameHistoryTests
    {
        [Fact]
        public void Should_not_mutate_original_history_on_apply()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));

            // act
            var newHistory = history.Apply(event1);

            // assert
            history.Length.Should().Be(1);
            history.CurrentIndex.Should().Be(0);
            newHistory.Length.Should().Be(2);
            newHistory.CurrentIndex.Should().Be(1);
        }

        [Fact]
        public void Should_not_mutate_original_history_on_undo()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var history1 = history.Apply(event1);

            // act
            var newHistory = history1.Undo();

            // assert
            history1.CurrentIndex.Should().Be(1);
            history1.CanUndo.Should().BeTrue();
            newHistory.CurrentIndex.Should().Be(0);
            newHistory.CanUndo.Should().BeFalse();
        }

        [Fact]
        public void Should_not_mutate_original_history_on_goto()
        {
            // arrange
            var history = new GameHistory(InitialProgress);
            var piece = InitialProgress.Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = InitialProgress.Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = InitialProgress.Game.GetTile("tile-2").EnsureNotNull();
            var tile3 = InitialProgress.Game.GetTile("tile-3").EnsureNotNull();

            var event1 = new MovePieceGameEvent(piece, CreatePath(tile1, tile2));
            var event2 = new MovePieceGameEvent(piece, CreatePath(tile2, tile3));

            var history1 = history.Apply(event1);
            var history2 = history1.Apply(event2);

            // act
            var newHistory = history2.GoTo(0);

            // assert
            history2.CurrentIndex.Should().Be(2);
            newHistory.CurrentIndex.Should().Be(0);
        }
    }
}
