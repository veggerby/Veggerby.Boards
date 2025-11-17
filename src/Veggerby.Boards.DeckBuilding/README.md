# Veggerby.Boards.DeckBuilding

Deck-building core module built on Veggerby.Boards and Veggerby.Boards.Cards providing deterministic supply management, player zones (deck/hand/discard/in-play), and phase-based gameplay foundation.

> Depends on `Veggerby.Boards` and `Veggerby.Boards.Cards`. Use when building deck-building games like Dominion, Ascension, or Star Realms.

## Install

Package publishes with the rest of the suite. Until then, reference the project directly.

## Overview

This module provides a deterministic foundation for deck-building games with:

- **CardDefinition**: Metadata artifacts (name, types, cost, victory points)
- **Player Zones**: Immutable pile management (Deck, Hand, Discard, InPlay)
- **Deterministic Shuffles**: Seeded RNG for reproducible gameplay
- **Phase Structure**: Setup → Action → Buy → Cleanup
- **Supply Management**: Centralized card pool with depletion tracking
- **Scoring & Termination**: Victory point computation and game-end detection

## Quick Start

```csharp
using Veggerby.Boards.DeckBuilding;

// Create a deck-building game
var builder = new DeckBuildingGameBuilder();
var progress = builder.Compile();

// Initialize a player's deck
var player = progress.Game.GetPlayer("player-1");
var deck = progress.Game.GetArtifact<Deck>("deck-1");

var piles = new Dictionary<string, IList<Card>>
{
    [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
    [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
    [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
    [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
};
var supply = new Dictionary<string, int> { ["copper"] = 7, ["estate"] = 3 };

progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));

// Draw cards (with automatic reshuffle when deck empty)
progress = progress.HandleEvent(new DrawWithReshuffleEvent(deck, count: 5));

// Gain a card from supply
progress = progress.HandleEvent(new GainFromSupplyEvent(
    player, deck, "copper", DeckBuildingGameBuilder.Piles.Discard));

// Trash cards from hand
progress = progress.HandleEvent(new TrashFromHandEvent(deck, cardsToTrash));

// End turn cleanup
progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));
```

## Key Concepts

### Phases

| Phase | Purpose | Events |
|-------|---------|--------|
| **Setup** | Initialize deck and supply | `CreateDeckEvent` |
| **Action** | Draw cards and trash | `DrawWithReshuffleEvent`, `TrashFromHandEvent` |
| **Buy** | Gain cards from supply | `GainFromSupplyEvent` |
| **Cleanup** | Move cards to discard, compute scores, end game | `CleanupToDiscardEvent`, `ComputeScoresEvent`, `EndGameEvent` |

### Events & Mutators

| Event | Mutator | Description |
|-------|---------|-------------|
| `RegisterCardDefinitionEvent` | `RegisterCardDefinitionStateMutator` | Register card metadata (name, types, cost, VP) |
| `CreateDeckEvent` | `CreateDeckStateMutator` | Initialize player zones and supply |
| `GainFromSupplyEvent` | `GainFromSupplyStateMutator` | Move card from supply to player pile, decrement supply |
| `DrawWithReshuffleEvent` | `DrawWithReshuffleStateMutator` | Draw cards, reshuffle discard if needed |
| `TrashFromHandEvent` | `TrashFromHandStateMutator` | Remove cards from hand (permanent) |
| `CleanupToDiscardEvent` | `CleanupToDiscardStateMutator` | Move Hand + InPlay to Discard |
| `ComputeScoresEvent` | `ComputeScoresStateMutator` | Calculate victory points per player |
| `EndGameEvent` | `EndGameStateMutator` | Mark game as ended |

### Zones (Player Piles)

Each player has four standard piles:
- **Draw**: Cards to be drawn
- **Hand**: Cards currently available for play
- **InPlay**: Cards played this turn
- **Discard**: Used cards awaiting reshuffle

All pile operations are immutable; mutators return new state snapshots.

### Deterministic Shuffles

Shuffles use `GameState.Random` with seeded RNG:

```csharp
var builder = new DeckBuildingGameBuilder();
builder.WithSeed(12345);  // Reproducible shuffle order
var progress = builder.Compile();
```

Same seed → same shuffle sequence across all runs.

### Supply Management

The supply is a shared pool of available cards:
- Tracked in `DeckSupplyStats` for O(1) empty pile detection
- Decremented on `GainFromSupplyEvent`
- Used for game-end trigger (see below)

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~DeckBuilding"
```

**Test Coverage**:
- Deck initialization and supply setup
- Gain from supply (happy path and insufficient supply)
- Draw with automatic reshuffle (determinism verified)
- Trash validation (card presence in hand)
- Cleanup behavior (all cards moved to discard)
- Scoring computation (victory point aggregation)
- End-game trigger conditions
- Structural invariants (phase ordering, event presence)

## Game-End Trigger

Configure supply depletion trigger:

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

## Error Modes

### Gain From Supply Failures

`GainFromSupplyEventCondition` returns `Fail` (raising `InvalidGameEventException`) with these reasons:

- **Unknown pile**: Target pile id not present in the deck state
- **Insufficient supply**: Supply dictionary does not contain the card id or its count is zero
- **Unknown card id**: Card artifact not registered in the game even though supply references it

The exception message includes the failing reason (e.g., `Invalid game event GainFromSupplyEvent: Unknown pile`).

## Known Limitations

- **Card Effects**: No action card effects implemented (beyond basic zone transitions)
- **Attack/Reaction**: Interaction between players not modeled
- **Duration Cards**: Multi-turn effect persistence not supported
- **Cost Reduction**: Dynamic pricing not implemented
- **Benchmarks**: Partial coverage only (shuffle, gain, condition gating)

## Extending This Module

Common extension scenarios:

### Adding Action Cards

```csharp
public sealed record PlayActionEvent(Deck Deck, Card ActionCard) : IGameEvent;

public sealed class PlayActionStateMutator : IStateMutator<PlayActionEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, PlayActionEvent @event)
    {
        // Move card from Hand to InPlay
        // Apply card-specific effects (draw, trash, coins, etc.)
    }
}
```

### Implementing Card Effects

```csharp
public interface ICardEffect
{
    GameState Apply(GameEngine engine, GameState state, Deck deck);
}

public sealed class DrawCardsEffect : ICardEffect
{
    private readonly int _count;
    
    public GameState Apply(GameEngine engine, GameState state, Deck deck)
    {
        // Execute draw logic
    }
}
```

### Custom End-Game Triggers

```csharp
public sealed class VictoryPointThresholdCondition : IGameStateCondition
{
    private readonly int _threshold;
    
    public ConditionResponse Evaluate(Game game, GameState state)
    {
        // Check if any player has reached VP threshold
    }
}
```

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Deck Building Guide**: See [/docs/deck-building.md](../../docs/deck-building.md) for detailed module documentation
- **Cards Module**: See [/src/Veggerby.Boards.Cards/README.md](../Veggerby.Boards.Cards/README.md) for underlying card primitives
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines

## Versioning

Semantic versioning aligned with repository releases. Breaking event or rule changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
