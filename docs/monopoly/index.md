# Monopoly Module

## Overview

The Monopoly module implements a simplified version of the classic Monopoly board game, demonstrating economic state management, property ownership, rent collection, cash flow, and player elimination patterns within the Veggerby.Boards framework.

## Features

### Board Topology
- **40-square circular track**: Standard Monopoly board layout
- **Special squares**: Go, Jail, Free Parking, Go To Jail
- **Property squares**: 22 properties across 8 color groups
- **Tax squares**: Income Tax ($200), Luxury Tax ($75)
- **Card squares**: 3 Chance, 3 Community Chest (card functionality deferred)

### Property Types
| Type | Count | Notes |
|------|-------|-------|
| Colored Properties | 22 | 8 color groups |
| Railroads | 4 | Fixed $200 price |
| Utilities | 2 | Electric Company, Water Works |

### Color Groups
| Color | Properties | Monopoly Bonus |
|-------|-----------|----------------|
| Brown | 2 | 2× base rent |
| Light Blue | 3 | 2× base rent |
| Pink | 3 | 2× base rent |
| Orange | 3 | 2× base rent |
| Red | 3 | 2× base rent |
| Yellow | 3 | 2× base rent |
| Green | 3 | 2× base rent |
| Dark Blue | 2 | 2× base rent |

## Rent Calculation

### Colored Properties
```
Rent = BaseRent × (HasMonopoly ? 2 : 1)  // Without houses
Rent = RentSchedule[HouseCount - 1]       // With houses (1-4) or hotel (5)
```
- Base rent varies by property ($2 - $50)
- Monopoly bonus doubles rent when owner owns all properties in color group (without houses)
- Houses/hotels use the rent schedule specific to each property

### House/Hotel Rent Schedule
Example rent values for Mediterranean Avenue ($60):
| Improvement | Rent |
|-------------|------|
| No houses (no monopoly) | $2 |
| No houses (monopoly) | $4 |
| 1 House | $10 |
| 2 Houses | $30 |
| 3 Houses | $90 |
| 4 Houses | $160 |
| Hotel | $250 |

### House Building Costs
| Color Group | House Cost | Hotel Cost |
|-------------|-----------|------------|
| Brown | $50 | $50 + 4 houses |
| Light Blue | $50 | $50 + 4 houses |
| Pink | $100 | $100 + 4 houses |
| Orange | $100 | $100 + 4 houses |
| Red | $150 | $150 + 4 houses |
| Yellow | $150 | $150 + 4 houses |
| Green | $200 | $200 + 4 houses |
| Dark Blue | $200 | $200 + 4 houses |

### Even Building Rule
- Houses must be built evenly across properties in a color group
- Cannot build a second house on one property until all properties have at least one house
- Hotels require 4 houses on all properties in the color group

### Railroads
```
Rent = $25 × 2^(RailroadsOwned - 1)
```
| Railroads Owned | Rent |
|-----------------|------|
| 1 | $25 |
| 2 | $50 |
| 3 | $100 |
| 4 | $200 |

### Utilities
```
Rent = DiceRoll × (UtilitiesOwned == 1 ? 4 : 10)
```
| Utilities Owned | Multiplier |
|-----------------|------------|
| 1 | 4× dice roll |
| 2 | 10× dice roll |

## Player State

### Cash Flow
- **Starting cash**: $1,500
- **Passing Go**: +$200
- **Income Tax**: -$200
- **Luxury Tax**: -$75
- **Property purchase**: -Price
- **House purchase**: -House cost (varies by color group)
- **Rent payment**: -Rent (transferred to property owner)

### Jail Mechanics
- **Go To Jail**: Landing on position 30 or drawing certain cards
- **Release methods**:
  - Roll doubles
  - Pay $50 fine
  - Use Get Out of Jail Free card
- **Maximum jail turns**: 3 (forced $50 fine on 4th turn)

### Bankruptcy
- Triggered when cash goes negative
- Properties transfer to creditor (or bank if bankrupted by tax)
- Player eliminated from game
- Last player standing wins

## Usage

### Basic Setup
```csharp
// Create game with 4 players and default settings
var builder = new MonopolyGameBuilder();
var progress = builder.Compile();

// Get initial player states
var playerStates = builder.CreateInitialPlayerStates(progress);
var state = progress.State.Next(playerStates);
```

### Custom Configuration
```csharp
// Create game with custom settings
var builder = new MonopolyGameBuilder(
    playerCount: 2,
    startingCash: 2000,
    playerNames: new[] { "Alice", "Bob" }
);
```

### Game Events
```csharp
// Movement
var moveEvent = new MovePlayerGameEvent(player, die1: 3, die2: 4);

// Buy property
var buyEvent = new BuyPropertyGameEvent(player, propertyPosition: 1);

// Pay rent
var rentEvent = new PayRentGameEvent(payer, propertyPosition: 1, diceTotal: 7);

// Go to jail
var jailEvent = new GoToJailGameEvent(player);

// Get out of jail
var releaseEvent = new GetOutOfJailGameEvent(player, GetOutOfJailMethod.PaidFine);
```

## Board Layout

