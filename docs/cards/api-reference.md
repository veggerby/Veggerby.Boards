---
slug: cards-api-reference
name: "Cards Module API Reference"
last_updated: 2025-11-14
owner: core
summary: >-
  Complete API reference for the Cards module artifacts, states, events, and builders.
---

# Cards Module API Reference

Complete reference for all public types in the `Veggerby.Boards.Cards` namespace.

## Artifacts

### Card

Immutable card artifact representing a single card identity.

**Namespace**: `Veggerby.Boards.Cards`

**Inheritance**: `Artifact`

#### Constructor

```csharp
public Card(string id)
```

**Parameters**:
- `id` (string): Unique card identifier

**Example**:
```csharp
var aceOfSpades = new Card("ace-of-spades");
```

---

### Deck

Immutable deck artifact representing a collection of ordered piles of cards.

**Namespace**: `Veggerby.Boards.Cards`

**Inheritance**: `Artifact`

#### Properties

```csharp
public IReadOnlyList<string> Piles { get; }
```

The defined pile identifiers for this deck.

#### Constructor

```csharp
public Deck(string id, IEnumerable<string> piles)
```

**Parameters**:
- `id` (string): Unique deck identifier
- `piles` (IEnumerable&lt;string&gt;): Pile identifiers (ordered, unique)

**Throws**:
- `ArgumentException`: If no piles provided or duplicate pile names

**Example**:
```csharp
var deck = new Deck("main-deck", new[] { "draw", "discard", "hand", "inplay" });
```

---

## States

### DeckState

Immutable state representing ordered piles for a Deck and optional supply counts.

**Namespace**: `Veggerby.Boards.Cards`

**Inheritance**: `ArtifactState<Deck>`

#### Properties

```csharp
public IReadOnlyDictionary<string, IReadOnlyList<Card>> Piles { get; }
```

The ordered cards per pile id.

```csharp
public IReadOnlyDictionary<string, int> Supply { get; }
```

Supply counts for card identifiers (optional).

#### Constructor

```csharp
public DeckState(
    Deck deck, 
    IDictionary<string, IList<Card>> piles, 
    IDictionary<string, int>? supply = null)
```

**Parameters**:
- `deck` (Deck): The deck artifact
- `piles` (IDictionary&lt;string, IList&lt;Card&gt;&gt;): Initial pile contents
- `supply` (IDictionary&lt;string, int&gt;, optional): Supply counts

**Throws**:
- `ArgumentException`: If required deck piles are missing

**Example**:
```csharp
var piles = new Dictionary<string, IList<Card>>
{
    ["draw"] = new List<Card> { card1, card2, card3 },
    ["discard"] = new List<Card>(),
    ["hand"] = new List<Card>()
};
var deckState = new DeckState(deck, piles);
```

---

## Events

All events implement `IGameEvent` and are immutable records.

### CreateDeckEvent

Initialize a deck's piles with cards.

#### Properties

```csharp
public Deck Deck { get; }
public IReadOnlyDictionary<string, IReadOnlyList<Card>> Piles { get; }
public IReadOnlyDictionary<string, int> Supply { get; }
```

#### Constructor

```csharp
public CreateDeckEvent(
    Deck deck, 
    IDictionary<string, IList<Card>> piles, 
    IDictionary<string, int>? supply = null)
```

---

### ShuffleDeckEvent

Deterministically shuffle a pile using Fisher-Yates algorithm.

#### Properties

```csharp
public Deck Deck { get; }
public string PileId { get; }
```

#### Constructor

```csharp
public ShuffleDeckEvent(Deck deck, string pileId)
```

**Example**:
```csharp
var shuffleEvt = new ShuffleDeckEvent(deck, "draw");
```

---

### DrawCardsEvent

Draw N cards from source pile to destination pile.

#### Properties

```csharp
public Deck Deck { get; }
public string FromPileId { get; }
public string ToPileId { get; }
public int Count { get; }
```

#### Constructor

```csharp
public DrawCardsEvent(
    Deck deck, 
    string fromPileId, 
    string toPileId, 
    int count)
```

**Throws**:
- `ArgumentNullException`: If deck or pile IDs are null
- `ArgumentOutOfRangeException`: If count is negative

**Example**:
```csharp
var drawEvt = new DrawCardsEvent(deck, "draw", "hand", 5);
```

---

### MoveCardsEvent

Move cards by count or explicit identities from source to destination.

#### Properties

```csharp
public Deck Deck { get; }
public string FromPileId { get; }
public string ToPileId { get; }
public int? Count { get; }
public IReadOnlyList<Card>? Cards { get; }
```

#### Constructors

Move by count:
```csharp
public MoveCardsEvent(
    Deck deck, 
    string fromPileId, 
    string toPileId, 
    int count)
```

Move explicit cards:
```csharp
public MoveCardsEvent(
    Deck deck, 
    string fromPileId, 
    string toPileId, 
    IReadOnlyList<Card> cards)
```

**Example**:
```csharp
// By count
var moveEvt = new MoveCardsEvent(deck, "hand", "inplay", 2);

// Explicit cards
var selectedCards = new[] { card1, card2 };
var moveEvt = new MoveCardsEvent(deck, "hand", "inplay", selectedCards);
```

---

### DiscardCardsEvent

Move specific cards to a destination pile.

#### Properties

```csharp
public Deck Deck { get; }
public string ToPileId { get; }
public IReadOnlyList<Card> Cards { get; }
```

#### Constructor

```csharp
public DiscardCardsEvent(
    Deck deck, 
    string toPileId, 
    IReadOnlyList<Card> cards)
```

**Example**:
```csharp
var handCards = state.Piles["hand"].ToList();
var discardEvt = new DiscardCardsEvent(deck, "discard", handCards);
```

