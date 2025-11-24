using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.DeckBuilding.States;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
namespace Veggerby.Boards.DeckBuilding.Mutators;

/// <summary>
/// Creates or replaces the DeckState for a deck using the provided piles and optional supply.
/// </summary>
public sealed class DeckBuildingCreateDeckStateMutator : IStateMutator<CreateDeckEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, CreateDeckEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var piles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in @event.Piles)
        {
            piles[kv.Key] = kv.Value?.ToList() ?? new List<Card>();
        }
        var ds = new DeckState(@event.Deck, piles, @event.Supply);
        var next = state.Next([ds]);
        if (@event.Supply is not null)
        {
            var stats = DeckSupplyStats.From(@event.Supply);
            return next.ReplaceExtras(stats);
        }
        return next;
    }
}