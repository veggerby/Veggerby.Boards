using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Tests.Core.Flows.Rules;

public class RuleExtensionsTests
{
    public class And
    {
        [Fact]
        public void Should_create_composition_with_type_all()
        {
            // arrange

            // act

            // assert

            var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(new SimpleGameEventCondition<MovePieceGameEvent>((eng, state, @event) => ConditionResponse.Valid), null, new MovePieceStateMutator());
            var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, state, @event) => ConditionResponse.Valid), null, new DiceStateMutator<int>());

            // act
            var actual = rule1.And(rule2);

            // assert
            actual.Should().BeOfType<CompositeGameEventRule>();
            var composite = actual as CompositeGameEventRule;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.All);
            composite.Rules.Should().Equal([rule1, rule2]);
        }

        [Fact]
        public void Should_create_composition_with_type_all_when_chained()
        {
            // arrange

            // act

            // assert

            var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(new SimpleGameEventCondition<MovePieceGameEvent>((eng, state, @event) => ConditionResponse.Valid), null, new MovePieceStateMutator());
            var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, state, @event) => ConditionResponse.Valid), null, new DiceStateMutator<int>());
            var rule3 = GameEventRule<IGameEvent>.Null;

            // act
            var actual = rule1.And(rule2).And(rule3);

            // assert
            actual.Should().BeOfType<CompositeGameEventRule>();
            var composite = actual as CompositeGameEventRule;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.All);
            composite.Rules.Should().Equal([rule1, rule2, rule3]);
        }

        [Fact]
        public void Should_not_chain_composition()
        {
            // arrange

            // act

            // assert

            var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(new SimpleGameEventCondition<MovePieceGameEvent>((eng, state, @event) => ConditionResponse.Valid), null, new MovePieceStateMutator());
            var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, state, @event) => ConditionResponse.Valid), null, new DiceStateMutator<int>());
            var rule3 = GameEventRule<IGameEvent>.Null;

            // act
            var actual = rule1.Or(rule2).And(rule3);

            // assert
            actual.Should().BeOfType<CompositeGameEventRule>();
            var composite = actual as CompositeGameEventRule;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.All);
            composite.Rules
                .OfType<CompositeGameEventRule>()
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

            var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(new SimpleGameEventCondition<MovePieceGameEvent>((eng, state, @event) => ConditionResponse.Valid), null, new MovePieceStateMutator());
            var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, state, @event) => ConditionResponse.Valid), null, new DiceStateMutator<int>());

            // act
            var actual = rule1.Or(rule2);

            // assert
            actual.Should().BeOfType<CompositeGameEventRule>();
            var composite = actual as CompositeGameEventRule;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.Any);
            composite.Rules.Should().Equal([rule1, rule2]);
        }

        [Fact]
        public void Should_create_composition_with_type_any_when_chained()
        {
            // arrange

            // act

            // assert

            var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(new SimpleGameEventCondition<MovePieceGameEvent>((eng, state, @event) => ConditionResponse.Valid), null, new MovePieceStateMutator());
            var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, state, @event) => ConditionResponse.Valid), null, new DiceStateMutator<int>());
            var rule3 = GameEventRule<IGameEvent>.Null;

            // act
            var actual = rule1.Or(rule2).Or(rule3);

            // assert
            actual.Should().BeOfType<CompositeGameEventRule>();
            var composite = actual as CompositeGameEventRule;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.Any);
            composite.Rules.Should().Equal([rule1, rule2, rule3]);
        }

        [Fact]
        public void Should_not_chain_composition()
        {
            // arrange

            // act

            // assert

            var rule1 = SimpleGameEventRule<MovePieceGameEvent>.New(new SimpleGameEventCondition<MovePieceGameEvent>((eng, state, @event) => ConditionResponse.Valid), null, new MovePieceStateMutator());
            var rule2 = SimpleGameEventRule<RollDiceGameEvent<int>>.New(new SimpleGameEventCondition<RollDiceGameEvent<int>>((eng, state, @event) => ConditionResponse.Valid), null, new DiceStateMutator<int>());
            var rule3 = GameEventRule<IGameEvent>.Null;

            // act
            var actual = rule1.And(rule2).Or(rule3);

            // assert
            actual.Should().BeOfType<CompositeGameEventRule>();
            var composite = actual as CompositeGameEventRule;
            composite.Should().NotBeNull();
            composite!.CompositeMode.Should().Be(CompositeMode.Any);
            composite.Rules
                .OfType<CompositeGameEventRule>()
                .Should().OnlyContain(x => x.CompositeMode == CompositeMode.All);
        }
    }
}
