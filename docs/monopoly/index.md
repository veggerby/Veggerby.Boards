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
Rent = $25 × 2^(UnmortgagedRailroadsOwned - 1)
```
| Railroads Owned | Rent |
|-----------------|------|
| 1 | $25 |
| 2 | $50 |
| 3 | $100 |
| 4 | $200 |

*Note: Mortgaged railroads don't count toward rent calculation.*

### Utilities
```
Rent = DiceRoll × (UnmortgagedUtilitiesOwned == 1 ? 4 : 10)
```
| Utilities Owned | Multiplier |
|-----------------|------------|
| 1 | 4× dice roll |
| 2 | 10× dice roll |

*Note: Mortgaged utilities don't count toward rent calculation.*

## Mortgages

### Mortgage Rules
- Only property owner can mortgage
- Cannot mortgage properties with houses/hotels
- Must sell all houses in color group before mortgaging any property in that group
- Mortgage value = 50% of property price
- Mortgaged properties collect no rent

### Unmortgage Rules
- Unmortgage cost = Mortgage value + 10% interest
- All properties in a color group must be unmortgaged before building houses

### Mortgage API
```csharp
// Check if can mortgage
ownership.CanMortgage(position, playerId);

// Mortgage a property
var newState = ownership.Mortgage(position);

// Check if can unmortgage
ownership.CanUnmortgage(position, playerId);

// Unmortgage a property
var newState = ownership.Unmortgage(position);

// Check if mortgaged
ownership.IsMortgaged(position);
```

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

## Auctions

When a player lands on an unowned property and declines to purchase it, an auction may be started.

### Auction Mechanics
- Any player can participate in the auction
- Bids must be higher than the current bid
- Players can pass (decline to bid further)
- Auction ends when all players except the highest bidder have passed
- Winner pays their bid amount and receives the property
- If all players pass without bidding, property remains unowned

### Auction API
```csharp
// Start an auction
var startEvent = new StartAuctionGameEvent(propertyPosition, eligiblePlayers);

// Place a bid
var bidEvent = new BidInAuctionGameEvent(player, bidAmount);

// Pass on the auction
var passEvent = new PassAuctionGameEvent(player);
```

## Trading

Players can trade properties, cash, and Get Out of Jail Free cards with each other.

### Trade Rules
- Cannot trade properties with houses (must sell houses first)
- Trade must include something from at least one side
- Both players must have sufficient assets to complete the trade
- Mortgaged properties can be traded (buyer inherits mortgage)

### Trade API
```csharp
// Propose a trade
var proposeEvent = new ProposeTradeGameEvent(
    proposer: player1,
    target: player2,
    offeredCash: 200,
    offeredProperties: new[] { 1, 3 },
    requestedCash: 0,
    requestedProperties: new[] { 6 });

// Accept or decline
var acceptEvent = new AcceptTradeGameEvent(player2);
var declineEvent = new DeclineTradeGameEvent(player2);
```

## Selling Houses

Players can sell houses back to the bank at half the purchase price.

### Sell House Rules
- Houses sell for 50% of purchase price
- Must follow even selling rule (sell from properties with most houses first)
- All houses must be sold from a color group before mortgaging any property

### Sell House API
```csharp
// Sell a house
var sellEvent = new SellHouseGameEvent(player, propertyPosition);
```

## Deferred Complexity

The following features are intentionally deferred to keep the implementation focused:

### Not Implemented
- **House Rules**: Free Parking pot, exact landing on Go
- **Housing Shortage**: Limited supply of houses/hotels

### Future Enhancements
These features may be added in future versions:
- Housing shortage mechanics
- Additional house rules variants

## Card System

### Implemented Card Effects
- **CollectFromBank**: Player collects money from the bank
- **PayToBank**: Player pays money to the bank
- **CollectFromPlayers**: Player collects money from all other players
- **PayToPlayers**: Player pays money to all other players
- **GoToJail**: Player goes directly to jail
- **GetOutOfJailFree**: Player keeps the card for later use
- **AdvanceToPosition**: Move to a specific position (collect $200 if passing Go)
- **MoveToPosition**: Move to a specific position (do not collect Go)
- **AdvanceToNearestRailroad**: Move to nearest railroad
- **AdvanceToNearestUtility**: Move to nearest utility
- **MoveForward**: Move forward a number of spaces
- **MoveBackward**: Move backward a number of spaces
- **PropertyRepairs**: Pay based on houses/hotels owned ($25/house, $100/hotel)

### Card Decks
Both Chance (16 cards) and Community Chest (16 cards) are fully defined with standard Monopoly card effects.

### Movement Card Examples
```csharp
// Advance to Go (collect $200)
new MonopolyCardDefinition("advance-go", "Advance to Go. Collect $200.", 
    MonopolyCardEffect.AdvanceToPosition, 0);