```
     20 21 22 23 24 25 26 27 28 29 30
     ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
  19 │FP│KY│CH│IN│IL│BO│AT│VE│WW│MG│GJ│ 31
     ├──┤                          ├──┤
  18 │TN│                          │PA│ 32
     ├──┤                          ├──┤
  17 │CC│                          │NC│ 33
     ├──┤                          ├──┤
  16 │SJ│                          │CC│ 34
     ├──┤                          ├──┤
  15 │PR│      MONOPOLY            │SL│ 35
     ├──┤                          ├──┤
  14 │VA│                          │CH│ 36
     ├──┤                          ├──┤
  13 │ST│                          │PP│ 37
     ├──┤                          ├──┤
  12 │EC│                          │LT│ 38
     ├──┤                          ├──┤
  11 │SC│                          │BW│ 39
     ├──┼──┬──┬──┬──┬──┬──┬──┬──┬──┼──┤
     │JL│CT│VT│CH│OR│RR│IT│BL│CC│ME│GO│
     └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
      10  9  8  7  6  5  4  3  2  1  0
```

Legend:
- GO = Go (Position 0)
- JL = Jail (Position 10)
- FP = Free Parking (Position 20)
- GJ = Go To Jail (Position 30)
- RR = Railroad
- CC = Community Chest
- CH = Chance
- IT = Income Tax
- LT = Luxury Tax
- EC = Electric Company
- WW = Water Works

## Deferred Complexity

The following features are intentionally deferred to keep the implementation focused:

### Not Implemented
- **Auctions**: Property auctions when purchase declined
- **Trading**: Player-to-player property/cash trades
- **Mortgages**: Property mortgaging for cash
- **House Rules**: Free Parking pot, exact landing on Go
- **Card Movement Effects**: Movement-based card effects (Advance to Go, etc.) require additional game flow integration
- **Selling Houses**: Selling houses back to the bank at half price
- **Housing Shortage**: Limited supply of houses/hotels

### Future Enhancements
These features may be added in future versions:
- Auction mechanics
- Trading interface
- Mortgage/unmortgage operations
- House selling and shortage mechanics

## Card System

### Implemented Card Effects
- **CollectFromBank**: Player collects money from the bank
- **PayToBank**: Player pays money to the bank
- **CollectFromPlayers**: Player collects money from all other players
- **PayToPlayers**: Player pays money to all other players
- **GoToJail**: Player goes directly to jail
- **GetOutOfJailFree**: Player keeps the card for later use

### Card Decks
Both Chance (16 cards) and Community Chest (16 cards) are fully defined with standard Monopoly card effects.

### Deferred Card Effects
- **AdvanceToPosition**: Move to a specific position
- **AdvanceToNearestRailroad**: Move to nearest railroad, pay double rent
- **AdvanceToNearestUtility**: Move to nearest utility, pay 10x dice
- **PropertyRepairs**: Pay based on houses/hotels owned

## Module Structure

```
Veggerby.Boards.Monopoly/
├── MonopolyGameBuilder.cs      # Game configuration and setup
├── MonopolyBoardConfiguration.cs # Static board layout data
├── MonopolySquareInfo.cs       # Square metadata record
├── MonopolyOutcomeState.cs     # Game termination state
├── RentCalculator.cs           # Rent calculation logic
├── SquareType.cs               # Square type enumeration
├── PropertyColorGroup.cs       # Color group enumeration
├── Cards/                      # Card system
│   ├── MonopolyCardDecks.cs    # Standard card definitions
│   ├── MonopolyCardDefinition.cs # Card record type
│   └── MonopolyCardEffect.cs   # Card effect enumeration
├── Conditions/                 # Validation conditions
│   ├── AlwaysValidCondition.cs
│   ├── CanBuyPropertyCondition.cs
│   ├── CanDrawCardCondition.cs
│   ├── CanGetOutOfJailCondition.cs
│   ├── MonopolyGameEndedCondition.cs
│   ├── MonopolyGameNotEndedCondition.cs
│   └── MustPayRentCondition.cs
├── Events/                     # Game events
│   ├── BuyPropertyGameEvent.cs
│   ├── DrawCardGameEvent.cs
│   ├── EliminatePlayerGameEvent.cs
│   ├── GetOutOfJailGameEvent.cs
│   ├── GoToJailGameEvent.cs
│   ├── MovePlayerGameEvent.cs
│   ├── PassGoGameEvent.cs
│   ├── PayRentGameEvent.cs
│   └── PayTaxGameEvent.cs
├── Mutators/                   # State mutators
│   ├── BuyPropertyStateMutator.cs
│   ├── DrawCardStateMutator.cs
│   ├── EliminatePlayerStateMutator.cs
│   ├── GetOutOfJailStateMutator.cs
│   ├── GoToJailStateMutator.cs
│   ├── MonopolyEndGameMutator.cs
│   ├── MovePlayerStateMutator.cs
│   ├── PassGoStateMutator.cs
│   ├── PayRentStateMutator.cs
│   └── PayTaxStateMutator.cs
└── States/                     # State types
    ├── MonopolyBoardConfigState.cs
    ├── MonopolyCardDeckState.cs
    ├── MonopolyCardsState.cs
    ├── MonopolyPlayerState.cs
    └── PropertyOwnershipState.cs
```

## Testing

The module includes comprehensive tests for:
- Board configuration and layout
- Rent calculation (all property types)
- Player state management
- Property ownership operations
- Game builder functionality
- Card deck operations (draw, reshuffle, determinism)

Run tests with:
```bash
dotnet test --filter "FullyQualifiedName~Monopoly"
```

## Dependencies

- **Veggerby.Boards**: Core game engine
- **Veggerby.Boards.Cards**: Card module (for future Chance/Community Chest)
