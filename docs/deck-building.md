# Deck-building Module

> Deterministic Dominion-like foundation: ordered supply, player zones (Draw / Hand / Discard / InPlay), Action → Buy → Cleanup turn segmentation, deterministic shuffles, scoring + termination, and a fluent supply configurator.

## Overview

The deck-building module (`Veggerby.Boards.DeckBuilding`) composes the generic Cards primitives with additional rules, phases, and events to model a Dominion‑style loop while preserving core engine invariants:

* Pure events & mutators (no hidden mutation).
* Deterministic replay (all randomness flows through explicit RNG / shuffle state).
* Immutable artifacts (Cards, Decks, CardDefinitions) and snapshot `GameState` history.

### Phases & Turn Segments

| Phase        | Segment Role | Key Events (Examples) |
|--------------|--------------|------------------------|
| `db-setup`   | Start        | `CreateDeckEvent` (initial piles), `RegisterCardDefinitionEvent` (metadata) |
| `db-action`  | Main         | `DrawWithReshuffleEvent`, `TrashFromHandEvent` |
| `db-buy`     | Main         | `GainFromSupplyEvent` |
| `db-cleanup` | End          | `CleanupToDiscardEvent`, `ComputeScoresEvent`, `EndGameEvent` |

Turn sequencing (Start → Main → End, rotation) is provided by the core turn system; deck-building phases plug into that lifecycle.

### Zones

Each player (via shared deck artifact) interacts with named piles:

* Draw (face-down ordered)
* Hand (face-up ordered as drawn)
* Discard (face-up stack; reshuffled into Draw when Draw exhausted on demand)
* InPlay (cards played this turn; cleared during cleanup)

All transitions allocate new arrays; no in-place mutation of pile contents occurs.

### Shuffle Determinism

`DrawWithReshuffleEvent` triggers a deterministic shuffle when Draw is empty and Discard has cards. The shuffle uses engine RNG seeded through `GameBuilder.WithSeed(seed)`. Given identical prior state + seed + event sequence, replay order is identical.

### Card Definitions & Scoring

`RegisterCardDefinitionEvent` registers a card's metadata:

* Id (string)
* Name (display)
* Types (`IList<string>`; e.g. Action, Victory, Treasure, Attack)
* Cost (int)
* Victory points contribution (int; aggregated by `ComputeScoresEvent`)

