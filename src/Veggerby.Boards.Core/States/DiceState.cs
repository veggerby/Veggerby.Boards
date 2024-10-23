using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

public class DiceState<T> : ArtifactState<Dice>
{
    public T CurrentValue { get; }

    public DiceState(Dice dice, T currentValue) : base(dice)
    {
        if (currentValue is null)
        {
            throw new ArgumentNullException(nameof(currentValue));
        }

        CurrentValue = currentValue;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as DiceState<T>);
    }

    public override bool Equals(IArtifactState other)
    {
        return Equals(other as DiceState<T>);
    }

    public bool Equals(DiceState<T> other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && CurrentValue.Equals(other.CurrentValue);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, CurrentValue);
}