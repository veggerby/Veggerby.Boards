# Veggerby.Boards.DeckBuilding

Deck-building core built on Veggerby.Boards and Veggerby.Boards.Cards. Provides deterministic supply and player zone foundations with an initial Buy phase rule.

## Install

Package publishes with the rest of the suite. Until then, reference the project directly.

## Scope (MVP)

- CardDefinition metadata (name, types, cost, victory points)
- Player zones (deck/hand/discard/in-play) atop Cards piles
- Turn phases scaffolded; Buy phase includes Gain From Supply
- Events/Rules wired:
  - CreateDeckEvent: initializes piles and optional supply
  - GainFromSupplyEvent: decrements supply and appends the card to a target pile (e.g., discard)
  - DrawWithReshuffleEvent: draws from draw into hand, shuffling discard deterministically into draw if needed
  - TrashFromHandEvent: removes specified cards from the hand (trashed)
  - CleanupToDiscardEvent: moves all cards from hand and in-play to discard for end-of-turn cleanup

Turn phases:

- Setup: initialize deck state (`CreateDeckEvent`)
- Action: draw and trash (`DrawWithReshuffleEvent`, `TrashFromHandEvent`)
- Buy: gain from supply (`GainFromSupplyEvent`)
- Cleanup: end-of-turn cleanup (`CleanupToDiscardEvent`)

## Determinism

All shuffles and draws are deterministic via the engine RNG. Seed with `GameBuilder.WithSeed(ulong)` to reproduce sequences.

## Usage

- Register your concrete cards before compiling the game:
  - `builder.WithCard("copper");`
  - or bulk: `builder.WithCards("copper", "estate", "silver");`
- Compile and initialize a player's deck state using `CreateDeckEvent` providing piles and optional supply.
- During the Buy phase, submit `GainFromSupplyEvent(player, deck, cardId, targetPileId)` to move a card from supply to a pile (commonly discard) and decrement supply deterministically.
- To draw with automatic reshuffle: submit `DrawWithReshuffleEvent(deck, count)`; it will reuse discard if draw is empty/insufficient.
- To trash: submit `TrashFromHandEvent(deck, cards)`; it removes those cards from the Hand pile.
- To cleanup at end of turn: submit `CleanupToDiscardEvent(deck)`; it moves all cards from Hand and InPlay to Discard.
- During Buy: submit `GainFromSupplyEvent(player, deck, cardId, targetPileId)` to move a card from supply to a pile (commonly discard) and decrement supply deterministically.
- During Action: submit `DrawWithReshuffleEvent(deck, count)`; it will reuse discard if draw is empty/insufficient.
- During Action: submit `TrashFromHandEvent(deck, cards)`; it removes those cards from the Hand pile.
- During Cleanup: submit `CleanupToDiscardEvent(deck)`; it moves all cards from Hand and InPlay to Discard.

Example initializing supply with `CreateDeckEvent`:

```csharp
var piles = new Dictionary<string, IList<Card>>
{
  [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
  [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
  [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
  [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
};
var supply = new Dictionary<string, int> { ["copper"] = 7, ["estate"] = 3 };
progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));
```

Determinism is preserved through the engine RNG; seed via `GameBuilder.WithSeed(ulong)` for reproducible sequences.

### End Game Trigger (Supply Depletion)

You can enable an alternate game end trigger based on supply depletion via:

```csharp
builder.WithEndTrigger(new DeckBuildingEndTriggerOptions(
  emptySupplyPilesThreshold: 3,                    // optional > 0 threshold
  keySupplyPileIds: new[]{"province", "colony"}   // optional specific key piles that must all be empty
));
```

Validation rule (enforced in constructor): at least one of the following must be configured:

1. `emptySupplyPilesThreshold > 0`
2. A non-empty `keySupplyPileIds` collection

If neither is provided, an `ArgumentException` is thrown. Negative thresholds throw `ArgumentOutOfRangeException`.

At runtime the `EndGameEventCondition` requires scores to have been computed (via a preceding `ComputeScoresEvent`) before a depletion-triggered end may validate; otherwise the end event is ignored until scoring occurs.

### Gain From Supply Failure Modes

`GainFromSupplyEventCondition` returns `Fail` (raising `InvalidGameEventException`) with these reasons:

- `Unknown pile` – Target pile id not present in the deck state.
- `Insufficient supply` – Supply dictionary does not contain the card id or its count is zero.
- `Unknown card id` – Card artifact not registered in the game even though supply references it.

The exception message now includes the failing reason (e.g., `Invalid game event GainFromSupplyEvent: Unknown pile`). Tests assert on substrings only; reason text is stable and considered part of the public contract.

## Status

Workstream 17 is in progress. This module currently supports initializing decks, gaining from supply, trashing, cleanup, and reshuffle-on-empty draws; additional features (e.g., scoring) will follow.
