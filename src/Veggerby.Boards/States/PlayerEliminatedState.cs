using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable marker indicating a player has been eliminated from the game.
/// </summary>
/// <remarks>
/// Used by elimination-based victory conditions (e.g., last player standing).
/// The elimination reason provides context (e.g., "Bankruptcy", "Defeated", "Eliminated").
/// </remarks>
public sealed class PlayerEliminatedState : ArtifactState<Player>
{
    /// <summary>
    /// Gets the player that was eliminated.
    /// </summary>
    public Player Player => Artifact;

    /// <summary>
    /// Gets the reason for elimination.
    /// </summary>
    public string Reason
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerEliminatedState"/> class.
    /// </summary>
    /// <param name="player">The eliminated player.</param>
    /// <param name="reason">The reason for elimination (e.g., "Bankruptcy", "Defeated").</param>
    public PlayerEliminatedState(Player player, string reason) : base(player)
    {
        Reason = reason ?? string.Empty;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PlayerEliminatedState);

    /// <inheritdoc />
    public override bool Equals(IArtifactState? other) => Equals(other as PlayerEliminatedState);

    private bool Equals(PlayerEliminatedState? other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && Reason == other.Reason;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, Reason);
}
