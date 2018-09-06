using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.States.Conditions
{
    public class DiceGameStateCondition<TDice, TValue> : IGameStateCondition
        where TDice : Dice<TValue>
    {
        public IEnumerable<TDice> Dice { get; }

        public DiceGameStateCondition(IEnumerable<TDice> dice)
        {
            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            if (!dice.Any())
            {
                throw new ArgumentException("Dice list cannot be empty", nameof(dice));
            }

            Dice = dice.Distinct().ToList().AsReadOnly();
        }

        public bool Evaluate(GameState state)
        {
            var rolledDice = Dice
                .Select(x => state.GetState<DiceState<TValue>>(x))
                .Where(x => x != null && !EqualityComparer<TValue>.Default.Equals(x.CurrentValue, default(TValue)))
                .Select(x => x.Artifact)
                .ToList();

            return !Dice.Except(rolledDice).Any();
        }
    }
}