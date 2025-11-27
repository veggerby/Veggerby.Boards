using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.States;

/// <summary>
/// Marker artifact for Monopoly player state.
/// </summary>
internal sealed class MonopolyPlayerMarker : Artifact
{
    public MonopolyPlayerMarker(string playerId) : base($"monopoly-player-{playerId}") { }
}

/// <summary>
/// Represents the economic state of a single Monopoly player.
/// </summary>
public sealed class MonopolyPlayerState : IArtifactState
{
    private readonly MonopolyPlayerMarker _marker;

    /// <summary>
    /// Gets the player this state applies to.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the current cash balance.
    /// </summary>
    public int Cash
    {
        get;
    }

    /// <summary>
    /// Gets whether the player is currently in jail.
    /// </summary>
    public bool InJail
    {
        get;
    }

    /// <summary>
    /// Gets the number of turns spent in jail.
    /// </summary>
    public int JailTurns
    {
        get;
    }

    /// <summary>
    /// Gets whether the player has a Get Out of Jail Free card.
    /// </summary>
    public bool HasGetOutOfJailCard
    {
        get;
    }

    /// <summary>
    /// Gets whether the player is bankrupt (eliminated).
    /// </summary>
    public bool IsBankrupt
    {
        get;
    }

    /// <summary>
    /// Gets the number of consecutive doubles rolled this turn.
    /// </summary>
    public int ConsecutiveDoubles
    {
        get;
    }

    /// <inheritdoc />
    public Artifact Artifact => _marker;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonopolyPlayerState"/> class.
    /// </summary>
    public MonopolyPlayerState(
        Player player,
        int cash,
        bool inJail = false,
        int jailTurns = 0,
        bool hasGetOutOfJailCard = false,
        bool isBankrupt = false,
        int consecutiveDoubles = 0)
    {
        ArgumentNullException.ThrowIfNull(player);

        _marker = new MonopolyPlayerMarker(player.Id);
        Player = player;
        Cash = cash;
        InJail = inJail;
        JailTurns = jailTurns;
        HasGetOutOfJailCard = hasGetOutOfJailCard;
        IsBankrupt = isBankrupt;
        ConsecutiveDoubles = consecutiveDoubles;
    }

    /// <summary>
    /// Creates a new state with modified cash.
    /// </summary>
    public MonopolyPlayerState WithCash(int cash)
    {
        return new MonopolyPlayerState(Player, cash, InJail, JailTurns, HasGetOutOfJailCard, IsBankrupt, ConsecutiveDoubles);
    }

    /// <summary>
    /// Creates a new state with adjusted cash (positive or negative delta).
    /// </summary>
    public MonopolyPlayerState AdjustCash(int delta)
    {
        return new MonopolyPlayerState(Player, Cash + delta, InJail, JailTurns, HasGetOutOfJailCard, IsBankrupt, ConsecutiveDoubles);
    }

    /// <summary>
    /// Creates a new state with the player sent to jail.
    /// </summary>
    public MonopolyPlayerState GoToJail()
    {
        return new MonopolyPlayerState(Player, Cash, true, 0, HasGetOutOfJailCard, IsBankrupt, 0);
    }

    /// <summary>
    /// Creates a new state with the player released from jail.
    /// </summary>
    public MonopolyPlayerState ReleaseFromJail()
    {
        return new MonopolyPlayerState(Player, Cash, false, 0, HasGetOutOfJailCard, IsBankrupt, 0);
    }

    /// <summary>
    /// Creates a new state with incremented jail turns.
    /// </summary>
    public MonopolyPlayerState IncrementJailTurns()
    {
        return new MonopolyPlayerState(Player, Cash, InJail, JailTurns + 1, HasGetOutOfJailCard, IsBankrupt, 0);
    }

    /// <summary>
    /// Creates a new state with modified Get Out of Jail Free card status.
    /// </summary>
    public MonopolyPlayerState WithGetOutOfJailCard(bool hasCard)
    {
        return new MonopolyPlayerState(Player, Cash, InJail, JailTurns, hasCard, IsBankrupt, ConsecutiveDoubles);
    }

    /// <summary>
    /// Creates a new state with the player marked as bankrupt.
    /// </summary>
    public MonopolyPlayerState MarkBankrupt()
    {
        return new MonopolyPlayerState(Player, 0, false, 0, false, true, 0);
    }

    /// <summary>
    /// Creates a new state with consecutive doubles count.
    /// </summary>
    public MonopolyPlayerState WithConsecutiveDoubles(int count)
    {
        return new MonopolyPlayerState(Player, Cash, InJail, JailTurns, HasGetOutOfJailCard, IsBankrupt, count);
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is MonopolyPlayerState mps &&
               Equals(mps.Player, Player) &&
               mps.Cash == Cash &&
               mps.InJail == InJail &&
               mps.JailTurns == JailTurns &&
               mps.HasGetOutOfJailCard == HasGetOutOfJailCard &&
               mps.IsBankrupt == IsBankrupt &&
               mps.ConsecutiveDoubles == ConsecutiveDoubles;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MonopolyPlayerState mps && Equals(mps);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Player, Cash, InJail, JailTurns, HasGetOutOfJailCard, IsBankrupt, ConsecutiveDoubles);
    }
}
