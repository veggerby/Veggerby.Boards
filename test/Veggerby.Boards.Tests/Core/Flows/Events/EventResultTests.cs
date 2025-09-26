using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Flows.Events;

public class EventResultTests
{
    private class PhaseClosedGameBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "pc";
            AddPlayer("p1");
            AddDirection("d");
            AddTile("a").WithRelationTo("b").InDirection("d");
            AddTile("b");
            AddPiece("piece-1").OnTile("a").HasDirection("d").DoesNotRepeat();
            // Phase condition always false => no active phase.
            AddGamePhase("never active")
                .If(game => new NullGameStateCondition(false))
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If(game => new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Valid))
                        .Then()
                            .Do<MovePieceStateMutator>();
        }
    }
    private class RuleRejectedGameBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "rr";
            AddPlayer("p1");
            AddDirection("d");
            AddTile("a").WithRelationTo("b").InDirection("d");
            AddTile("b");
            AddPiece("piece-1").OnTile("a").HasDirection("d").DoesNotRepeat();
            AddGamePhase("invalid move phase")
                .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If(game => new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Invalid))
                        .Then()
                            .Do<MovePieceStateMutator>();
        }
    }

    private class DiceMismatchGameBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "dm";
            AddPlayer("p1");
            AddDirection("d");
            AddTile("a").WithRelationTo("b").InDirection("d");
            AddTile("b");
            AddPiece("piece-1").OnTile("a").HasDirection("d").CanRepeat();
            AddDice("d1").HasValue(4);
            AddDice("d2").HasValue(5);
            AddGamePhase("move phase")
                .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If(game => new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Valid))
                        .Then()
                            .Do(game => new ClearDiceStateMutator(game.GetArtifacts<Dice>("d1", "d2")))
                            .Do<MovePieceStateMutator>();
        }
    }

    private class NoMoveRuleGameBuilder : GameBuilder
    {
        public class NoOpGameEvent : IGameEvent { }

        private class NoOpMutator : IStateMutator<NoOpGameEvent>
        {
            public GameState MutateState(GameEngine engine, GameState state, NoOpGameEvent @event)
            {
                return state; // intentionally no change
            }
        }
        protected override void Build()
        {
            BoardId = "nm";
            AddPlayer("p1");
            AddDirection("d");
            AddTile("a").WithRelationTo("b").InDirection("d");
            AddTile("b");
            AddPiece("piece-1").OnTile("a").HasDirection("d").DoesNotRepeat();
            // Provide a rule for a custom no-op event whose mutator returns the same state reference => NotApplicable.
            AddGamePhase("noop phase")
                .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<NoOpGameEvent>()
                        .If(game => new SimpleGameEventCondition<NoOpGameEvent>((eng, s, e) => ConditionResponse.Valid))
                        .Then()
                            .Do<NoOpMutator>();
        }
    }
    [Fact]
    public void HandleEventResult_Should_return_Accepted_on_state_change()
    {
        // arrange
        var progress = new TestGameBuilder(useSimpleGamePhase: false).Compile();
        var game = progress.Game;
        var piece = game.GetPiece("piece-1");
        var state = progress.State.GetState<PieceState>(piece);
        var toTile = game.GetTile("tile-2");
        var path = new TilePath([new TileRelation(state.CurrentTile, toTile, Direction.Clockwise)]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = progress.HandleEventResult(evt);

        // assert
        result.Applied.Should().BeTrue();
        result.Reason.Should().Be(EventRejectionReason.None);
        result.State.Should().NotBe(progress.State);
        result.State.GetState<PieceState>(piece).CurrentTile.Should().Be(toTile);
    }

    [Fact]
    public void HandleEventResult_Should_return_NotApplicable_when_no_state_change()
    {
        // arrange
        var progress = new NoMoveRuleGameBuilder().Compile();
        var evt = new NoMoveRuleGameBuilder.NoOpGameEvent();

        // act
        var result = progress.HandleEventResult(evt);

        // assert
        result.Applied.Should().BeFalse();
        result.Reason.Should().Be(EventRejectionReason.NotApplicable);
        result.State.Should().Be(progress.State);
    }

    [Fact]
    public void HandleEventResult_Should_return_InvalidOwnership_on_invalid_from_tile()
    {
        // arrange
        var progress = new TestGameBuilder(useSimpleGamePhase: false).Compile();
        var game = progress.Game;
        var piece = game.GetPiece("piece-1");
        var wrongFrom = game.GetTile("tile-2");
        var toTile = game.GetTile("tile-1");
        var path = new TilePath([new TileRelation(wrongFrom, toTile, Direction.CounterClockwise)]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = progress.HandleEventResult(evt);

        // assert
        result.Applied.Should().BeFalse();
        result.Reason.Should().Be(EventRejectionReason.InvalidOwnership);
        result.Message.Should().Contain("Invalid from tile");
    }

    [Fact]
    public void HandleEventResult_Should_return_RuleRejected_on_invalid_condition()
    {
        // arrange
        var progress = new RuleRejectedGameBuilder().Compile();
        var game = progress.Game;
        var piece = game.GetPiece("piece-1");
        var from = game.GetTile("a");
        var to = game.GetTile("b");
        var path = new TilePath([new TileRelation(from, to, Direction.Clockwise)]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = progress.HandleEventResult(evt);

        // assert
        result.Applied.Should().BeFalse();
        result.Reason.Should().Be(EventRejectionReason.RuleRejected);
    }

    [Fact]
    public void HandleEventResult_Should_return_PathNotFound_on_dice_mismatch()
    {
        // arrange
        var progress = new DiceMismatchGameBuilder().Compile();
        var game = progress.Game;
        var piece = game.GetPiece("piece-1");
        var from = game.GetTile("a");
        var to = game.GetTile("b");
        var path = new TilePath([new TileRelation(from, to, Direction.Clockwise)]); // distance 1 not matching dice 4/5
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = progress.HandleEventResult(evt);

        // assert
        result.Applied.Should().BeFalse();
        result.Reason.Should().Be(EventRejectionReason.PathNotFound);
    }

    [Fact]
    public void HandleEventResult_Should_return_PhaseClosed_when_no_active_phase()
    {
        // arrange
        var progress = new PhaseClosedGameBuilder().Compile();
        var piece = progress.Game.GetPiece("piece-1");
        // Valid single-step path (phase still closed so it won't apply)
        var from = progress.Game.GetTile("a");
        var to = progress.Game.GetTile("b");
        var path = new TilePath([new TileRelation(from, to, Direction.Clockwise)]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = progress.HandleEventResult(evt);

        // assert
        result.Applied.Should().BeFalse();
        result.Reason.Should().Be(EventRejectionReason.PhaseClosed);
    }

    private class InvalidEventGameBuilder : GameBuilder
    {
        private class ThrowingMutator : IStateMutator<MovePieceGameEvent>
        {
            public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
            {
                throw new BoardException("Unmapped mutator failure");
            }
        }
        protected override void Build()
        {
            BoardId = "ie";
            AddPlayer("p1");
            AddDirection("d");
            AddTile("a").WithRelationTo("b").InDirection("d");
            AddTile("b");
            AddPiece("piece-1").OnTile("a").HasDirection("d").DoesNotRepeat();
            AddGamePhase("throwing phase")
                .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If(game => new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Valid))
                        .Then()
                            .Do<ThrowingMutator>();
        }
    }

    [Fact]
    public void HandleEventResult_Should_return_InvalidEvent_on_unmapped_board_exception()
    {
        // arrange
        var progress = new InvalidEventGameBuilder().Compile();
        var piece = progress.Game.GetPiece("piece-1");
        var from = progress.Game.GetTile("a");
        var to = progress.Game.GetTile("b");
        var path = new TilePath([new TileRelation(from, to, Direction.Clockwise)]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = progress.HandleEventResult(evt);

        // assert
        result.Applied.Should().BeFalse();
        result.Reason.Should().Be(EventRejectionReason.InvalidEvent);
        result.Message.Should().Contain("Unmapped mutator failure");
    }
}