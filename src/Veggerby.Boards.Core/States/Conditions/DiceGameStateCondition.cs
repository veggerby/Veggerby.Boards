using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.States.Conditions
{
    public class DiceGameStateCondition<TValue> : IGameStateCondition
    {
        public IEnumerable<Dice> Dice { get; }
        public CompositeMode Mode { get; }

        public DiceGameStateCondition(IEnumerable<Dice> dice, CompositeMode mode)
        {
            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            if (!dice.Any())
            {
                throw new ArgumentException("Dice list cannot be empty", nameof(dice));
            }

            if (dice.Any(x => x == null))
            {
                throw new ArgumentException("All dice must be non null", nameof(dice));
            }

            Dice = dice.Distinct().ToList().AsReadOnly();
            Mode = mode;
        }

        public ConditionResponse Evaluate(GameState state)
        {
            var rolledDice = Dice
                .Select(x => state.GetState<DiceState<TValue>>(x))
                .Where(x => x != null && !EqualityComparer<TValue>.Default.Equals(x.CurrentValue, default(TValue)))
                .Select(x => x.Artifact)
                .ToList();

            var result = Mode == CompositeMode.All
                ? Dice.All(x => rolledDice.Contains(x))
                : Dice.Any(x => rolledDice.Contains(x));

            return result ? ConditionResponse.Valid : ConditionResponse.Invalid;
        }
    }
}