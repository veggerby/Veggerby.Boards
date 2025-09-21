namespace Veggerby.Boards.Core.States;

/// <summary>
/// Defines a rule predicate that can be evaluated against a <see cref="GameState"/> producing a validity response.
/// </summary>
/// <remarks>
/// Conditions are pure (side-effect free) and should execute deterministically based solely on the provided state.
/// </remarks>
public interface IGameStateCondition
{
    /// <summary>
    /// Evaluates the condition for the supplied <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The game state.</param>
    /// <returns>A <see cref="ConditionResponse"/> indicating outcome.</returns>
    ConditionResponse Evaluate(GameState state);
}