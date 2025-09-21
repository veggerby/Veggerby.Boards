using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

/// <summary>
/// Represents the state of a single die artifact including its current rolled value.
/// </summary>
/// <typeparam name="T">The value type produced by the die.</typeparam>
/// <remarks>
/// Dice states are immutable snapshots. A new <see cref="DiceState{T}"/> instance is produced when a die is rolled again
/// and incorporated into a subsequent <see cref="GameState"/>.
/// </remarks>
public class DiceState<T> : ArtifactState<Dice>
{
    /// <summary>
    /// Gets the current (rolled) value for the die.
    /// </summary>
    public T CurrentValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiceState{T}"/> class.
    /// </summary>
    /// <param name="dice">The die artifact.</param>
    /// <param name="currentValue">The current value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentValue"/> is null.</exception>
    public DiceState(Dice dice, T currentValue) : base(dice)
    {
        if (currentValue is null)
        {
            throw new ArgumentNullException(nameof(currentValue));
        }

        CurrentValue = currentValue;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as DiceState<T>);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState other)
    {
        return Equals(other as DiceState<T>);
    }

    /// <summary>
    /// Determines equality with another dice state.
    /// </summary>
    /// <param name="other">The other dice state.</param>
    /// <returns><c>true</c> when artifact and value match; otherwise <c>false</c>.</returns>
    public bool Equals(DiceState<T> other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && CurrentValue.Equals(other.CurrentValue);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, CurrentValue);
}