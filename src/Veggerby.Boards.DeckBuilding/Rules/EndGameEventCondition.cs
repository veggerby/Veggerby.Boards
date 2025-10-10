using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Allows <see cref="EndGameEvent"/> when a <see cref="ScoreState"/> has been produced (scoring completed) and no prior <see cref="GameEndedState"/> exists.
/// Additionally enforces a configurable max turn threshold (inclusive) if a TurnState is present.
/// </summary>
public sealed class EndGameEventCondition : IGameEventCondition<EndGameEvent>
{
    private const int MaxTurns = 1; // minimal MVP threshold

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, EndGameEvent @event)
    {
        // Already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return ConditionResponse.Ignore("Already ended");
        }
        // Require scores first
        if (!state.GetStates<ScoreState>().Any())
        {
            return ConditionResponse.Ignore("Scores not computed yet");
        }
        // If a turn state exists, ensure threshold reached
        var turn = state.GetStates<TurnState>().FirstOrDefault();
        if (turn != null && turn.TurnNumber < MaxTurns)
        {
            return ConditionResponse.Ignore("Max turns not reached");
        }
        return ConditionResponse.Valid;
    }
}