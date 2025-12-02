# Veggerby.Boards.Monopoly

Monopoly module for Veggerby.Boards providing complete economic state management with property ownership, rent collection, cash flow, jail mechanics, house/hotel building, trading, auctions, and player elimination built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards` and `Veggerby.Boards.Cards`. Use when you want a ready Monopoly ruleset or a foundation for economic/property-based board games.

## Install

```bash
dotnet add package Veggerby.Boards.Monopoly
```

## Overview

This module provides a comprehensive, deterministic implementation of Monopoly with:

- Standard 40-square circular board with all property types
- Property ownership with mortgaging and unmortgaging
- Rent calculation with monopoly bonuses and improvements
- House and hotel building with even-building rules
- Railroad and utility rent multipliers
- Jail mechanics with multiple release methods
- Chance and Community Chest card decks (32 cards total)
- Player elimination and bankruptcy tracking
- Auction and trading systems
- Tax squares (Income Tax, Luxury Tax)
- Support for 2-8 players (configurable)

This package does **not** implement AI, UI, or network play.

## Quick Start

```csharp
using Veggerby.Boards.Monopoly;

// Create a 4-player Monopoly game with default settings
var builder = new MonopolyGameBuilder(playerCount: 4);
var progress = builder.Compile();

// Add initial player states with starting cash
var playerStates = builder.CreateInitialPlayerStates(progress);
progress = progress.State.Next(playerStates);

// Roll dice
var dice1 = progress.Game.GetArtifact<Dice>("dice-1");
var dice2 = progress.Game.GetArtifact<Dice>("dice-2");
progress = progress.HandleEvent(new RollDiceGameEvent<int>(
    new DiceState<int>(dice1, 4),
    new DiceState<int>(dice2, 3)));

// Move player (total: 7 squares from Go)
var player = progress.Game.GetPlayer("Player-1");
progress = progress.HandleEvent(new MovePlayerGameEvent(player, 4, 3));

// Buy property if landed on unowned property
if (CanBuyProperty(progress, player, position: 7))
{
    progress = progress.HandleEvent(new BuyPropertyGameEvent(player, 7));
}

// End turn
progress = progress.HandleEvent(new EndTurnGameEvent());

// Check if game is over
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome() as MonopolyOutcomeState;
    Console.WriteLine($"Winner: {outcome.Winner.Id}");
}
```

## Key Concepts

### Board Topology

The Monopoly board consists of 40 squares:
- **22 Colored Properties**: 8 color groups (Brown, Light Blue, Pink, Orange, Red, Yellow, Green, Dark Blue)
- **4 Railroads**: $200 each, rent scales with ownership count
- **2 Utilities**: Electric Company, Water Works (rent based on dice roll)
- **Special Squares**: Go, Jail/Just Visiting, Free Parking, Go To Jail
- **Tax Squares**: Income Tax ($200), Luxury Tax ($75)
- **Card Squares**: 3 Chance, 3 Community Chest

### Property Types

| Type | Count | Base Price | Notes |
|------|-------|------------|-------|
| Colored Properties | 22 | $60-$400 | 8 color groups |
| Railroads | 4 | $200 | $25-$200 rent |
| Utilities | 2 | $150 | 4× or 10× dice roll |

### Rent Calculation

**Colored Properties**:
```
Rent = BaseRent × (HasMonopoly ? 2 : 1)  // Without houses
Rent = RentSchedule[HouseCount]          // With houses/hotel
```

**Railroads**:
```
Rent = $25 × 2^(UnmortgagedCount - 1)    // $25, $50, $100, $200
```

**Utilities**:
```
Rent = DiceTotal × (TwoOwned ? 10 : 4)
```

### Events & Mutators

| Event | Mutator | Description |
|-------|---------|-------------|
| `RollDiceGameEvent<int>` | `DiceStateMutator<int>` | Rolls both dice |
| `MovePlayerGameEvent` | `MovePlayerStateMutator` | Moves token, handles passing Go |
| `BuyPropertyGameEvent` | `BuyPropertyStateMutator` | Purchases unowned property |
| `PayRentGameEvent` | `PayRentStateMutator` | Pays rent to property owner |
| `PayTaxGameEvent` | `PayTaxStateMutator` | Pays Income/Luxury Tax |
| `GoToJailGameEvent` | `GoToJailStateMutator` | Sends player to jail |
| `GetOutOfJailGameEvent` | `GetOutOfJailStateMutator` | Releases from jail (various methods) |
| `BuyHouseGameEvent` | `BuyHouseStateMutator` | Adds house/hotel to property |
| `SellHouseGameEvent` | `SellHouseStateMutator` | Removes house/hotel from property |
| `MortgagePropertyGameEvent` | `MortgagePropertyStateMutator` | Mortgages property for cash |
| `UnmortgagePropertyGameEvent` | `UnmortgagePropertyStateMutator` | Unmortgages property |
| `DrawCardGameEvent` | `DrawCardStateMutator` | Draws Chance/Community Chest card |
| `StartAuctionGameEvent` | `StartAuctionStateMutator` | Starts property auction |
| `BidInAuctionGameEvent` | `BidInAuctionStateMutator` | Places auction bid |
| `ProposeTradeGameEvent` | `ProposeTradeStateMutator` | Proposes player trade |
| `AcceptTradeGameEvent` | `AcceptTradeStateMutator` | Accepts trade proposal |
| `EliminatePlayerGameEvent` | `EliminatePlayerStateMutator` | Eliminates bankrupt player |
| `EndTurnGameEvent` | `EndTurnStateMutator` | Ends current turn |

### Jail Mechanics

Players can get out of jail by:
1. Rolling doubles on their turn
2. Paying $50 fine
3. Using "Get Out of Jail Free" card
4. After 3 failed attempts, must pay $50

### House Building Rules

- **Monopoly Required**: Must own all properties in color group
- **Even Building**: Houses must be built evenly across the color group
- **All Unmortgaged**: Cannot build while any property in group is mortgaged
- **Hotel**: Requires 4 houses on all properties in group

### Card System

**Chance** (16 cards): Movement, payments, repairs
**Community Chest** (16 cards): Payments, collections, jail cards

Card decks are shuffled deterministically using a seeded algorithm.

## Configuration Options

```csharp
// Default: 4 players, $1500 starting cash
var builder = new MonopolyGameBuilder();

