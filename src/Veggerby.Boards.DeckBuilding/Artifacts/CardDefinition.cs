using System;

using System.Collections.Generic;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Immutable definition of a card used by the deck-building module.
/// </summary>
/// <remarks>
/// This definition is metadata only. Concrete card instances are provided by the Cards module
/// and referenced by id via supply piles. No runtime behavior hooks are included in the MVP; effects
/// will be modeled explicitly by events and rules in future iterations to preserve determinism.
/// </remarks>
public sealed class CardDefinition : Artifact
{
    /// <summary>Gets the display name of the card.</summary>
    public string Name { get; }

    /// <summary>Gets the set of types associated with the card (e.g., Action, Treasure, Victory).</summary>
    public IReadOnlyList<string> Types { get; }

    /// <summary>Gets the cost used during buy phase.</summary>
    public int Cost { get; }

    /// <summary>Gets the victory points contributed by this card at scoring time.</summary>
    public int VictoryPoints { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CardDefinition"/> class.
    /// </summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="name">Human-friendly name.</param>
    /// <param name="types">List of type tags; may be empty but not null.</param>
    /// <param name="cost">Non-negative cost.</param>
    /// <param name="victoryPoints">Victory points (can be negative in some games, zero by default).</param>
    public CardDefinition(string id, string name, IList<string> types, int cost, int victoryPoints = 0) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (types is null)
        {
            throw new ArgumentNullException(nameof(types));
        }
        if (cost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cost));
        }

        Name = name;
        Types = new List<string>(types).AsReadOnly();
        Cost = cost;
        VictoryPoints = victoryPoints;
    }
}