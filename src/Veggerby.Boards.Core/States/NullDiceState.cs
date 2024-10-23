using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

public class NullDiceState(Dice dice) : ArtifactState<Dice>(dice)
{
    public override bool Equals(object obj)
    {
        return Equals(obj as NullDiceState);
    }

    public override bool Equals(IArtifactState other)
    {
        return Equals(other as NullDiceState);
    }

    public bool Equals(NullDiceState other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact);
}