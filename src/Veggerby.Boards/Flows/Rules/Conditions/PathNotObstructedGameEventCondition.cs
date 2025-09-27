using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Validates that every intermediate tile (excluding origin and destination) on a movement path is empty.
/// </summary>
/// <remarks>
/// Applies to sliding style movement (rook, bishop, queen in chess). Knights and single-step non-repeating moves
/// naturally have no intermediate tiles and therefore always pass. Destination tile occupancy (capture rules) is
/// intentionally ignored here and should be handled by a separate condition or mutator logic when capture semantics
/// are introduced.
/// </remarks>
public sealed class PathNotObstructedGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var (blocked, tileId) = MovementPathValidator.IsPathObstructed(state, @event);
        return blocked
            ? ConditionResponse.Ignore($"Path blocked at {tileId}")
            : ConditionResponse.Valid;
    }
}