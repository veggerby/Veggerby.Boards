using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Tracks whether a given <see cref="Player"/> is currently active (whose turn it is) within a <see cref="GameState"/>.
/// </summary>
public class ActivePlayerState(Player player, bool isActive) : ArtifactState<Player>(player)
{
    /// <summary>
    /// Gets a value indicating whether the associated player is active.
    /// </summary>
    public bool IsActive { get; } = isActive;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as ActivePlayerState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState other)
    {
        return Equals(other as ActivePlayerState);
    }

    /// <summary>
    /// Determines equality with another <see cref="ActivePlayerState"/> considering player identity and active flag.
    /// </summary>
    /// <param name="other">Other state.</param>
    /// <returns><c>true</c> if both reference the same player and have identical activity; otherwise false.</returns>
    public bool Equals(ActivePlayerState? other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && IsActive.Equals(other.IsActive);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, IsActive);
}