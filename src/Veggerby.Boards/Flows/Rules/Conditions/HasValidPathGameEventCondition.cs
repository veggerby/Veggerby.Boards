using System.Linq;


using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Validates that a movement event follows at least one legal pattern-defined path.
/// </summary>
public class HasValidPathGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var pieceState = state.GetState<PieceState>(@event.Piece);

        if (!pieceState.CurrentTile.Equals(@event.From))
        {
            return ConditionResponse.Invalid;
        }

        var result = @event
            .Piece
            .Patterns
            .Select(pattern =>
            {
                var visitor = new ResolveTilePathPatternVisitor(engine.Game.Board, @event.From, @event.To);
                pattern.Accept(visitor);
                return visitor.ResultPath;
            })
            .Any(x => x is not null);

        return result ? ConditionResponse.Valid : ConditionResponse.Fail("No path to destination tile");
    }
}