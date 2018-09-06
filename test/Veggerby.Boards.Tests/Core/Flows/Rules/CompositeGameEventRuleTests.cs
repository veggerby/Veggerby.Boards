using System;
using System.Linq;
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
    public class CompositeGameEventRuleTests
    {
        public class ctor
        {
            [Fact]
            public void Should_instatiate_composite_rule()
            {
                // arrange
                var rule1 = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid);
                var rule2 = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid);

                // act
                var actual = rule1.And(rule2);

                // assert
                actual.ShouldBeOfType<CompositeGameEventRule>();
                (actual as CompositeGameEventRule).CompositeMode.ShouldBe(CompositeMode.All);
                (actual as CompositeGameEventRule).Rules.ShouldBe(new [] { rule1, rule2 });
            }
        }
        public class Check
        {
            [Fact]
            public void Should_return_valid_all_rules()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid)
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid))
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_valid_all_rules_when_one_ignore()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid)
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.NotApplicable))
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_invalid_all_rules_when_one_fails()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid)
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason")))
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason")));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("a reason,yet another reason");
            }

            [Fact]
            public void Should_return_invalid_all_rules_when_all_fail()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason"))
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason")));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("a reason,yet another reason");
            }

            [Fact]
            public void Should_return_valid_any_rules()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid)
                    .Or(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid))
                    .Or(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_valid_any_rules_when_one_fails()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid)
                    .Or(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason")))
                    .Or(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason")));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_invalid_any_rules_when_all_fail()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason") )
                    .Or(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason") ));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("a reason,yet another reason");
            }

            [Fact]
            public void Should_return_ignore_when_all_rules_are_ignore_mode_all()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.NotApplicable)
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.NotApplicable))
                    .And(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.NotApplicable));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Ignore);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_ignore_when_all_rules_are_ignore_mode_any()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.NotApplicable)
                    .Or(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.NotApplicable))
                    .Or(SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.NotApplicable));

                // act
                var actual = rule.Check(engine.GameState, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Ignore);
                actual.Reason.ShouldBeNull();
            }
        }

        public class HandleEvent
        {
            [Fact]
            public void Sould_handle_simple_event_mode_all()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, from, to);

                var rule = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid)
                    .And(SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator()));

                // act
                var actual = rule.HandleEvent(initialState, @event);

                // assert
                actual.ShouldNotBeSameAs(initialState);
                var newPieceState = actual.GetState<PieceState>(piece);
                newPieceState.CurrentTile.ShouldBe(to);
            }

            [Fact]
            public void Sould_handle_simple_event_mode_any()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, from, to);

                var rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.Valid, null, new DiceStateMutator<int>())
                    .Or(SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator()));

                // act
                var actual = rule.HandleEvent(initialState, @event);

                // assert
                actual.ShouldNotBeSameAs(initialState);
                var newPieceState = actual.GetState<PieceState>(piece);
                newPieceState.CurrentTile.ShouldBe(to);
            }

            [Fact]
            public void Sould_ignore_simple_event_mode_all()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, from, to);

                var rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.NotApplicable, null, new DiceStateMutator<int>())
                    .And(SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.NotApplicable, null, new PieceStateMutator()));

                // act
                var actual = rule.HandleEvent(initialState, @event);

                // assert
                actual.ShouldBeSameAs(initialState);
            }

            [Fact]
            public void Sould_aggregate_game_states()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to1 = game.GetTile("tile-2");
                var to2 = game.GetTile("tile-3");
                var dice = game.GetArtifact<RegularDice>("dice");
                var @event = new MovePieceGameEvent(piece, from, to1);

                var rule = SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator())
                    .And(SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new SimpleGameStateMutator<MovePieceGameEvent>(x => new DiceState<int>(dice, 3))))
                    .And(SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new SimpleGameStateMutator<MovePieceGameEvent>(x => new PieceState(piece, to2))));

                // act
                var actual = rule.HandleEvent(initialState, @event);

                // assert
                actual.ShouldNotBeSameAs(initialState);
                actual.IsInitialState.ShouldBeFalse();

                var pieceState = actual.GetState<PieceState>(piece);
                var diceState = actual.GetState<DiceState<int>>(dice);

                pieceState.CurrentTile.ShouldBe(to2);
                diceState.CurrentValue.ShouldBe(3);
            }

            [Fact]
            public void Sould_throw_when_event_mode_all()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.GameState;
                var piece = game.GetPiece("piece-1");
                var from = game.GetTile("tile-1");
                var to = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, from, to);

                var rule = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.NotApplicable, null, new DiceStateMutator<int>())
                    .And(SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Invalid, null, new PieceStateMutator()));

                // act
                var actual = Should.Throw<BoardException>(() => rule.HandleEvent(initialState, @event));

                // assert
                actual.Message.ShouldBe("Invalid game event");
            }
        }
    }
}