// Go back 3 spaces
new MonopolyCardDefinition("go-back-3", "Go back 3 spaces.", 
    MonopolyCardEffect.MoveBackward, 3);

// Property repairs
new MonopolyCardDefinition("repairs", "Make general repairs on all your property.", 
    MonopolyCardEffect.PropertyRepairs, 25, 100); // $25/house, $100/hotel
```

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
│   ├── CanBuyHouseCondition.cs
│   ├── CanBuyPropertyCondition.cs
│   ├── CanDrawCardCondition.cs
│   ├── CanGetOutOfJailCondition.cs
│   ├── CanMortgagePropertyCondition.cs
│   ├── CanUnmortgagePropertyCondition.cs
│   ├── MonopolyGameEndedCondition.cs
│   ├── MonopolyGameNotEndedCondition.cs
│   └── MustPayRentCondition.cs
├── Events/                     # Game events
│   ├── BuyHouseGameEvent.cs
│   ├── BuyPropertyGameEvent.cs
│   ├── DrawCardGameEvent.cs
│   ├── EliminatePlayerGameEvent.cs
│   ├── GetOutOfJailGameEvent.cs
│   ├── GoToJailGameEvent.cs
│   ├── MortgagePropertyGameEvent.cs
│   ├── MovePlayerGameEvent.cs
│   ├── MoveToPositionGameEvent.cs
│   ├── PassGoGameEvent.cs
│   ├── PayRentGameEvent.cs
│   ├── PayTaxGameEvent.cs
│   └── UnmortgagePropertyGameEvent.cs
├── Mutators/                   # State mutators
│   ├── BuyHouseStateMutator.cs
│   ├── BuyPropertyStateMutator.cs
│   ├── DrawCardStateMutator.cs
│   ├── EliminatePlayerStateMutator.cs
│   ├── GetOutOfJailStateMutator.cs
│   ├── GoToJailStateMutator.cs
│   ├── MonopolyEndGameMutator.cs
│   ├── MortgagePropertyStateMutator.cs
│   ├── MovePlayerStateMutator.cs
│   ├── MoveToPositionStateMutator.cs
│   ├── PassGoStateMutator.cs
│   ├── PayRentStateMutator.cs
│   ├── PayTaxStateMutator.cs
│   └── UnmortgagePropertyStateMutator.cs
├── Conditions/                 # Additional conditions
│   ├── CanAcceptTradeCondition.cs
│   ├── CanBidInAuctionCondition.cs
│   ├── CanDeclineTradeCondition.cs
│   ├── CanPassAuctionCondition.cs
│   ├── CanProposeTradeCondition.cs
│   ├── CanSellHouseCondition.cs
│   └── CanStartAuctionCondition.cs
├── Events/                     # Additional events
│   ├── AcceptTradeGameEvent.cs
│   ├── BidInAuctionGameEvent.cs
│   ├── DeclineTradeGameEvent.cs
│   ├── PassAuctionGameEvent.cs
│   ├── ProposeTradeGameEvent.cs
│   ├── SellHouseGameEvent.cs
│   └── StartAuctionGameEvent.cs
├── Mutators/                   # Additional mutators
│   ├── AcceptTradeStateMutator.cs
│   ├── BidInAuctionStateMutator.cs
│   ├── DeclineTradeStateMutator.cs
│   ├── PassAuctionStateMutator.cs
│   ├── ProposeTradeStateMutator.cs
│   ├── SellHouseStateMutator.cs
│   └── StartAuctionStateMutator.cs
└── States/                     # State types
    ├── AuctionState.cs
    ├── MonopolyBoardConfigState.cs
    ├── MonopolyCardDeckState.cs
    ├── MonopolyCardsState.cs
    ├── MonopolyPlayerState.cs
    ├── PropertyOwnershipState.cs
    └── TradeProposalState.cs
```

## Testing

The module includes comprehensive tests for:
- Board configuration and layout
- Rent calculation (all property types, with mortgages)
- Player state management
- Property ownership operations
- Mortgage and unmortgage operations
- House/hotel building rules
- Selling houses with even selling rule
- Auction mechanics (bidding, passing, ending)
- Trading (proposing, accepting, declining)
- Game builder functionality
- Card deck operations (draw, reshuffle, determinism)

Run tests with:
```bash
dotnet test --filter "FullyQualifiedName~Monopoly"
```

Test count: 150+ unit tests

## Dependencies

- **Veggerby.Boards**: Core game engine
- **Veggerby.Boards.Cards**: Card module (for future Chance/Community Chest)
