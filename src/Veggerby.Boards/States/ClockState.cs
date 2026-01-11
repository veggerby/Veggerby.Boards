using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable state tracking remaining time for each player in a game clock.
/// </summary>
/// <remarks>
/// ClockState maintains time banks for all players and tracks the currently active turn.
/// Time tracking is deterministic - all timestamps are explicit event parameters (not wall-clock).
/// State transitions produce new instances; original states are never mutated.
/// </remarks>
public sealed class ClockState : IArtifactState
{
    /// <summary>
    /// Gets the game clock artifact this state belongs to.
    /// </summary>
    public GameClock Clock => (GameClock)Artifact;

    /// <inheritdoc />
    public Artifact Artifact
    {
        get;
    }

    /// <summary>
    /// Gets the remaining time for each player.
    /// </summary>
    /// <remarks>
    /// Dictionary maps players to their remaining time banks.
    /// When time reaches or falls below zero, the player has flagged (lost on time).
    /// </remarks>
    public IReadOnlyDictionary<Player, TimeSpan> RemainingTime
    {
        get;
    }

    /// <summary>
    /// Gets the player whose clock is currently running.
    /// </summary>
    /// <remarks>
    /// Null when no turn is active (e.g., between moves, game not started).
    /// </remarks>
    public Player? ActivePlayer
    {
        get;
    }

    /// <summary>
    /// Gets the timestamp when the current turn started.
    /// </summary>
    /// <remarks>
    /// Null when no turn is active. Used to calculate elapsed time when stopping the clock.
    /// Timestamps are explicit event parameters for deterministic replay.
    /// </remarks>
    public DateTime? TurnStartedAt
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClockState"/> class.
    /// </summary>
    /// <param name="clock">The game clock artifact.</param>
    /// <param name="remainingTime">Remaining time per player.</param>
    /// <param name="activePlayer">Currently active player (null if none).</param>
    /// <param name="turnStartedAt">Turn start timestamp (null if no active turn).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="clock"/> or <paramref name="remainingTime"/> is null.</exception>
    public ClockState(
        GameClock clock,
        IReadOnlyDictionary<Player, TimeSpan> remainingTime,
        Player? activePlayer = null,
        DateTime? turnStartedAt = null)
    {
        ArgumentNullException.ThrowIfNull(clock, nameof(clock));
        ArgumentNullException.ThrowIfNull(remainingTime, nameof(remainingTime));

        Artifact = clock;
        RemainingTime = remainingTime;
        ActivePlayer = activePlayer;
        TurnStartedAt = turnStartedAt;
    }

    /// <summary>
    /// Starts the clock for a player's turn.
    /// </summary>
    /// <param name="player">The player whose turn is starting.</param>
    /// <param name="timestamp">Explicit timestamp when the turn started.</param>
    /// <returns>New clock state with active turn tracking.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a turn is already active.</exception>
    public ClockState StartTurn(Player player, DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        if (ActivePlayer != null)
        {
            throw new InvalidOperationException($"Cannot start turn for {player.Id}: turn already active for {ActivePlayer.Id}");
        }

        return new ClockState(
            Clock,
            RemainingTime,
            player,
            timestamp);
    }

    /// <summary>
    /// Stops the clock for the current turn, deducting elapsed time and applying increments.
    /// </summary>
    /// <param name="timestamp">Explicit timestamp when the turn ended.</param>
    /// <returns>New clock state with updated time banks and no active turn.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no turn is active.</exception>
    public ClockState EndTurn(DateTime timestamp)
    {
        if (ActivePlayer == null || TurnStartedAt == null)
        {
            throw new InvalidOperationException("Cannot end turn: no active turn");
        }

        var elapsed = timestamp - TurnStartedAt.Value;
        var increment = Clock.Control.Increment ?? TimeSpan.Zero;

        var updatedTime = new Dictionary<Player, TimeSpan>(RemainingTime);
        updatedTime[ActivePlayer] = RemainingTime[ActivePlayer] - elapsed + increment;

        return new ClockState(
            Clock,
            updatedTime,
            null,
            null);
    }

    /// <summary>
    /// Checks if a player has run out of time.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's remaining time is zero or negative.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    public bool IsTimeExpired(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        return RemainingTime.TryGetValue(player, out var remaining) && remaining <= TimeSpan.Zero;
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState? other)
    {
        if (other is not ClockState clockState)
        {
            return false;
        }

        if (!Artifact.Equals(clockState.Artifact))
        {
            return false;
        }

        if (!Equals(ActivePlayer, clockState.ActivePlayer))
        {
            return false;
        }

        if (TurnStartedAt != clockState.TurnStartedAt)
        {
            return false;
        }

        if (RemainingTime.Count != clockState.RemainingTime.Count)
        {
            return false;
        }

        foreach (var kvp in RemainingTime)
        {
            if (!clockState.RemainingTime.TryGetValue(kvp.Key, out var otherTime))
            {
                return false;
            }

            if (kvp.Value != otherTime)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Artifact);
        hash.Add(ActivePlayer);
        hash.Add(TurnStartedAt);

        foreach (var kvp in RemainingTime.OrderBy(x => x.Key.Id))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }

        return hash.ToHashCode();
    }
}
