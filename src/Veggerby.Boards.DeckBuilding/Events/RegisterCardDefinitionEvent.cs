using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Event registering a <see cref="CardDefinition"/> (metadata + victory points) for later scoring lookups.
/// </summary>
public sealed class RegisterCardDefinitionEvent : IGameEvent
{
    /// <summary>Gets the card id (artifact id shared with concrete Card instances).</summary>
    public string CardId { get; }

    /// <summary>Gets the display name.</summary>
    public string Name { get; }

    /// <summary>Gets the type tags.</summary>
    public IReadOnlyList<string> Types { get; }

    /// <summary>Gets the cost.</summary>
    public int Cost { get; }

    /// <summary>Gets the victory points contributed at scoring time.</summary>
    public int VictoryPoints { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCardDefinitionEvent"/> class.
    /// </summary>
    public RegisterCardDefinitionEvent(string cardId, string name, IList<string> types, int cost, int victoryPoints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (types is null) { throw new ArgumentNullException(nameof(types)); }
        if (cost < 0) { throw new ArgumentOutOfRangeException(nameof(cost)); }
        CardId = cardId;
        Name = name;
        Types = new List<string>(types).AsReadOnly();
        Cost = cost;
        VictoryPoints = victoryPoints;
    }
}