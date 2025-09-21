namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Trivial condition that always yields the configured result.
/// </summary>
/// <remarks>
/// Useful as a sentinel or placeholder when constructing phase graphs or composite conditions prior to adding
/// a meaningful evaluation implementation.
/// </remarks>
public class NullGameStateCondition(bool result) : IGameStateCondition
{
    private readonly bool _result = result;

    /// <summary>
    /// Initializes a new instance that always evaluates to <c>true</c>.
    /// </summary>
    public NullGameStateCondition() : this(true) // needed for initializer
    {
    }

    /// <summary>
    /// Evaluates the condition.
    /// </summary>
    /// <param name="state">The game state (unused).</param>
    /// <returns>The configured <see cref="ConditionResponse"/>.</returns>
    public ConditionResponse Evaluate(GameState state)
    {
        return _result ? ConditionResponse.Valid : ConditionResponse.Invalid;
    }
}