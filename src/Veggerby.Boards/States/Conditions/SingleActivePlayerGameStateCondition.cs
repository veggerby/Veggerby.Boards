using System.Linq;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Condition that is valid only when exactly one player is marked active.
/// </summary>
/// <remarks>
/// Ensures turn-based integrity by preventing ambiguous active player states (zero or multiple active players).
/// </remarks>
public class SingleActivePlayerGameStateCondition : IGameStateCondition
{
    /// <summary>
    /// Evaluates whether exactly one <see cref="ActivePlayerState"/> has <c>IsActive</c> set.
    /// </summary>
    /// <param name="state">The game state.</param>
    /// <returns><see cref="ConditionResponse.Valid"/> when exactly one active player is present; otherwise <see cref="ConditionResponse.Invalid"/>.</returns>
    public ConditionResponse Evaluate(GameState state)
    {
        return state.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive) is not null
            ? ConditionResponse.Valid
            : ConditionResponse.Invalid;
    }
}