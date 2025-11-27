using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.States;

/// <summary>
/// Marker artifact for trade proposal state.
/// </summary>
internal sealed class TradeProposalMarker : Artifact
{
    public static readonly TradeProposalMarker Instance = new();

    private TradeProposalMarker() : base("monopoly-trade-proposal") { }
}

/// <summary>
/// Represents items offered in a trade by one party.
/// </summary>
public sealed record TradeOffer
{
    /// <summary>
    /// Gets the player ID making this offer.
    /// </summary>
    public string PlayerId
    {
        get; init;
    }

    /// <summary>
    /// Gets the cash amount being offered.
    /// </summary>
    public int Cash
    {
        get; init;
    }

    /// <summary>
    /// Gets the property positions being offered.
    /// </summary>
    public IReadOnlyList<int> PropertyPositions
    {
        get; init;
    }

    /// <summary>
    /// Gets whether a Get Out of Jail Free card is being offered.
    /// </summary>
    public bool GetOutOfJailCard
    {
        get; init;
    }

    /// <summary>
    /// Creates a new TradeOffer.
    /// </summary>
    public TradeOffer(
        string playerId,
        int cash = 0,
        IEnumerable<int>? propertyPositions = null,
        bool getOutOfJailCard = false)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        PlayerId = playerId;
        Cash = cash;
        PropertyPositions = (propertyPositions?.ToList() ?? new List<int>()).AsReadOnly();
        GetOutOfJailCard = getOutOfJailCard;
    }
}

/// <summary>
/// Represents the state of an active trade proposal.
/// </summary>
public sealed class TradeProposalState : IArtifactState
{
    /// <summary>
    /// Gets the offer from the proposing player.
    /// </summary>
    public TradeOffer? ProposerOffer
    {
        get;
    }

    /// <summary>
    /// Gets the offer requested from the target player.
    /// </summary>
    public TradeOffer? TargetOffer
    {
        get;
    }

    /// <summary>
    /// Gets whether there is an active trade proposal.
    /// </summary>
    public bool IsActive
    {
        get;
    }

    /// <inheritdoc />
    public Artifact Artifact => TradeProposalMarker.Instance;

    /// <summary>
    /// Creates a new inactive trade proposal state.
    /// </summary>
    public TradeProposalState()
    {
        ProposerOffer = null;
        TargetOffer = null;
        IsActive = false;
    }

    /// <summary>
    /// Creates a new active trade proposal state.
    /// </summary>
    public TradeProposalState(TradeOffer proposerOffer, TradeOffer targetOffer)
    {
        ArgumentNullException.ThrowIfNull(proposerOffer);
        ArgumentNullException.ThrowIfNull(targetOffer);

        ProposerOffer = proposerOffer;
        TargetOffer = targetOffer;
        IsActive = true;
    }

    /// <summary>
    /// Creates a new trade proposal.
    /// </summary>
    public TradeProposalState Propose(TradeOffer proposerOffer, TradeOffer targetOffer)
    {
        return new TradeProposalState(proposerOffer, targetOffer);
    }

    /// <summary>
    /// Cancels the trade proposal.
    /// </summary>
    public TradeProposalState Cancel()
    {
        return new TradeProposalState();
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        if (other is not TradeProposalState tradeState)
        {
            return false;
        }

        if (IsActive != tradeState.IsActive)
        {
            return false;
        }

        if (!IsActive)
        {
            return true;
        }

        return Equals(ProposerOffer, tradeState.ProposerOffer) &&
               Equals(TargetOffer, tradeState.TargetOffer);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TradeProposalState tradeState && Equals(tradeState);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(IsActive, ProposerOffer, TargetOffer);
    }
}
