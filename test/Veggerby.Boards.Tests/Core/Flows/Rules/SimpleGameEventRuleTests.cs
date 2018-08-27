using System;
using Shouldly;
using Veggerby.Boards.Core.Flows.Events;
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
                var actual = new SimpleGameEventRule((state, @event) => RuleCheckState.Valid);

                // assert
                actual.ShouldNotBeNull();
            }

            [Fact]
            public void Should_throw_with_null_handler()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentNullException>(() => new SimpleGameEventRule(null));

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
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null);
                var rule = new SimpleGameEventRule((s, e) => RuleCheckState.Valid);

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.ShouldBe(RuleCheckState.Valid);
            }

            [Fact]
            public void Should_return_ignore_check_state()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null);
                var rule = new SimpleGameEventRule((s, e) => RuleCheckState.Ignore("just because"));

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Ignore);
                actual.Reason.ShouldBe("just because");
            }
        }

        public class HandleEvent
        {
            [Fact]
            public void Should_return_same_state()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null);
                var rule = new SimpleGameEventRule((s, e) => RuleCheckState.Valid);

                // act
                var actual = rule.HandleEvent(state, new NullGameEvent());

                // assert
                actual.ShouldBe(state);
            }
        }
    }
}