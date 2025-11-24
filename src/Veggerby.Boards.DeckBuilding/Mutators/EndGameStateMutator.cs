using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.DeckBuilding.States;
namespace Veggerby.Boards.DeckBuilding.Mutators;

/// <summary>
/// Appends a <see cref="GameEndedState"/> marker and <see cref="DeckBuildingOutcomeState"/> when an <see cref="EndGameEvent"/> is processed.
/// </summary>
public sealed class EndGameStateMutator : IStateMutator<EndGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, EndGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Idempotent: if already ended, no change
        foreach (var _ in state.GetStates<GameEndedState>())
        {
            return state;
        }

        // Collect all score states
        var scores = state.GetStates<ScoreState>().ToList();

        // Create outcome state from scores
        var outcomeState = new DeckBuildingOutcomeState(scores);

        return state.Next([new GameEndedState(), outcomeState]);
    }
}