`ComputeScoresEvent` produces a `ScoreState` snapshot mapping each player to total VP (sum of owned cards' victory values). `EndGameEvent` marks termination (follows scoring; ordering invariant enforced in tests).

## Supply Configurator

To reduce boilerplate when setting up multiple card definitions and initial supply counts, use `DeckBuildingSupplyConfigurator` via the `ConfigureSupply` extension on `DeckBuildingGameBuilder`.

Deterministic guarantees:

* Definitions are emitted as `RegisterCardDefinitionEvent`s in insertion order.
* A single `CreateDeckEvent` follows definitions, initializing piles and capturing a supply snapshot (only non-zero counts stored).
* Duplicate definition ids throw.
* Attempting to assign supply for an undefined card throws.

### Fluent API

````csharp
builder.ConfigureSupply(cfg =>
{
    cfg
        .AddDefinition("copper", "Copper", new[]{"Treasure"}, cost: 0, victoryPoints: 0)
        .AddDefinition("estate", "Estate", new[]{"Victory"}, cost: 2, victoryPoints: 1)
        .AddDefinition("silver", "Silver", new[]{"Treasure"}, cost: 3, victoryPoints: 0)
        .AddSupply("copper", 60)
        .AddSupply("estate", 24)
        .AddSupply("silver", 40);
});
````

Build the startup events (usually right before or during game compile); they are not auto-applied to allow you to interleave with other setup if desired:

````csharp
// Acquire the compiled game framework first (players, board, phases already wired).
var progress = builder.Compile();

// Emit deterministic startup events from configurator.
var deck = progress.Game.GetDeck("main"); // example accessor
var startup = builder.SupplyConfigurator.BuildStartupEvents(deck);

foreach (var ev in startup)
{
    progress = progress.HandleEvent(ev);
}
````

You can then gain cards from the supply:

````csharp
// Gain a Copper into a player's discard (deck pile id sample: "discard")
var copperCard = progress.Game.GetCard("copper"); // metadata artifact
progress = progress.HandleEvent(new GainFromSupplyEvent(deck, copperCard, targetPileId: "discard"));
````

### Sample End-to-End Setup

````csharp
var gameBuilder = new DeckBuildingGameBuilder()
    .WithPlayer("alice")
    .WithPlayer("bob")
    .ConfigureSupply(cfg =>
    {
        cfg
            .AddDefinition("copper", "Copper", new[]{"Treasure"}, 0, 0)
            .AddDefinition("estate", "Estate", new[]{"Victory"}, 2, 1)
            .AddDefinition("silver", "Silver", new[]{"Treasure"}, 3, 0)
            .AddSupply("copper", 60)
            .AddSupply("estate", 24)
            .AddSupply("silver", 40);
    });

var progress = gameBuilder.Compile();
var deck = progress.Game.GetDeck("main");
var startup = gameBuilder.SupplyConfigurator.BuildStartupEvents(deck);
foreach (var e in startup)
{
    progress = progress.HandleEvent(e);
}

// Draw opening hand (example: draw 5 cards)
for (int i = 0; i < 5; i++)
{
    progress = progress.HandleEvent(new DrawWithReshuffleEvent(deck, count: 1));
}

// Perform a buy (gain Silver)
var silver = progress.Game.GetCard("silver");
progress = progress.HandleEvent(new GainFromSupplyEvent(deck, silver, targetPileId: "discard"));

// Cleanup at end of turn
progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));

// Trigger scoring + termination (cleanup phase ordering ensures ComputeScores precedes EndGame)
progress = progress.HandleEvent(new ComputeScoresEvent());
progress = progress.HandleEvent(new EndGameEvent());
````

### Error Modes

* Duplicate definition id: `InvalidOperationException`.
* Supply for undefined id: `InvalidOperationException`.
* Negative cost or supply count: `ArgumentOutOfRangeException`.
* Null/blank ids or name: `ArgumentException`.

### Alternate End Trigger (Supply Depletion)

By default the module allows `EndGameEvent` once scores are computed and the minimal fixed turn threshold is reached.

You can optionally enable a *supply depletion* end trigger so the game may end earlier when either:

* A configured number of distinct supply piles are empty (threshold), OR
* Any pile from a configured key set is empty.

Configuration (call before `Compile`):

```csharp
var builder = new DeckBuildingGameBuilder()
    .WithEndTrigger(new DeckBuildingEndTriggerOptions(
        emptySupplyPilesThreshold: 2, // require 2 empty piles
        keyPileCardIds: new[]{"province"} // OR province pile empty
    ));
```

Semantics:

* Scores (`ComputeScoresEvent`) must still have been processed; depletion alone never bypasses scoring.
* If options are provided and neither condition is met the `EndGameEvent` is ignored (turn threshold does not apply when custom options are active).
* Options are immutable and captured in the initial state as an extras snapshot (participating in hashing for deterministic replay).

### Extension Points

Future planned extensions (guarded until needed):

* Attack / Reaction card types (additional types + conditions).
* Cost modifiers / effect hooks.
* Batch definition addition helper.

## Determinism & Testing

Tests cover:

* Insertion ordering of definition events.
* Duplicate definition rejection.
* Undefined supply assignment rejection.
* Integration of emitted events with `GainFromSupplyEvent` decrement path.
* Scoring & termination ordering invariant.

Re-run tests with:

```bash
dotnet test test/Veggerby.Boards.Tests -c Debug
```

(Use the existing solution test tasks; above is illustrative.)

---
*End of deck-building module docs.*
