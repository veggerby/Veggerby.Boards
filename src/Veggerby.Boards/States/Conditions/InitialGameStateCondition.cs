namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Condition that is valid only for the initial (root) <see cref="GameState"/>.
/// </summary>
/// <remarks>
/// This is typically used to gate phase transitions or rule execution that must only occur once at the start of a game.
/// </remarks>
public class InitialGameStateCondition : IGameStateCondition
{
    /// <summary>
    /// Evaluates the current game state.
    /// </summary>
    /// <param name="state">The game state instance.</param>
    /// <returns><see cref="ConditionResponse.Valid"/> when <see cref="GameState.IsInitialState"/> is true; otherwise <see cref="ConditionResponse.Invalid"/>.</returns>
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.IsInitialState ? ConditionResponse.Valid : ConditionResponse.Invalid;
    }
}