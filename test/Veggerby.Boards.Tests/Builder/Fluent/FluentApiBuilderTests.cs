using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Fluent;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Builder.Fluent;

public class FluentApiBuilderTests
{
    // Test game event and conditions for testing
    private class TestGameEvent : IGameEvent
    {
    }

    private class TestCondition1 : IGameEventCondition<TestGameEvent>
    {
        public ConditionResponse Evaluate(GameEngine engine, GameState state, TestGameEvent @event)
        {
            return ConditionResponse.Valid;
        }
    }

    private class TestCondition2 : IGameEventCondition<TestGameEvent>
    {
        public ConditionResponse Evaluate(GameEngine engine, GameState state, TestGameEvent @event)
        {
            return ConditionResponse.Valid;
        }
    }

    private class TestMutator : IStateMutator<TestGameEvent>
    {
        public GameState MutateState(GameEngine engine, GameState state, TestGameEvent @event)
        {
            return state;
        }
    }

    // Simple builder to test DefineRules
    private class TestFluentGameBuilder : GameBuilder
    {
        public bool DefineRulesCalled
        {
            get; private set;
        }
        public bool OnEventCalled
        {
            get; private set;
        }
        public bool WhenConditionCalled
        {
            get; private set;
        }
        public bool ExecuteMutatorsCalled
        {
            get; private set;
        }

        protected override void Build()
        {
            BoardId = "test-fluent";

            AddPlayer("player-1");
            AddPlayer("player-2");

            AddDirection("north");

            AddTile("tile-1");
            AddTile("tile-2");

            WithTile("tile-1")
                .WithRelationTo("tile-2")
                .InDirection("north");

            AddGamePhase("test-phase")
                .If<NullGameStateCondition>()
                .DefineRules(phase =>
                {
                    DefineRulesCalled = true;

                    phase.On<TestGameEvent>(evt =>
                    {
                        OnEventCalled = true;

                        evt.When<TestCondition1>()
                            .And<TestCondition2>()
                            .Execute(mutators =>
                            {
                                ExecuteMutatorsCalled = true;
                                mutators.Apply<TestMutator>();
                            });
                    });
                });
        }
    }

    public class DefineRules_Tests
    {
        [Fact]
        public void Should_call_DefineRules_lambda()
        {
            // arrange
            var builder = new TestFluentGameBuilder();

            // act
            var result = builder.Compile();

            // assert
            result.Should().NotBeNull();
            builder.DefineRulesCalled.Should().BeTrue("DefineRules lambda should be invoked");
        }

        [Fact]
        public void Should_call_On_event_handler_lambda()
        {
            // arrange
            var builder = new TestFluentGameBuilder();

            // act
            var result = builder.Compile();

            // assert
            result.Should().NotBeNull();
            builder.OnEventCalled.Should().BeTrue("On event lambda should be invoked");
        }

        [Fact]
        public void Should_call_Execute_mutators_lambda()
        {
            // arrange
            var builder = new TestFluentGameBuilder();

            // act
            var result = builder.Compile();

            // assert
            result.Should().NotBeNull();
            builder.ExecuteMutatorsCalled.Should().BeTrue("Execute mutators lambda should be invoked");
        }

        [Fact]
        public void Should_build_valid_game_with_fluent_API()
        {
            // arrange
            var builder = new TestFluentGameBuilder();

            // act
            var result = builder.Compile();

            // assert
            result.Should().NotBeNull();
            result.Game.Should().NotBeNull();
            result.State.Should().NotBeNull();
            result.Engine.Should().NotBeNull();
        }
    }

    public class ConditionGroup_Tests
    {
        [Fact]
        public void Should_create_condition_group()
        {
            // arrange
            var group = ConditionGroup<TestGameEvent>.Create()
                .Require<TestCondition1>()
                .Require<TestCondition2>();

            // act
            var conditions = group.GetConditions().ToList();

            // assert
            conditions.Should().NotBeNull();
            conditions.Should().HaveCount(2);
        }

        [Fact]
        public void Should_apply_condition_group_with_With_method()
        {
            // arrange
            var group = ConditionGroup<TestGameEvent>.Create()
                .Require<TestCondition1>()
                .Require<TestCondition2>();

            var builder = new TestFluentWithConditionGroupGameBuilder(group);

            // act
            var result = builder.Compile();

            // assert
            result.Should().NotBeNull();
            builder.ConditionGroupApplied.Should().BeTrue();
        }

        private class TestFluentWithConditionGroupGameBuilder : GameBuilder
        {
            private readonly ConditionGroup<TestGameEvent> _group;
            public bool ConditionGroupApplied
            {
                get; private set;
            }

            public TestFluentWithConditionGroupGameBuilder(ConditionGroup<TestGameEvent> group)
            {
                _group = group;
            }

            protected override void Build()
            {
                BoardId = "test-fluent-group";

                AddPlayer("player-1");
                AddPlayer("player-2");

                AddDirection("north");

                AddTile("tile-1");
                AddTile("tile-2");

                WithTile("tile-1")
                    .WithRelationTo("tile-2")
                    .InDirection("north");

                AddGamePhase("test-phase")
                    .If<NullGameStateCondition>()
                    .DefineRules(phase =>
                    {
                        phase.On<TestGameEvent>(evt =>
                        {
                            ConditionGroupApplied = true;
                            evt.With(_group)
                                .Execute(mutators =>
                                {
                                    mutators.Apply<TestMutator>();
                                });
                        });
                    });
            }
        }
    }

    public class When_Conditional_Tests
    {
        [Fact]
        public void Should_include_rules_when_condition_is_true()
        {
            // arrange
            var builder = new TestFluentConditionalGameBuilder(includeRules: true);

            // act
            var result = builder.Compile();

            // assert
            result.Should().NotBeNull();
            builder.ConditionalRulesIncluded.Should().BeTrue();
        }

        [Fact]
        public void Should_exclude_rules_when_condition_is_false()
        {
            // arrange
            var builder = new TestFluentConditionalGameBuilder(includeRules: false);

            // act
            var result = builder.Compile();

            // assert
            result.Should().NotBeNull();
            builder.ConditionalRulesIncluded.Should().BeFalse();
        }

        private class TestFluentConditionalGameBuilder : GameBuilder
        {
            private readonly bool _includeRules;
            public bool ConditionalRulesIncluded
            {
                get; private set;
            }

            public TestFluentConditionalGameBuilder(bool includeRules)
            {
                _includeRules = includeRules;
            }

            protected override void Build()
            {
                BoardId = "test-fluent-conditional";

                AddPlayer("player-1");
                AddPlayer("player-2");

                AddDirection("north");

                AddTile("tile-1");
                AddTile("tile-2");

                WithTile("tile-1")
                    .WithRelationTo("tile-2")
                    .InDirection("north");

                AddGamePhase("test-phase")
                    .If<NullGameStateCondition>()
                    .DefineRules(phase =>
                    {
                        phase.When(_includeRules, p =>
                        {
                            ConditionalRulesIncluded = true;
                            p.On<TestGameEvent>(evt =>
                            {
                                evt.When<TestCondition1>()
                                    .Execute(mutators =>
                                    {
                                        mutators.Apply<TestMutator>();
                                    });
                            });
                        });
                    });
            }
        }
    }
}
