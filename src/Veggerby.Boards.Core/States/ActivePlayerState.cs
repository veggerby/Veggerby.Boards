using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

public class ActivePlayerState(Player player, bool isActive) : ArtifactState<Player>(player)
{
    public bool IsActive { get; } = isActive;

    public override bool Equals(object obj)
    {
        return Equals(obj as ActivePlayerState);
    }

    public override bool Equals(IArtifactState other)
    {
        return Equals(other as ActivePlayerState);
    }

    public bool Equals(ActivePlayerState other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && IsActive.Equals(other.IsActive);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, IsActive);
}