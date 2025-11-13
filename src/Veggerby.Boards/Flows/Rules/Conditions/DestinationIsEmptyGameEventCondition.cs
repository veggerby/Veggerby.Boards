using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Valid only when the destination tile of a <see cref="MovePieceGameEvent"/> contains no material pieces.
/// </summary>
/// <remarks>
/// Provides an explicit emptiness guard for normal moves so that capture semantics remain unambiguous:
/// capture rule (opponent present) is evaluated first; normal move rule demands the destination is empty.
/// </remarks>
public sealed class DestinationIsEmptyGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var any = state.GetPiecesOnTile(@event.To).Any();
        return any ? ConditionResponse.Ignore("Destination occupied") : ConditionResponse.Valid;
    }
}