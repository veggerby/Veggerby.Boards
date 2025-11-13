using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.States.Conditions;

public class ConditionExtensionsTests
{
    public class And
    {
        [Fact]
        public void Should_create_composition_with_type_all()
        {
            // arrange

            // act

            // assert

            var dice = new Dice("dice");
            var condition1 = new InitialGameStateCondition();
            var condition2 = new DiceGameStateCondition<int>([dice], CompositeMode.All);

            // act
            var actual = condition1.And(condition2);

            // assert
            actual.Should().BeOfType<CompositeGameStateCondition>();
            var composite = actual as CompositeGameStateCondition;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.All);
            composite.ChildConditions.Should().Equal([condition1, condition2]);
        }

        [Fact]
        public void Should_create_composition_with_type_all_when_chained()
        {
            // arrange

            // act

            // assert

            var dice = new Dice("dice");
            var condition1 = new InitialGameStateCondition();
            var condition2 = new DiceGameStateCondition<int>([dice], CompositeMode.All);
            var condition3 = new NullGameStateCondition();

            // act
            var actual = condition1.And(condition2).And(condition3);

            // assert
            actual.Should().BeOfType<CompositeGameStateCondition>();
            var composite = actual as CompositeGameStateCondition;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.All);
            composite.ChildConditions.Should().Equal([condition1, condition2, condition3]);
        }

        [Fact]
        public void Should_not_chain_composition()
        {
            // arrange

            // act

            // assert

            var dice = new Dice("dice");
            var condition1 = new InitialGameStateCondition();
            var condition2 = new DiceGameStateCondition<int>([dice], CompositeMode.All);
            var condition3 = new NullGameStateCondition();

            // act
            var actual = condition1.Or(condition2).And(condition3);

            // assert
            actual.Should().BeOfType<CompositeGameStateCondition>();
            var composite = actual as CompositeGameStateCondition;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.All);
            composite.ChildConditions
                .OfType<CompositeGameStateCondition>()
                .Should().OnlyContain(x => x.CompositeMode == CompositeMode.Any);
        }
    }

    public class Or
    {
        [Fact]
        public void Should_create_composition_with_type_or()
        {
            // arrange

            // act

            // assert

            var dice = new Dice("dice");
            var condition1 = new InitialGameStateCondition();
            var condition2 = new DiceGameStateCondition<int>([dice], CompositeMode.All);

            // act
            var actual = condition1.Or(condition2);

            // assert
            actual.Should().BeOfType<CompositeGameStateCondition>();
            var composite = actual as CompositeGameStateCondition;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.Any);
            composite.ChildConditions.Should().Equal([condition1, condition2]);
        }

        [Fact]
        public void Should_create_composition_with_type_any_when_chained()
        {
            // arrange

            // act

            // assert

            var dice = new Dice("dice");
            var condition1 = new InitialGameStateCondition();
            var condition2 = new DiceGameStateCondition<int>([dice], CompositeMode.All);
            var condition3 = new NullGameStateCondition();

            // act
            var actual = condition1.Or(condition2).Or(condition3);

            // assert
            actual.Should().BeOfType<CompositeGameStateCondition>();
            var composite = actual as CompositeGameStateCondition;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.Any);
            composite.ChildConditions.Should().Equal([condition1, condition2, condition3]);
        }

        [Fact]
        public void Should_not_chain_composition()
        {
            // arrange

            // act

            // assert

            var dice = new Dice("dice");
            var condition1 = new InitialGameStateCondition();
            var condition2 = new DiceGameStateCondition<int>([dice], CompositeMode.All);
            var condition3 = new NullGameStateCondition();

            // act
            var actual = condition1.And(condition2).Or(condition3);

            // assert
            actual.Should().BeOfType<CompositeGameStateCondition>();
            var composite = actual as CompositeGameStateCondition;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.Any);
            composite.ChildConditions
                .OfType<CompositeGameStateCondition>()
                .Should().OnlyContain(x => x.CompositeMode == CompositeMode.All);
        }
    }
}
