using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Permits scoring iff no existing <see cref="ScoreState"/> present.
/// </summary>
public sealed class ComputeScoresEventCondition : IGameEventCondition<ComputeScoresEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, ComputeScoresEvent @event)
    {
        // If any ScoreState already present treat as Ignore (idempotent) so other events can proceed.
        foreach (var _ in state.GetStates<ScoreState>())
        {
            return ConditionResponse.Ignore("Scores already computed");
        }
        return ConditionResponse.Valid;
    }
}