using System.Linq;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Rules
{
    public class RuleExtensionsTests
    {
        public class And
        {
            [Fact]
            public void Should_create_composition_with_type_all()
            {
                // arrange
                var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator());
                var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.Valid, null, new DiceStateMutator<int>());

                // act
                var actual = rule1.And(rule2);

                // assert
                actual.ShouldBeOfType<CompositeGameEventRule>();
                (actual as CompositeGameEventRule).CompositeMode.ShouldBe(CompositeMode.All);
                (actual as CompositeGameEventRule).Rules.ShouldBe(new [] { rule1, rule2 });
            }

            [Fact]
            public void Should_create_composition_with_type_all_when_chained()
            {
                // arrange
                var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator());
                var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.Valid, null, new DiceStateMutator<int>());
                var rule3 = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid);

                // act
                var actual = rule1.And(rule2).And(rule3);

                // assert
                actual.ShouldBeOfType<CompositeGameEventRule>();
                (actual as CompositeGameEventRule).CompositeMode.ShouldBe(CompositeMode.All);
                (actual as CompositeGameEventRule).Rules.ShouldBe(new [] { rule1, rule2, rule3 });
            }

            [Fact]
            public void Should_not_chain_composition()
            {
                // arrange
                var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator());
                var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.Valid, null, new DiceStateMutator<int>());
                var rule3 = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid);

                // act
                var actual = rule1.Or(rule2).And(rule3);

                // assert
                actual.ShouldBeOfType<CompositeGameEventRule>();
                (actual as CompositeGameEventRule).CompositeMode.ShouldBe(CompositeMode.All);
                (actual as CompositeGameEventRule)
                    .Rules
                    .OfType<CompositeGameEventRule>()
                    .ShouldAllBe(x => x.CompositeMode == CompositeMode.Any);
            }
        }

        public class Or
        {
            [Fact]
            public void Should_create_composition_with_type_or()
            {
                // arrange
                var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator());
                var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.Valid, null, new DiceStateMutator<int>());

                // act
                var actual = rule1.Or(rule2);

                // assert
                actual.ShouldBeOfType<CompositeGameEventRule>();
                (actual as CompositeGameEventRule).CompositeMode.ShouldBe(CompositeMode.Any);
                (actual as CompositeGameEventRule).Rules.ShouldBe(new [] { rule1, rule2 });
            }

            [Fact]
            public void Should_create_composition_with_type_any_when_chained()
            {
                // arrange
                var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator());
                var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.Valid, null, new DiceStateMutator<int>());
                var rule3 = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid);

                // act
                var actual = rule1.Or(rule2).Or(rule3);

                // assert
                actual.ShouldBeOfType<CompositeGameEventRule>();
                (actual as CompositeGameEventRule).CompositeMode.ShouldBe(CompositeMode.Any);
                (actual as CompositeGameEventRule).Rules.ShouldBe(new [] { rule1, rule2, rule3 });
            }

            [Fact]
            public void Should_not_chain_composition()
            {
                // arrange
                var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(() => RuleCheckState.Valid, null, new PieceStateMutator());
                var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(() => RuleCheckState.Valid, null, new DiceStateMutator<int>());
                var rule3 = SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid);

                // act
                var actual = rule1.And(rule2).Or(rule3);

                // assert
                actual.ShouldBeOfType<CompositeGameEventRule>();
                (actual as CompositeGameEventRule).CompositeMode.ShouldBe(CompositeMode.Any);
                (actual as CompositeGameEventRule)
                    .Rules
                    .OfType<CompositeGameEventRule>()
                    .ShouldAllBe(x => x.CompositeMode == CompositeMode.All);
            }
        }
    }
}