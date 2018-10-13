using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class HasValidPathWithDiceStateGameEventCondition : IGameEventCondition<MovePieceGameEvent>
    {
        public HasValidPathWithDiceStateGameEventCondition(params Dice[] dice)
        {
            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            if (!dice.Any())
            {
                throw new ArgumentException(nameof(dice));
            }

            Dice = dice;
        }

        public Dice[] Dice { get; }

        public ConditionResponse Evaluate(GameState state, MovePieceGameEvent @event)
        {
            var states = Dice.Select(x => state.GetState<DiceState<int>>(x)).Where(x => x != null);

            var paths = @event
                .Piece
                .Patterns
                .Select(pattern => {
                    var visitor = new ResolveTilePathPatternVisitor(state.Game.Board, @event.From, @event.To);
                    pattern.Accept(visitor);
                    return visitor.ResultPath;
                })
                .Where(x => x != null)
                .ToList();

            return ConditionResponse.Invalid;
        }
    }
}