using System;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.Flows.Rules;

public class SimpleGameEventRuleTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize()
        {
            // arrange
            // act
            var actual = SimpleGameEventRule<NullGameEvent>.New(new SimpleGameEventCondition<NullGameEvent>((eng, s, e) => ConditionResponse.Valid));

            // assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void Should_throw_with_null_condition()
        {
            // arrange
            // act
            var actual = () => SimpleGameEventRule<NullGameEvent>.New((IGameEventCondition<NullGameEvent>)null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("condition");
        }
    }

    public class Check
    {
        [Fact]
        public void Should_return_valid_check_state()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var rule = GameEventRule<NullGameEvent>.Null; ;

            // act
            var actual = rule.Check(engine.Engine, engine.State, new NullGameEvent());

            // assert
            actual.Should().Be(ConditionResponse.Valid);
        }

        [Fact]
        public void Should_return_ignore_check_state()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var rule = SimpleGameEventRule<NullGameEvent>.New(new SimpleGameEventCondition<NullGameEvent>((eng, s, e) => ConditionResponse.Ignore("just because")));

            // act
            var actual = rule.Check(engine.Engine, engine.State, new NullGameEvent());

            // assert
            actual.Result.Should().Be(ConditionResult.Ignore);
            actual.Reason.Should().Be("just because");
        }

        [Fact]
        public void Should_throw_with_null_event_on_explicit_interface()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, s, e) => ConditionResponse.Valid), null, new DiceStateMutator<int>());

            // act
            var actual = () => rule.Check(engine.Engine, engine.State, (IGameEvent)null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("@event");
        }

        [Fact]
        public void Should_return_ignore_check_state_with_different_event_type_on_explicit_interface()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, s, e) => ConditionResponse.Valid), null, new DiceStateMutator<int>());

            // act
            var actual = rule.Check(engine.Engine, engine.State, new NullGameEvent());

            // assert
            actual.Result.Should().Be(ConditionResult.Ignore);
        }
    }

    public class HandleEvent
    {
        [Fact]
        public void Should_update_state()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var piece = game.GetPiece("piece-1").EnsureNotNull();
            var from = game.GetTile("tile-1").EnsureNotNull();
            var to = game.GetTile("tile-2").EnsureNotNull();
            var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            piece.Patterns.Single().Accept(visitor);
            var path = visitor.ResultPath;
            path.Should().NotBeNull();
            var @event = new MovePieceGameEvent(piece, path!);
            var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Valid),
                null,
                new MovePieceStateMutator());

            // act
            var actual = rule.HandleEvent(engine.Engine, initialState, @event);

            // assert
            var newPieceState = actual.GetState<PieceState>(piece).EnsureNotNull();
            newPieceState.CurrentTile.Should().Be(to);
        }

        [Fact]
        public void Should_not_mutate_state_when_rule_result_is_ignore()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var piece = game.GetPiece("piece-1").EnsureNotNull();
            var from = game.GetTile("tile-1").EnsureNotNull();
            var to = game.GetTile("tile-2").EnsureNotNull();
            var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            piece.Patterns.Single().Accept(visitor);
            var path = visitor.ResultPath;
            path.Should().NotBeNull();
            var @event = new MovePieceGameEvent(piece, path!);
            var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.NotApplicable),
                null,
                new MovePieceStateMutator());

            // act
            var actual = rule.HandleEvent(engine.Engine, initialState, @event);

            // assert
            actual.Should().Be(initialState);
            var newPieceState = actual.GetState<PieceState>(piece).EnsureNotNull();
            newPieceState.CurrentTile.Should().Be(from);
        }

        [Fact]
        public void Should_throw_if_handle_event_called_with_invalid_rule_result()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var piece = game.GetPiece("piece-1").EnsureNotNull();
            var from = game.GetTile("tile-1").EnsureNotNull();
            var to = game.GetTile("tile-2").EnsureNotNull();
            var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            piece.Patterns.Single().Accept(visitor);
            var path = visitor.ResultPath;
            path.Should().NotBeNull();
            var @event = new MovePieceGameEvent(piece, path!);
            var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Invalid),
                null,
                new MovePieceStateMutator());

            // act
            var actual = () => rule.HandleEvent(engine.Engine, initialState, @event);

            // assert
            actual.Should().Throw<BoardException>().WithMessage("*Invalid game event*");
        }


        [Fact]
        public void Should_throw_if_handle_null_event()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var piece = game.GetPiece("piece-1").EnsureNotNull();
            var from = game.GetTile("tile-1").EnsureNotNull();
            var to = game.GetTile("tile-2").EnsureNotNull();
            var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => ConditionResponse.Invalid),
                null,
                new MovePieceStateMutator());

            // act
            var actual = () => rule.HandleEvent(engine.Engine, initialState, (MovePieceGameEvent)null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("@event");
        }

        [Fact]
        public void Should_return_same_state()
        {
            // arrange
            var state = GameState.New(Array.Empty<IArtifactState>());
            var rule = SimpleGameEventRule<NullGameEvent>.New(new SimpleGameEventCondition<NullGameEvent>((eng, s, e) => ConditionResponse.Valid));
            var engine = new TestGameBuilder().Compile();

            // act
            var actual = rule.HandleEvent(engine.Engine, state, new NullGameEvent());

            // assert
            actual.Should().Be(state);
        }

        [Fact]
        public void Should_throw_with_null_event_on_explicit_interface()
        {
            // arrange
            var state = GameState.New(Array.Empty<IArtifactState>());
            IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, s, e) => ConditionResponse.Valid), null, new DiceStateMutator<int>());
            var engine = new TestGameBuilder().Compile();

            // act
            var actual = () => rule.HandleEvent(engine.Engine, state, (IGameEvent)null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("@event");
        }

        [Fact]
        public void Should_return_ignore_check_state_with_different_event_type_on_explicit_interface()
        {
            // arrange
            var state = GameState.New(Array.Empty<IArtifactState>());
            IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, s, e) => ConditionResponse.Valid), null, new DiceStateMutator<int>());
            var engine = new TestGameBuilder().Compile();

            // act
            var actual = rule.HandleEvent(engine.Engine, state, new NullGameEvent());

            // assert
            actual.Should().Be(state);
        }
    }
}