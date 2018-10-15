using System;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Phases
{
    // TESTS BOTH GamePhase and CompositGamePhase
    public class GamePhaseTests
    {
        public class New
        {
            [Fact]
            public void Should_instantiate_new_gamephase()
            {
                // arrange
                var condition = new NullGameStateCondition();
                var parent = GamePhase.NewParent(1);
                var rule = GameEventRule<IGameEvent>.Null;

                // act
                var actual = GamePhase.New(1, condition, rule, parent);

                // assert
                actual.ShouldNotBeNull();
                actual.Number.ShouldBe(1);
                actual.Condition.ShouldBe(condition);
                actual.Rule.ShouldBe(rule);
                actual.Parent.ShouldBe(parent);
                parent.ChildPhases.ShouldContain(actual);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-10)]
            public void Should_throw_with_non_positive_number(int number)
            {
                // arrange
                var condition = new NullGameStateCondition();
                var parent = GamePhase.NewParent(1);

                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => GamePhase.New(number, condition, GameEventRule<IGameEvent>.Null, parent));

                // assert
                actual.ParamName.ShouldBe("number");
            }

            [Fact]
            public void Should_throw_with_null_condition()
            {
                // arrange
                var parent = GamePhase.NewParent(1);

                // act
                var actual = Should.Throw<ArgumentNullException>(() => GamePhase.New(1, null, GameEventRule<IGameEvent>.Null, parent));

                // assert
                actual.ParamName.ShouldBe("condition");
            }


            [Fact]
            public void Should_throw_with_null_rule()
            {
                // arrange
                var parent = GamePhase.NewParent(1);

                // act
                var actual = Should.Throw<ArgumentNullException>(() => GamePhase.New(1, new NullGameStateCondition(), null, parent));

                // assert
                actual.ParamName.ShouldBe("rule");
            }
        }

        public class GetActiveGamePhase
        {
            private readonly GameState _initialGameState;

            public GetActiveGamePhase()
            {
                _initialGameState = GameState.New(null);
            }

            [Fact]
            public void Should_return_simple_gamephase()
            {
                // arrange
                var child = GamePhase.New(1, new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null);

                // act
                var actual = child.GetActiveGamePhase(_initialGameState);

                // assert
                actual.ShouldBe(child);
            }

            [Fact]
            public void Should_return_null_simple_gamephase_condition_false()
            {
                // arrange
                var child = GamePhase.New(1, new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null);

                // act
                var actual = child.GetActiveGamePhase(_initialGameState);

                // assert
                actual.ShouldBeNull();
            }

            [Fact]
            public void Should_return_simple_gamephase_child()
            {
                // arrange
                var parent = GamePhase.NewParent(1);
                var child = GamePhase.New(1, new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, parent);

                // act
                var actual = parent.GetActiveGamePhase(_initialGameState);

                // assert
                actual.ShouldBe(child);
            }

            [Fact]
            public void Should_return_null_simple_gamephase_child_condition_is_fakse()
            {
                // arrange
                var parent = GamePhase.NewParent(1);
                var child = GamePhase.New(1, new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, parent);

                // act
                var actual = parent.GetActiveGamePhase(_initialGameState);

                // assert
                actual.ShouldBeNull();
            }

            [Fact]
            public void Should_return_child_with_valid_condition()
            {
                // arrange
                var parent = GamePhase.NewParent(1);
                var child1 = GamePhase.New(1, new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, parent);
                var child2 = GamePhase.New(2, new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, parent);

                // act
                var actual = parent.GetActiveGamePhase(_initialGameState);

                // assert
                actual.ShouldBe(child2);
            }

            [Fact]
            public void Should_return_correct_child_more_complex_phase_hierarchy()
            {
                // arrange
                var root = GamePhase.NewParent(1);
                var group1 = GamePhase.NewParent(1, null, root);
                var group2 = GamePhase.NewParent(2, new NullGameStateCondition(false), root);
                var group3 = GamePhase.NewParent(3, new InitialGameStateCondition(), root);

                var child11 = GamePhase.New(1, new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, group1);
                var child12 = GamePhase.New(2, new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, group1);

                var child21 = GamePhase.New(1, new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, group2);

                var child31 = GamePhase.New(1, new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, group3);
                var child32 = GamePhase.New(1, new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, group3);

                // act
                var actual = root.GetActiveGamePhase(_initialGameState);

                // assert
                actual.ShouldBe(child32);
            }
        }
    }
}