// Custom configuration
var builder = new MonopolyGameBuilder(
    playerCount: 6,
    startingCash: 2000,
    playerNames: new[] { "Alice", "Bob", "Carol", "Dave", "Eve", "Frank" },
    cardShuffleSeed: 12345  // For deterministic card order
);
```

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Monopoly"
```

**Test Coverage** (150+ unit tests):
- Board configuration and layout validation
- Rent calculation (all property types, with mortgages)
- Player state management (cash, position, jail status)
- Property ownership operations (buy, mortgage, unmortgage)
- House/hotel building rules (even building, monopoly requirement)
- Selling houses with even selling rule
- Auction mechanics (bidding, passing, ending)
- Trading (proposing, accepting, declining)
- Card deck operations (draw, reshuffle, determinism)
- Game builder functionality

## Known Limitations

- **Housing Shortage**: Unlimited houses/hotels available (no scarcity)
- **House Rules**: Free Parking pot, exact Go landing bonus not implemented
- **Speed Die**: Not implemented
- **Incomplete Bankruptcy Resolution**: Some edge cases in asset transfer

## Extending This Module

Common extension scenarios:

### Adding Housing Shortage

```csharp
public sealed record HousingInventoryState(int AvailableHouses, int AvailableHotels);

public sealed class HousingShortageCondition : IGameStateCondition
{
    public ConditionResponse Evaluate(Game game, GameState state)
    {
        // Check available housing inventory before allowing purchase
    }
}
```

### Implementing Speed Die

```csharp
public sealed class SpeedDieMonopolyBuilder : MonopolyGameBuilder
{
    protected override void Build()
    {
        base.Build();
        
        AddDice("speed-die").HasNoValue();
        // Add speed die rules (Mr. Monopoly, bus, 1-2-3)
    }
}
```

### Adding Custom House Rules

```csharp
public sealed class FreeParkingPotMonopolyBuilder : MonopolyGameBuilder
{
    protected override void Build()
    {
        base.Build();
        
        WithState(new FreeParkingPotState(initialAmount: 500));
        // Route tax payments to pot, award on landing
    }
}
```

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines
- **Game Termination**: See [/docs/game-termination.md](../../docs/game-termination.md) for outcome patterns
- **Module Documentation**: See [/docs/monopoly/index.md](../../docs/monopoly/index.md) for detailed game rules
- **Cards Module**: See [/src/Veggerby.Boards.Cards/README.md](../Veggerby.Boards.Cards/README.md) for card primitives

## Versioning

Semantic versioning aligned with repository releases. Breaking rule or API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
