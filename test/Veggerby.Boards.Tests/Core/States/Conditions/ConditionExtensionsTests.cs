using System.Linq;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States.Conditions
{
    public class ConditionExtensionsTests
    {
        public class And
        {
            [Fact]
            public void Should_create_composition_with_type_all()
            {
                // arrange
                var dice = new RegularDice("dice");
                var condition1 = new InitialStateGamePhaseCondition();
                var condition2 = new HasDiceStateGameStateCondition<RegularDice, int>(new[] { dice });

                // act
                var actual = condition1.And(condition2);

                // assert
                actual.ShouldBeOfType<CompositeGameStateCondition>();
                (actual as CompositeGameStateCondition).CompositeMode.ShouldBe(CompositeMode.All);
                (actual as CompositeGameStateCondition).ChildConditions.ShouldBe(new IGameStateCondition[] { condition1, condition2 });
            }

            [Fact]
            public void Should_create_composition_with_type_all_when_chained()
            {
                // arrange
                var dice = new RegularDice("dice");
                var condition1 = new InitialStateGamePhaseCondition();
                var condition2 = new HasDiceStateGameStateCondition<RegularDice, int>(new[] { dice });
                var condition3 = new SimpleGameStateCondition();

                // act
                var actual = condition1.And(condition2).And(condition3);

                // assert
                actual.ShouldBeOfType<CompositeGameStateCondition>();
                (actual as CompositeGameStateCondition).CompositeMode.ShouldBe(CompositeMode.All);
                (actual as CompositeGameStateCondition).ChildConditions.ShouldBe(new IGameStateCondition[] { condition1, condition2, condition3 });
            }

            [Fact]
            public void Should_not_chain_composition()
            {
                // arrange
                var dice = new RegularDice("dice");
                var condition1 = new InitialStateGamePhaseCondition();
                var condition2 = new HasDiceStateGameStateCondition<RegularDice, int>(new[] { dice });
                var condition3 = new SimpleGameStateCondition();

                // act
                var actual = condition1.Or(condition2).And(condition3);

                // assert
                actual.ShouldBeOfType<CompositeGameStateCondition>();
                (actual as CompositeGameStateCondition).CompositeMode.ShouldBe(CompositeMode.All);
                (actual as CompositeGameStateCondition)
                    .ChildConditions
                    .OfType<CompositeGameStateCondition>()
                    .ShouldAllBe(x => x.CompositeMode == CompositeMode.Any);
            }
        }

        public class Or
        {
            [Fact]
            public void Should_create_composition_with_type_or()
            {
                // arrange
                var dice = new RegularDice("dice");
                var condition1 = new InitialStateGamePhaseCondition();
                var condition2 = new HasDiceStateGameStateCondition<RegularDice, int>(new[] { dice });

                // act
                var actual = condition1.Or(condition2);

                // assert
                actual.ShouldBeOfType<CompositeGameStateCondition>();
                (actual as CompositeGameStateCondition).CompositeMode.ShouldBe(CompositeMode.Any);
                (actual as CompositeGameStateCondition).ChildConditions.ShouldBe(new IGameStateCondition[] { condition1, condition2 });
            }

            [Fact]
            public void Should_create_composition_with_type_any_when_chained()
            {
                // arrange
                var dice = new RegularDice("dice");
                var condition1 = new InitialStateGamePhaseCondition();
                var condition2 = new HasDiceStateGameStateCondition<RegularDice, int>(new[] { dice });
                var condition3 = new SimpleGameStateCondition();

                // act
                var actual = condition1.Or(condition2).Or(condition3);

                // assert
                actual.ShouldBeOfType<CompositeGameStateCondition>();
                (actual as CompositeGameStateCondition).CompositeMode.ShouldBe(CompositeMode.Any);
                (actual as CompositeGameStateCondition).ChildConditions.ShouldBe(new IGameStateCondition[] { condition1, condition2, condition3 });
            }

            [Fact]
            public void Should_not_chain_composition()
            {
                // arrange
                var dice = new RegularDice("dice");
                var condition1 = new InitialStateGamePhaseCondition();
                var condition2 = new HasDiceStateGameStateCondition<RegularDice, int>(new[] { dice });
                var condition3 = new SimpleGameStateCondition();

                // act
                var actual = condition1.And(condition2).Or(condition3);

                // assert
                actual.ShouldBeOfType<CompositeGameStateCondition>();
                (actual as CompositeGameStateCondition).CompositeMode.ShouldBe(CompositeMode.Any);
                (actual as CompositeGameStateCondition)
                    .ChildConditions
                    .OfType<CompositeGameStateCondition>()
                    .ShouldAllBe(x => x.CompositeMode == CompositeMode.All);
            }
        }
    }
}