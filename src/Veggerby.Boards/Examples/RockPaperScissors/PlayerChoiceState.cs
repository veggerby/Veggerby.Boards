using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.RockPaperScissors;

/// <summary>
/// Tracks a player's choice in Rock-Paper-Scissors.
/// </summary>
public sealed class PlayerChoiceState : ArtifactState<Player>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerChoiceState"/> class.
    /// </summary>
    /// <param name="player">The player who made the choice.</param>
    /// <param name="choice">The choice made by the player.</param>
    public PlayerChoiceState(Player player, Choice choice)
        : base(player)
    {
        Choice = choice;
    }

    /// <summary>
    /// Gets the choice made by the player.
    /// </summary>
    public Choice Choice { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as PlayerChoiceState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState? other)
    {
        return Equals(other as PlayerChoiceState);
    }

    /// <summary>
    /// Determines equality with another <see cref="PlayerChoiceState"/>.
    /// </summary>
    /// <param name="other">Other state.</param>
    /// <returns><c>true</c> if both reference the same player and have the same choice.</returns>
    public bool Equals(PlayerChoiceState? other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && Choice == other.Choice;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Artifact, Choice);
    }
}
