using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.Flows.Events;
namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Fluent configurator for deck-building card definitions and initial shared supply counts.
/// Produces startup events (<see cref="RegisterCardDefinitionEvent"/>, single <see cref="CreateDeckEvent"/>) in deterministic insertion order.
/// </summary>
public sealed class DeckBuildingSupplyConfigurator
{
    private readonly Dictionary<string, CardDefinitionSpec> _definitions = new(StringComparer.Ordinal);
    private readonly List<string> _definitionOrder = new();
    private readonly Dictionary<string, int> _supply = new(StringComparer.Ordinal);

    private sealed class CardDefinitionSpec
    {
        public string Id
        {
            get;
        }
        public string Name
        {
            get;
        }
        public IList<string> Types
        {
            get;
        }
        public int Cost
        {
            get;
        }
        public int Victory
        {
            get;
        }
        public CardDefinitionSpec(string id, string name, IList<string> types, int cost, int victory)
        {
            Id = id;
            Name = name;
            Types = types;
            Cost = cost;
            Victory = victory;
        }
    }

    /// <summary>Adds a card definition (metadata + scoring) to the configuration.</summary>
    public DeckBuildingSupplyConfigurator AddDefinition(string id, string name, IEnumerable<string> types, int cost, int victoryPoints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (types is null)
        {
            throw new ArgumentNullException(nameof(types));
        }
        if (cost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cost));
        }
        if (_definitions.ContainsKey(id))
        {
            throw new InvalidOperationException($"Card definition '{id}' already added.");
        }
        var list = new List<string>();
        foreach (var t in types)
        {
            if (!string.IsNullOrWhiteSpace(t))
            {
                list.Add(t);
            }
        }
        _definitions[id] = new CardDefinitionSpec(id, name, list, cost, victoryPoints);
        _definitionOrder.Add(id);
        return this;
    }

    /// <summary>Adds (or replaces) a supply count for a defined card id.</summary>
    public DeckBuildingSupplyConfigurator AddSupply(string id, int count)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        if (!_definitions.ContainsKey(id))
        {
            throw new InvalidOperationException($"Cannot assign supply for undefined card id '{id}'. Add definition first.");
        }
        _supply[id] = count;
        return this;
    }

    /// <summary>Builds startup events (definitions then single CreateDeckEvent with empty piles + supply).</summary>
    public IReadOnlyList<IGameEvent> BuildStartupEvents(Deck deck)
    {
        if (deck is null)
        {
            throw new ArgumentNullException(nameof(deck));
        }
        var events = new List<IGameEvent>(_definitionOrder.Count + 1);
        // Register definitions in insertion order.
        for (var i = 0; i < _definitionOrder.Count; i++)
        {
            var id = _definitionOrder[i];
            var spec = _definitions[id];
            events.Add(new RegisterCardDefinitionEvent(spec.Id, spec.Name, spec.Types, spec.Cost, spec.Victory));
        }
        // Prepare empty piles mapping (all deck piles start empty; callers can follow-up with Gain/Draw events)
        var piles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var pileId in deck.Piles)
        {
            piles[pileId] = new List<Card>();
        }
        // Copy supply counts (skip zero counts to keep dictionary minimal)
        var supplyDict = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var kv in _supply)
        {
            if (kv.Value > 0)
            {
                supplyDict[kv.Key] = kv.Value;
            }
        }
        events.Add(new CreateDeckEvent(deck, piles, supplyDict));
        return events;
    }
}

/// <summary>
/// Extension helpers for integrating <see cref="DeckBuildingSupplyConfigurator"/> with <see cref="DeckBuildingGameBuilder"/>.
/// </summary>
public static class DeckBuildingSupplyConfiguratorExtensions
{
    /// <summary>Gets or lazily creates a supply configurator attached to the builder.</summary>
    public static DeckBuildingSupplyConfigurator ConfigureSupply(this DeckBuildingGameBuilder builder, Action<DeckBuildingSupplyConfigurator> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
        var cfg = builder.SupplyConfigurator ?? new DeckBuildingSupplyConfigurator();
        configure(cfg);
        builder.SupplyConfigurator = cfg; // store (internal set) for later retrieval
        return cfg;
    }
}