---

### PeekCardsEvent

View top N cards from a pile without removing them (read-only).

#### Properties

```csharp
public Deck Deck { get; }
public string PileId { get; }
public int Count { get; }
```

#### Constructor

```csharp
public PeekCardsEvent(Deck deck, string pileId, int count)
```

**Throws**:
- `ArgumentNullException`: If deck or pile ID is null
- `ArgumentOutOfRangeException`: If count is negative

**Example**:
```csharp
var peekEvt = new PeekCardsEvent(deck, "draw", 3);
```

**Note**: Does not modify state. Can be used for scrying/preview mechanics.

---

### RevealCardsEvent

Make specific cards visible to all players (optional visibility tracking).

#### Properties

```csharp
public Deck Deck { get; }
public string PileId { get; }
public IReadOnlyList<Card> Cards { get; }
```

#### Constructor

```csharp
public RevealCardsEvent(
    Deck deck, 
    string pileId, 
    IReadOnlyList<Card> cards)
```

**Example**:
```csharp
var communityCards = state.Piles["inplay"].ToList();
var revealEvt = new RevealCardsEvent(deck, "inplay", communityCards);
```

**Note**: Basic implementation is no-op; visibility tracking can be added via extras.

---

### ReshuffleEvent

Move cards from source pile to destination pile and shuffle.

#### Properties

```csharp
public Deck Deck { get; }
public string FromPileId { get; }
public string ToPileId { get; }
```

#### Constructor

```csharp
public ReshuffleEvent(
    Deck deck, 
    string fromPileId, 
    string toPileId)
```

**Example**:
```csharp
var reshuffleEvt = new ReshuffleEvent(deck, "discard", "draw");
```

**Use case**: Discard-to-draw recycling for continuous play.

---

## Builders

### CardsGameBuilder

Minimal game builder for cards operations.

**Namespace**: `Veggerby.Boards.Cards`

**Inheritance**: `GameBuilder`

#### Predefined Constants

```csharp
public static class Piles
{
    public const string Draw = "draw";
    public const string Discard = "discard";
    public const string Hand = "hand";
    public const string InPlay = "inplay";
}

public static class CardIds
{
    public const string C1 = "card-1";
    public const string C2 = "card-2";
    public const string C3 = "card-3";
    public const string C4 = "card-4";
    public const string C5 = "card-5";
}
```

#### Methods

```csharp
public CreateDeckEvent CreateInitialDeckEvent()
```

Helper to build initial CreateDeckEvent payload with 5 demo cards in draw pile.

**Returns**: CreateDeckEvent configured with demo deck

**Throws**: 
- `InvalidOperationException`: If Build has not been invoked

**Example**:
```csharp
var builder = new CardsGameBuilder();
builder.WithSeed(42UL);
var progress = builder.Compile();
progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
```

#### Wired Events

The builder wires the following events in a single open phase ("cards-ops"):
- CreateDeckEvent
- ShuffleDeckEvent
- DrawCardsEvent
- MoveCardsEvent
- DiscardCardsEvent
- PeekCardsEvent
- RevealCardsEvent
- ReshuffleEvent

---

## Conditions

Event conditions validate events before mutators execute. All are internal implementations.

### Common Validation Rules

- **Deck initialized**: DeckState must exist before operations
- **Valid piles**: Pile IDs must exist in deck definition
- **Sufficient cards**: Draw/move operations require enough cards in source
- **Card presence**: Cards must exist in specified pile(s)
- **Non-negative counts**: Count parameters cannot be negative

### Condition Responses

- `ConditionResponse.Valid`: Event passes validation
- `ConditionResponse.Fail(message)`: Event rejected with reason
- `ConditionResponse.NotApplicable`: Event doesn't apply (rare)

---

## Mutators

State mutators produce new GameState snapshots. All are internal implementations.

### Mutator Guarantees

- **Immutability**: Never modify existing state
- **Determinism**: Same input → same output
- **Allocation-conscious**: Avoid unnecessary allocations in hot paths
- **Pure functions**: No side effects beyond state transformation

---

## Error Handling

### InvalidGameEventException

Thrown when an event fails validation conditions.

**Common causes**:
- Drawing more cards than available
- Moving cards not present in source pile
- Peeking at more cards than exist
- Unknown pile identifiers

**Example**:
```csharp
try
{
    // Try to draw 10 from empty pile
    progress = progress.HandleEvent(new DrawCardsEvent(deck, "draw", "hand", 10));
}
catch (InvalidGameEventException ex)
{
    Console.WriteLine($"Event rejected: {ex.Message}");
}
```

---

## Extension Points

### Custom Piles

```csharp
var customPiles = new[] { "draw", "discard", "hand", "exile", "revealed" };
var deck = new Deck("my-deck", customPiles);
```

### Visibility Tracking (Advanced)

Extend DeckState with extras dictionary:

```csharp
// In custom mutator
var extras = new Dictionary<string, object>
{
    ["revealed"] = new HashSet<string> { "card-1", "card-2" }
};
// Store in DeckState or separate state artifact
```

### Auto-Reshuffle Policy

```csharp
// Custom condition checking draw pile
public class AutoReshuffleCondition : IGameEventCondition<DrawCardsEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, DrawCardsEvent evt)
    {
        var ds = state.GetState<DeckState>(evt.Deck);
        if (ds?.Piles[evt.FromPileId].Count < evt.Count)
        {
            // Trigger reshuffle in phase logic
            return ConditionResponse.NotApplicable;
        }
        return ConditionResponse.Valid;
    }
}
```

---

## See Also

- [Cards Module Overview](index.md) - Core concepts and integration guide
- [Examples](examples.md) - Practical game scenarios
- [Deck-Building Module](/docs/deck-building.md) - Supply mechanics
