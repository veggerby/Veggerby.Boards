using System;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Rules
{
    public class SimpleGameEventRuleTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize()
            {
                // arrange
                // act
                var actual = SimpleGameEventRule<NullGameEvent>.New((s, e) => ConditionResponse.Valid);

                // assert
                actual.ShouldNotBeNull();
            }

            [Fact]
            public void Should_throw_with_null_handler()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentNullException>(() => SimpleGameEventRule<NullGameEvent>.New(null));

                // assert
                actual.ParamName.ShouldBe("handler");
            }
        }

        public class Check
        {
            [Fact]
            public void Should_return_valid_check_state()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var rule = SimpleGameEventRule<NullGameEvent>.New((s, e) => ConditionResponse.Valid);

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.ShouldBe(ConditionResponse.Valid);
            }

            [Fact]
            public void Should_return_ignore_check_state()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var rule = SimpleGameEventRule<NullGameEvent>.New((s, e) => ConditionResponse.Ignore("just because"));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(ConditionResult.Ignore);
                actual.Reason.ShouldBe("just because");
            }

            [Fact]
            public void Should_throw_with_null_event_on_explicit_interface()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New((s, e) => ConditionResponse.Valid, null, new DiceStateMutator<int>());

                // act
                var actual = Should.Throw<ArgumentNullException>(() => rule.Check(engine.GameState, null));

                // assert
                actual.ParamName.ShouldBe("event");
            }

            [Fact]
            public void Should_return_ignore_check_state_with_different_event_type_on_explicit_interface()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New((s, e) => ConditionResponse.Valid, null, new DiceStateMutator<int>());

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(ConditionResult.Ignore);
            }
        }

        public class HandleEvent
        {
            [Fact]
            public void Should_update_state()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, from, to);
                var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                    (s, e) => ConditionResponse.Valid,
                    null,
                    new PieceStateMutator());

                // act
                var actual = rule.HandleEvent(initialState, @event);

                // assert
                var newPieceState = actual.GetState<PieceState>(piece);
                newPieceState.CurrentTile.ShouldBe(to);
            }

            [Fact]
            public void Should_not_mutate_state_when_rule_result_is_ignore()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, from, to);
                var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                    (s, e) => ConditionResponse.NotApplicable,
                    null,
                    new PieceStateMutator());

                // act
                var actual = rule.HandleEvent(initialState, @event);

                // assert
                actual.ShouldBe(initialState);
                var newPieceState = actual.GetState<PieceState>(piece);
                newPieceState.CurrentTile.ShouldBe(from);
            }

            [Fact]
            public void Should_throw_if_handle_event_called_with_invalid_rule_result()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, from, to);
                var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                    (s, e) => ConditionResponse.Invalid,
                    null,
                    new PieceStateMutator());

                // act
                var actual = Should.Throw<BoardException>(() => rule.HandleEvent(initialState, @event));

                // assert
                actual.Message.ShouldBe("Invalid game event");
            }


            [Fact]
            public void Should_throw_if_handle_null_event()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var rule = SimpleGameEventRule<MovePieceGameEvent>.New(
                    (s, e) => ConditionResponse.Invalid,
                    null,
                    new PieceStateMutator());

                // act
                var actual = Should.Throw<ArgumentNullException>(() => rule.HandleEvent(initialState, null));

                // assert
                actual.ParamName.ShouldBe("event");
            }

            [Fact]
            public void Should_return_same_state()
            {
                // arrange
                var game = new TestGameEngineBuilder().Compile().Game;
                var state = GameState.New(game, null);
                var rule = SimpleGameEventRule<NullGameEvent>.New((s, e) => ConditionResponse.Valid);

                // act
                var actual = rule.HandleEvent(state, new NullGameEvent());

                // assert
                actual.ShouldBe(state);
            }

            [Fact]
            public void Should_throw_with_null_event_on_explicit_interface()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var state = GameState.New(engine.Game, null);
                IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New((s, e) => ConditionResponse.Valid, null, new DiceStateMutator<int>());

                // act
                var actual = Should.Throw<ArgumentNullException>(() => rule.HandleEvent(state, null));

                // assert
                actual.ParamName.ShouldBe("event");
            }

            [Fact]
            public void Should_return_ignore_check_state_with_different_event_type_on_explicit_interface()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var state = GameState.New(engine.Game, null);
                IGameEventRule rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New((s, e) => ConditionResponse.Valid, null, new DiceStateMutator<int>());

                // act
                var actual = rule.HandleEvent(state, new NullGameEvent());

                // assert
                actual.ShouldBeSameAs(state);
            }
        }
    }
}