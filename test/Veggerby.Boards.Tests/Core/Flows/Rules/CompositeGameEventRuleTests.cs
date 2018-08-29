using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;
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
                var subrules = new []
                {
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid),
                };

                // act
                var actual = new CompositeGameEventRule(subrules, CompositeMode.All);

                // assert
                actual.CompositeMode.ShouldBe(CompositeMode.All);
                actual.Rules.ShouldBe(subrules);
            }

            [Fact]
            public void Should_throw_null_rules()
            {
                // arrange

                // act
                var rule = Should.Throw<ArgumentNullException>(() => new CompositeGameEventRule(null, CompositeMode.All));

                // assert
                rule.ParamName.ShouldBe("rules");
            }

            [Fact]
            public void Should_throw_empty_rules()
            {
                // arrange

                // act
                var rule = Should.Throw<ArgumentException>(() => new CompositeGameEventRule(Enumerable.Empty<IGameEventRule<IGameEvent>>(), CompositeMode.All));

                // assert
                rule.ParamName.ShouldBe("rules");
            }
        }
        public class Check
        {
            [Fact]
            public void Should_return_valid_all_rules()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = new TestInitialGameStateBuilder().Compile(game);

                var subrules = new []
                {
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid),
                };

                var rule = new CompositeGameEventRule(subrules, CompositeMode.All);

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_invalid_all_rules_when_one_fails()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = new TestInitialGameStateBuilder().Compile(game);

                var subrules = new []
                {
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason")),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason")),
                };

                var rule = new CompositeGameEventRule(subrules, CompositeMode.All);

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("a reason,yet another reason");
            }

            [Fact]
            public void Should_return_invalid_all_rules_when_all_fail()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = new TestInitialGameStateBuilder().Compile(game);

                var subrules = new []
                {
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason") ),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason") ),
                };

                var rule = new CompositeGameEventRule(subrules, CompositeMode.All);

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("a reason,yet another reason");
            }

            [Fact]
            public void Should_return_valid_any_rules()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = new TestInitialGameStateBuilder().Compile(game);

                var subrules = new []
                {
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid ),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid ),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid ),
                };

                var rule = new CompositeGameEventRule(subrules, CompositeMode.Any);

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_valid_any_rules_when_one_fails()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = new TestInitialGameStateBuilder().Compile(game);

                var subrules = new []
                {
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid ),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason") ),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason") ),
                };

                var rule = new CompositeGameEventRule(subrules, CompositeMode.Any);

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Valid);
                actual.Reason.ShouldBeNull();
            }

            [Fact]
            public void Should_return_invalid_any_rules_when_all_fail()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = new TestInitialGameStateBuilder().Compile(game);

                var subrules = new []
                {
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("a reason") ),
                    SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Fail("yet another reason") ),
                };

                var rule = new CompositeGameEventRule(subrules, CompositeMode.Any);

                // act
                var actual = rule.Check(state, new NullGameEvent());

                // assert
                actual.Result.ShouldBe(RuleCheckResult.Invalid);
                actual.Reason.ShouldBe("a reason,yet another reason");
            }
        }
    }
}