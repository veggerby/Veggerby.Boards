using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Visibility policy that filters states based on piece ownership and visibility level.
/// </summary>
/// <remarks>
/// This policy is designed for imperfect-information games where players should only see:
/// <list type="bullet">
/// <item><description>Public states (visible to all)</description></item>
/// <item><description>Private states they own (e.g., their hand cards, hidden units)</description></item>
/// </list>
/// Hidden states are not visible to anyone, including the owner, until explicitly revealed.
/// </remarks>
public sealed class PlayerOwnedVisibilityPolicy : IVisibilityPolicy
{
    /// <summary>
    /// Gets the singleton instance of the player-owned visibility policy.
    /// </summary>
    public static PlayerOwnedVisibilityPolicy Instance { get; } = new PlayerOwnedVisibilityPolicy();

    private PlayerOwnedVisibilityPolicy()
    {
    }

    /// <inheritdoc />
    public bool CanSee(Player? viewer, IArtifactState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Public states are visible to all
        if (state.Visibility == Visibility.Public)
        {
            return true;
        }

        // Hidden states are not visible to anyone
        if (state.Visibility == Visibility.Hidden)
        {
            return false;
        }

        // Private states are visible only to the owner
        if (state.Visibility == Visibility.Private)
        {
            // Check if the state is associated with a piece owned by the viewer
            if (state is PieceState pieceState && pieceState.Artifact is Piece piece)
            {
                return piece.Owner.Equals(viewer);
            }

            // For non-piece states with Private visibility, use artifact-level ownership if available
            // This allows custom artifact types to participate in ownership-based filtering
            if (state.Artifact is Piece artifactPiece)
            {
                return artifactPiece.Owner.Equals(viewer);
            }

            // If no ownership information available, hide the state
            return false;
        }

        // Default: hide unknown visibility levels
        return false;
    }

    /// <inheritdoc />
    public IArtifactState? Redact(Player? viewer, IArtifactState state)
    {
        if (CanSee(viewer, state))
        {
            return state;
        }

        // For piece states, return a redacted placeholder
        if (state is PieceState pieceState)
        {
            return new RedactedPieceState(pieceState.Artifact, pieceState.CurrentTile);
        }

        // For other hidden states, omit them entirely
        return null;
    }
}
