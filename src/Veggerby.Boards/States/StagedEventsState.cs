using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable snapshot of staged events awaiting simultaneous resolution during a commitment phase.
/// </summary>
/// <remarks>
/// Tracks which players have committed actions and which are still pending. All commitments are
/// revealed and resolved together once all required players have committed. Equality includes
/// both the set of pending players and all committed events to ensure state diffing captures
/// commitment changes.
/// </remarks>
public sealed class StagedEventsState : ArtifactState<StagedEventsArtifact>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StagedEventsState"/> class.
    /// </summary>
    /// <param name="artifact">Associated <see cref="StagedEventsArtifact"/>.</param>
    /// <param name="commitments">Dictionary of player commitments (player to event).</param>
    /// <param name="pendingPlayers">Set of players who have not yet committed.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public StagedEventsState(
        StagedEventsArtifact artifact,
        IReadOnlyDictionary<Player, IGameEvent> commitments,
        IReadOnlySet<Player> pendingPlayers)
        : base(artifact)
    {
        ArgumentNullException.ThrowIfNull(commitments);
        ArgumentNullException.ThrowIfNull(pendingPlayers);

        Commitments = commitments;
        PendingPlayers = pendingPlayers;
    }

    /// <summary>
    /// Gets the dictionary of committed player actions.
    /// </summary>
    public IReadOnlyDictionary<Player, IGameEvent> Commitments { get; }

    /// <summary>
    /// Gets the set of players who have not yet committed an action.
    /// </summary>
    public IReadOnlySet<Player> PendingPlayers { get; }

    /// <summary>
    /// Gets a value indicating whether all required players have committed.
    /// </summary>
    public bool IsComplete => !PendingPlayers.Any();

    /// <summary>
    /// Creates a new <see cref="StagedEventsState"/> with an additional commitment recorded.
    /// </summary>
    /// <param name="player">The player committing an action.</param>
    /// <param name="event">The event being committed.</param>
    /// <returns>A new state with the commitment added and player removed from pending.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the player has already committed or is not in pending set.</exception>
    public StagedEventsState AddCommitment(Player player, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(@event);

        if (!PendingPlayers.Contains(player))
        {
            throw new InvalidOperationException($"Player '{player.Id}' has already committed or is not expected to commit.");
        }

        var newCommitments = new Dictionary<Player, IGameEvent>(Commitments)
        {
            [player] = @event
        };

        var newPending = new HashSet<Player>(PendingPlayers);
        newPending.Remove(player);

        return new StagedEventsState(Artifact, newCommitments, newPending);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as StagedEventsState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState? other)
    {
        return Equals(other as StagedEventsState);
    }

    /// <summary>
    /// Determines equality with another <see cref="StagedEventsState"/> considering artifact, commitments, and pending players.
    /// </summary>
    /// <param name="other">Other state.</param>
    /// <returns><c>true</c> if both reference the same artifact and have identical commitments and pending players.</returns>
    public bool Equals(StagedEventsState? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!Artifact.Equals(other.Artifact))
        {
            return false;
        }

        if (!PendingPlayers.SetEquals(other.PendingPlayers))
        {
            return false;
        }

        if (Commitments.Count != other.Commitments.Count)
        {
            return false;
        }

        foreach (var kvp in Commitments)
        {
            if (!other.Commitments.TryGetValue(kvp.Key, out var otherEvent))
            {
                return false;
            }

            if (!kvp.Value.Equals(otherEvent))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(GetType());
        code.Add(Artifact);

        foreach (var player in PendingPlayers.OrderBy(p => p.Id))
        {
            code.Add(player);
        }

        foreach (var kvp in Commitments.OrderBy(kvp => kvp.Key.Id))
        {
            code.Add(kvp.Key);
            code.Add(kvp.Value);
        }

        return code.ToHashCode();
    }

    /// <summary>
    /// Gets the visibility of staged events (Hidden by default - not visible to any player during commitment phase).
    /// </summary>
    /// <remarks>
    /// Committed actions are hidden from all players during the commitment phase. This integrates with
    /// the player view system to prevent information leakage. Observers may see pending counts but not contents.
    /// </remarks>
    public Visibility Visibility => Visibility.Hidden;
}
