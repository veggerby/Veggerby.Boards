using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Strategy interface for producing dice values.
/// </summary>
public interface IDiceValueGenerator<T>
{
    /// <summary>
    /// Generates a new value for the dice (current state may inform result).
    /// </summary>
    T GetValue(IArtifactState currentState);
}