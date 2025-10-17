using System;


using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Flows.Phases;

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
            var actual = GamePhase.New(1, "test", condition, rule, parent);

            // assert
            actual.Should().NotBeNull();
            actual.Number.Should().Be(1);
            actual.Condition.Should().Be(condition);
            actual.Rule.Should().Be(rule);
            actual.Parent.Should().Be(parent);
            parent.ChildPhases.Should().Contain(actual);
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
            var actual = () => GamePhase.New(number, "test", condition, GameEventRule<IGameEvent>.Null, parent);

            // assert
            actual.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("number");
        }

        [Fact]
        public void Should_throw_with_null_condition()
        {
            // arrange
            var parent = GamePhase.NewParent(1);

            // act
            var actual = () => GamePhase.New(1, "test", null!, GameEventRule<IGameEvent>.Null, parent);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("condition");
        }


        [Fact]
        public void Should_throw_with_null_rule()
        {
            // arrange
            var parent = GamePhase.NewParent(1);

            // act
            var actual = () => GamePhase.New(1, "test", new NullGameStateCondition(), null!, parent);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("rule");
        }
    }

    public class GetActiveGamePhase
    {
        private readonly GameState _initialGameState;

        public GetActiveGamePhase()
        {
            _initialGameState = GameState.New(Array.Empty<IArtifactState>());
        }

        [Fact]
        public void Should_return_simple_gamephase()
        {
            // arrange
            var child = GamePhase.New(1, "test", new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null);

            // act
            var actual = child.GetActiveGamePhase(_initialGameState);

            // assert
            actual.Should().Be(child);
        }

        [Fact]
        public void Should_return_null_simple_gamephase_condition_false()
        {
            // arrange
            var child = GamePhase.New(1, "test", new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null);

            // act
            var actual = child.GetActiveGamePhase(_initialGameState);

            // assert
            actual.Should().BeNull();
        }

        [Fact]
        public void Should_return_simple_gamephase_child()
        {
            // arrange
            var parent = GamePhase.NewParent(1);
            var child = GamePhase.New(1, "test", new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, parent);

            // act
            var actual = parent.GetActiveGamePhase(_initialGameState);

            // assert
            actual.Should().Be(child);
        }

        [Fact]
        public void Should_return_null_simple_gamephase_child_condition_is_fakse()
        {
            // arrange
            var parent = GamePhase.NewParent(1);
            var child = GamePhase.New(1, "test", new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, parent);

            // act
            var actual = parent.GetActiveGamePhase(_initialGameState);

            // assert
            actual.Should().BeNull();
        }

        [Fact]
        public void Should_return_child_with_valid_condition()
        {
            // arrange
            var parent = GamePhase.NewParent(1);
            var child1 = GamePhase.New(1, "test 1", new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, parent);
            var child2 = GamePhase.New(2, "test 2", new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, parent);

            // act
            var actual = parent.GetActiveGamePhase(_initialGameState);

            // assert
            actual.Should().Be(child2);
        }

        [Fact]
        public void Should_return_correct_child_more_complex_phase_hierarchy()
        {
            // arrange
            var root = GamePhase.NewParent(1);
            var group1 = GamePhase.NewParent(1, "test 1", null, root);
            var group2 = GamePhase.NewParent(2, "test 2", new NullGameStateCondition(false), root);
            var group3 = GamePhase.NewParent(3, "test 3", new InitialGameStateCondition(), root);

            var child11 = GamePhase.New(1, "test 4", new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, group1);
            var child12 = GamePhase.New(2, "test 5", new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, group1);

            var child21 = GamePhase.New(1, "test 6", new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, group2);

            var child31 = GamePhase.New(1, "test 7", new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, group3);
            var child32 = GamePhase.New(1, "test 8", new NullGameStateCondition(true), GameEventRule<IGameEvent>.Null, group3);

            // act
            var actual = root.GetActiveGamePhase(_initialGameState);

            // assert
            actual.Should().Be(child32);
        }
    }
}