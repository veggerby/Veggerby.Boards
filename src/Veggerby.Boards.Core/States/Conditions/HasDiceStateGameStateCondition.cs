using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.States.Conditions
{
    public class HasDiceStateGameStateCondition<TDice, TValue> : IGameStateCondition
        where TDice : Dice<TValue>
    {
        private IEnumerable<TDice> _dice;

        public HasDiceStateGameStateCondition(IEnumerable<TDice> dice)
        {
            if (dice == null)
            {
                throw new System.ArgumentNullException(nameof(dice));
            }

            _dice = dice;
        }

        public bool Evaluate(GameState state)
        {
            var rolledDice = _dice
                .Select(x => state.GetState(x))
                .OfType<DiceState<TValue>>()
                .Where(x => !EqualityComparer<TValue>.Default.Equals(x.CurrentValue, default(TValue)))
                .Select(x => x.Artifact)
                .ToList();

            return !_dice.Except(rolledDice).Any();
        }
    }
}