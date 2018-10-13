using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class HasValidPathGameEventCondition : IGameEventCondition<MovePieceGameEvent>
    {
        public ConditionResponse Evaluate(GameState state, MovePieceGameEvent @event)
        {
            var result = @event
                .Piece
                .Patterns
                .Select(pattern => {
                    var visitor = new ResolveTilePathPatternVisitor(state.Game.Board, @event.From, @event.To);
                    pattern.Accept(visitor);
                    return visitor.ResultPath;
                })
                .Any(x => x != null);

            return result ? ConditionResponse.Valid : ConditionResponse.Invalid;
        }
    }
}