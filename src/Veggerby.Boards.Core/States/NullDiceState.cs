using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

/// <summary>
/// Sentinel dice state representing a die with no rolled value.
/// </summary>
/// <remarks>
/// This can be used to model an uninitialized die before its first roll while still participating in
/// state comparisons and artifact change calculations.
/// </remarks>
public class NullDiceState(Dice dice) : ArtifactState<Dice>(dice)
{
    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as NullDiceState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState other)
    {
        return Equals(other as NullDiceState);
    }

    /// <summary>
    /// Determines equality with another null dice state.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <returns><c>true</c> if artifacts match; otherwise <c>false</c>.</returns>
    public bool Equals(NullDiceState other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact);
}