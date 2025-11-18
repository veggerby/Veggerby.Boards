# GameBuilder Guide

A comprehensive guide to creating new game modules using the Veggerby.Boards engine.

## Table of Contents

- [Introduction](#introduction)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [GameBuilder API Surface](#gamebuilder-api-surface)
- [Tile Graph Construction](#tile-graph-construction)
- [Pieces & Movement Patterns](#pieces--movement-patterns)
- [Dice & Randomness](#dice--randomness)
- [Initial State Setup](#initial-state-setup)
- [Phases & Rules](#phases--rules)
- [Custom Events, Mutators, and Conditions](#custom-events-mutators-and-conditions)
- [Compilation & Execution](#compilation--execution)
- [Testing Strategy](#testing-strategy)
- [Common Patterns by Game Type](#common-patterns-by-game-type)
- [Common Pitfalls](#common-pitfalls)
- [Example: Implementing Ludo](#example-implementing-ludo)

## Introduction

### What is a Game Module?

A game module is a self-contained package that implements a specific board game using the Veggerby.Boards engine. Modules provide:

- **Game definition**: Board topology, pieces, players, and initial state
- **Rule implementation**: Events, mutators, and conditions that enforce game rules
- **Phase structure**: Turn sequencing and game flow control
- **Testing**: Unit and integration tests validating behavior

Examples: `Veggerby.Boards.Chess`, `Veggerby.Boards.Go`, `Veggerby.Boards.Backgammon`, `Veggerby.Boards.DeckBuilding`

### When to Create a New Module vs. Extending Existing

**Create a new module when:**
- The game has fundamentally different mechanics (e.g., Go vs. Chess)
- The board topology is unique (e.g., Go's intersections vs. Chess's squares)
- The game deserves its own namespace and package

**Extend an existing module when:**
- Implementing a variant of an existing game (e.g., Chess960)
- Adding optional rules or modes (e.g., different scoring methods)
- Customizing initial setup (e.g., custom Chess position)

### Prerequisites

Before creating a game module, you should understand:

1. **Core Concepts** - Read [core-concepts.md](core-concepts.md) to understand:
   - Artifacts (immutable identities)
   - GameState (immutable state snapshots)
   - Events (declarative intents)
   - Rules & Conditions (validation)
   - Mutators (pure state transformations)
   - Phases (conditional scopes)

2. **Determinism** - Read [determinism-rng-timeline.md](determinism-rng-timeline.md):
   - No hidden randomness
   - Dice artifacts encapsulate RNG
   - Same inputs → same outputs

3. **Phase-Based Design** - Read [phase-based-design.md](phase-based-design.md):
   - Phase hierarchies and ordering
   - Conditional phase activation
   - Endgame detection patterns

## Project Structure

Standard module structure:

```
src/
  Veggerby.Boards.<GameName>/
    ├── <GameName>GameBuilder.cs       # Entry point - defines game structure
    ├── <GameName>StateExtras.cs       # Game-specific state (optional)
    ├── <GameName>OutcomeState.cs      # Game outcome (implements IGameOutcome)
    ├── Events/                        # Custom events (if needed)
    │   ├── <Custom>GameEvent.cs
    │   └── ...
    ├── Mutators/                      # Custom mutators
    │   ├── <Custom>StateMutator.cs
    │   └── ...
    ├── Conditions/                    # Custom conditions (optional)
    │   ├── <Custom>Condition.cs
    │   └── ...
    ├── README.md                      # Module documentation
    └── Veggerby.Boards.<GameName>.csproj

test/
  Veggerby.Boards.Tests/
    └── <GameName>/
        ├── <GameName>GameBuilderTests.cs
        ├── <GameName>FullGameTests.cs
        └── ...

samples/                               # Optional demo
  <GameName>Demo/
    ├── Program.cs
    └── <GameName>Demo.csproj
```

## GameBuilder API Surface

The `GameBuilder` abstract class provides fluent methods for defining your game.

### Foundational Setup

```csharp
public class LudoGameBuilder : GameBuilder
{
    protected override void Build()
    {
        // 1. Set unique board identifier
        BoardId = "ludo";
        
        // 2. Define players (order matters for turn rotation)
        AddPlayer("red");
        AddPlayer("blue");
        AddPlayer("green");
        AddPlayer("yellow");
        
        // 3. Define directions (for tile relations)
        AddDirection("clockwise");
        AddDirection("counter-clockwise");  // if bidirectional
        
        // Continue with tiles, pieces, rules...
    }
}
```

### Board ID

```csharp
BoardId = "unique-identifier";
```

The board ID should be:
- Lowercase with hyphens (kebab-case)
- Descriptive of the game
- Unique across all modules

Examples: `"chess"`, `"go-19x19"`, `"backgammon"`, `"ludo"`

### Players

```csharp
AddPlayer("player-id");
```

Add players in the order they should take turns. Player IDs should be:
- Lowercase descriptive names
- Color names for traditional games: `"white"`, `"black"`, `"red"`, `"blue"`
- Numbered for generic games: `"player-1"`, `"player-2"`

### Directions

```csharp
AddDirection("direction-id");
```

Directions define tile relation semantics. Common patterns:

**Cardinal Directions** (Chess, Go, grid-based games):
```csharp
AddDirection("north");
AddDirection("south");
AddDirection("east");
AddDirection("west");
AddDirection("north-east");  // Diagonals
AddDirection("north-west");
AddDirection("south-east");
AddDirection("south-west");
```

**Circular Track** (Ludo, Monopoly):
```csharp
AddDirection("clockwise");
```

**Custom Directions** (game-specific):
```csharp
AddDirection("forward");
AddDirection("backward");
```

## Tile Graph Construction

Tiles form the board topology. Relations between tiles define legal movement paths.

### Adding Tiles

```csharp
var tile = AddTile("tile-id");
```

Tile IDs should be:
- Descriptive and systematic
- Grid-based: `"tile-x-y"` or algebraic notation like `"e4"`
- Track-based: `"track-0"`, `"track-1"`, etc.
- Named locations: `"jail"`, `"go"`, `"home"`

### Tile Relations

```csharp
tile.WithRelationTo("target-tile-id").InDirection("direction-id");
```

Relations are **unidirectional** by default. For bidirectional relations, define both:

```csharp
AddTile("a").WithRelationTo("b").InDirection("forward");
AddTile("b").WithRelationTo("a").InDirection("backward");
```

### Common Patterns

#### Grid Topology (Chess, Go)

```csharp
for (int x = 1; x <= 8; x++)
{
    for (int y = 1; y <= 8; y++)
    {
        var tile = AddTile($"tile-{x}-{y}");
        
        // Horizontal relations
        if (x > 1)
            tile.WithRelationTo($"tile-{x-1}-{y}").InDirection("west");
        if (x < 8)
            tile.WithRelationTo($"tile-{x+1}-{y}").InDirection("east");
        
        // Vertical relations
        if (y > 1)
            tile.WithRelationTo($"tile-{x}-{y-1}").InDirection("south");
        if (y < 8)
            tile.WithRelationTo($"tile-{x}-{y+1}").InDirection("north");
        
        // Diagonal relations (if needed)
        if (x > 1 && y > 1)
            tile.WithRelationTo($"tile-{x-1}-{y-1}").InDirection("south-west");
        // ... other diagonals
    }
}
```

#### Ring/Track Topology (Ludo, Monopoly)

```csharp
// Create ring of tiles
for (int i = 0; i < 52; i++)
{
    var tile = AddTile($"track-{i}");
    if (i > 0)
    {
        tile.WithRelationTo($"track-{i-1}").InDirection("clockwise");
    }
}

// Close the ring
AddTile("track-0").WithRelationTo("track-51").InDirection("clockwise");
```

#### Branching Paths (Ludo home stretches)

```csharp
// Main track tile 50 branches to red home
AddTile("red-home-1")
    .WithRelationFrom("track-50").InDirection("clockwise");

AddTile("red-home-2")
    .WithRelationFrom("red-home-1").InDirection("clockwise");

// Continue home stretch...
```

#### Graph Topology (Risk territories)

```csharp
// Define territories
AddTile("alaska");
AddTile("northwest-territory");
AddTile("greenland");

// Define adjacencies (bidirectional)
AddTile("alaska")
    .WithRelationTo("northwest-territory").InDirection("adjacent");
AddTile("northwest-territory")
    .WithRelationTo("alaska").InDirection("adjacent");

AddTile("alaska")
    .WithRelationTo("kamchatka").InDirection("adjacent");  // Cross-board connection
AddTile("kamchatka")
    .WithRelationTo("alaska").InDirection("adjacent");
```

## Pieces & Movement Patterns

Pieces are the game tokens that move around the board.

### Adding Pieces

```csharp
var piece = AddPiece("piece-id");
```

### Ownership

```csharp
piece.WithOwner("player-id");
```

Example:
```csharp
AddPiece("red-token-1").WithOwner("red");
AddPiece("blue-token-1").WithOwner("blue");
```

### Movement Patterns

#### Simple Directional Movement

Move one step in a direction:

```csharp
AddPiece("checker")
    .WithOwner("white")
    .HasDirection("north-east");  // Can move one step northeast
```

#### Repeating Directional Movement (Sliding)

Move multiple steps in a direction until blocked:

```csharp
AddPiece("rook")
    .WithOwner("white")
    .HasDirection("north").CanRepeat()  // Slide north
    .HasDirection("south").CanRepeat()  // Slide south
    .HasDirection("east").CanRepeat()   // Slide east
    .HasDirection("west").CanRepeat();  // Slide west
```

#### Fixed Pattern (Chess Knight)

Move in a specific sequence of directions:

```csharp
AddPiece("knight")
    .WithOwner("white")
    .HasPattern("north", "north", "east")   // L-shape: two north, one east
    .HasPattern("north", "north", "west")   // L-shape: two north, one west
    .HasPattern("south", "south", "east")   // ... and so on
    .HasPattern("south", "south", "west")
    .HasPattern("east", "east", "north")
    .HasPattern("east", "east", "south")
    .HasPattern("west", "west", "north")
    .HasPattern("west", "west", "south");
```

#### Multi-Direction Patterns

Combine multiple directional moves:

```csharp
AddPiece("queen")
    .WithOwner("white")
    .HasMultiDirection("north", "south", "east", "west",
                       "north-east", "north-west", "south-east", "south-west")
    .CanRepeat();  // Can slide in all 8 directions
```

### Piece Metadata

You can store additional piece metadata for use in conditions:

```csharp
AddPiece("pawn-1")
    .WithOwner("white")
    .WithMetadata("role", "pawn")      // For identifying piece types
    .WithMetadata("color", "white");   // For color-based rules
```

## Dice & Randomness

Dice artifacts encapsulate randomness deterministically.

### Adding Dice

```csharp
var dice = AddDice<int>("d6");  // Generic type = value type (int, string, etc.)
```

### Initial Dice State

```csharp
// No value initially (must roll first)
dice.HasNoValue();

// Or start with a specific value (for testing)
WithDice("d6").HasValue(6);
```

### Dice in Rules

Dice values are accessed via `DiceState<T>` in the game state:

```csharp
// In a condition:
var diceState = gameState.GetDiceState(dice);
if (diceState?.Value == 6)
{
    // Allow entry from start
}

// In a mutator:
var rolledValue = @event.Value;  // From RollDiceGameEvent<T>
return gameState.Next([new DiceState<int>(dice, rolledValue)]);
```

### Multiple Dice

```csharp
AddDice<int>("d6-1").HasNoValue();
AddDice<int>("d6-2").HasNoValue();
```

Access both dice for combined logic (e.g., Backgammon doubles).

## Initial State Setup

Configure the initial board state using `With*` methods.

### Placing Pieces

```csharp
WithPiece("white-pawn-1").OnTile("e2");
WithPiece("white-rook-1").OnTile("a1");
WithPiece("black-king").OnTile("e8");
```

### Setting Dice Values

```csharp
WithDice("d6").HasValue(4);  // Mostly for testing
```

### Custom State Extras

Many games need custom state beyond pieces and dice:

```csharp
public sealed record LudoStateExtras(int TurnsWithoutCapture);

// In Build():
WithState(new LudoStateExtras(TurnsWithoutCapture: 0));
```

Examples from existing modules:
- **Chess**: `ChessStateExtras` (castling rights, en passant target)
- **Go**: `GoStateExtras` (ko tile, board size)
- **DeckBuilding**: `DeckSupplyStats` (supply pile counts)

## Phases & Rules

Phases structure game flow and gate event processing.

### Basic Phase Structure

```csharp
AddGamePhase("phase-name")
    .If<SomeCondition>()       // Phase activation condition
    .Then()
        .All()                 // Process all matching events
        .ForEvent<SomeEvent>()
            .If<EventCondition>()
            .Then()
                .Do<SomeMutator>();
```

### Phase Conditions

Phases can be conditionally active:

```csharp
// Always active
AddGamePhase("play")
    .If<NullGameStateCondition>()  // Always true
    .Then()
        // ...

// Active when specific condition met
AddGamePhase("bearing-off")
    .If<AllPiecesInHomeBoardCondition>()
    .Then()
        // ...

// Active when dice have no value (need to roll)
AddGamePhase("roll-phase")
    .If<NoDiceValueCondition>()
    .Then()
        // ...
```

### Event Rules

Rules specify how events transform state:

```csharp
.ForEvent<MovePieceGameEvent>()
    .If<PieceOwnedByActivePlayerCondition>()   // Who can move
    .And<DestinationIsEmptyCondition>()        // Where can move
    .And<PathIsValidCondition>()               // How can move
    .Then()
        .Do<MovePieceStateMutator>()           // Execute move
        .Do<NextPlayerStateMutator>();         // Rotate player
```

### Mutator Chaining

Multiple mutators execute in order:

```csharp
.Then()
    .Do<MovePieceStateMutator>()        // 1. Move the piece
    .Do<CaptureOpponentPieceStateMutator>()  // 2. Remove captured piece
    .Do<CheckWinConditionStateMutator>()     // 3. Check for win
    .Do<NextPlayerStateMutator>();           // 4. Rotate player
```

### Multi-Phase Games

Complex games have multiple phases per turn:

```csharp
// Deck-building game
AddGamePhase("db-setup")
    .If<NoDecksInitializedCondition>()
    .Then()
        .ForEvent<CreateDeckEvent>()
            .Then().Do<CreateDeckStateMutator>();

AddGamePhase("db-action")
    .If<HasDecksCondition>()
    .Then()
        .ForEvent<DrawCardEvent>()
            .Then().Do<DrawCardStateMutator>()
        .ForEvent<TrashCardEvent>()
            .Then().Do<TrashCardStateMutator>();

AddGamePhase("db-buy")
    .If<HasDecksCondition>()
    .Then()
        .ForEvent<GainFromSupplyEvent>()
            .Then().Do<GainFromSupplyStateMutator>();

AddGamePhase("db-cleanup")
    .If<HasDecksCondition>()
    .Then()
        .ForEvent<CleanupEvent>()
            .Then().Do<CleanupStateMutator>();
```

### Endgame Detection

Use `.WithEndGameDetection()` for automatic checkmate/stalemate detection:

```csharp
AddGamePhase("play")
    .WithEndGameDetection()  // Checks if game should end
    .Then()
        .All()
        .ForEvent<...>()
            // ...
```

See [phase-based-design.md](phase-based-design.md) for patterns.

## Custom Events, Mutators, and Conditions

### When to Create Custom Types

**Events**: When declarative intent differs from core events
- Core events: `MovePieceGameEvent`, `RollDiceGameEvent<T>`
- Custom: `PlaceStoneGameEvent` (Go), `CastlingGameEvent` (Chess)

**Mutators**: When state transformations are game-specific
- Core: `MovePieceStateMutator`, `DiceStateMutator<T>`
- Custom: `EnPassantCapturePieceStateMutator` (Chess), `PlaceStoneStateMutator` (Go)

**Conditions**: When validation logic is domain-specific
- Generic: `PieceOwnedByActivePlayerCondition`, `TileIsEmptyCondition`
- Custom: `CastlingKingSafetyCondition` (Chess), `SuicideRuleCondition` (Go)

### Custom Event Pattern

```csharp
/// <summary>
/// Represents capturing an opponent's piece at a specific tile.
/// </summary>
/// <param name="AttackingPiece">The piece making the capture.</param>
/// <param name="TargetTile">The tile where the capture occurs.</param>
public sealed record CaptureOpponentPieceEvent(
    Piece AttackingPiece,
    Tile TargetTile
) : IGameEvent;
```

Key principles:
- Immutable record with positional parameters
- Declarative (what to do, not how)
- XML docs with summary and parameters
- Implements `IGameEvent`

### Custom Mutator Pattern

```csharp
/// <summary>
/// Removes opponent's piece from the target tile and returns updated state.
/// </summary>
public sealed class CaptureOpponentPieceMutator 
    : IStateMutator<CaptureOpponentPieceEvent>
{
    public GameState MutateState(
        GameEngine engine,
        GameState gameState,
        CaptureOpponentPieceEvent @event)
    {
        // 1. Find the piece on the target tile
        var targetPieceState = gameState.GetStates<PieceState>()
            .FirstOrDefault(ps => ps.Tile?.Equals(@event.TargetTile) ?? false);
        
        if (targetPieceState == null)
        {
            // No piece to capture - return original state
            return gameState;
        }
        
        // 2. Remove piece from board (set Tile to null)
        var capturedPieceState = new PieceState(
            targetPieceState.Artifact,
            tile: null  // Off the board
        );
        
        // 3. Return new state snapshot
        return gameState.Next([capturedPieceState]);
    }
}
```

Key principles:
- **Pure function**: No side effects
- **Immutable**: Return new `GameState`, never mutate input
- **Idempotent**: Return original state if no change needed
- **Fast**: Avoid LINQ in hot paths (use explicit loops if needed)

### Custom Condition Pattern

```csharp
/// <summary>
/// Validates that the target tile contains an opponent's piece.
/// </summary>
public sealed class CanCaptureOpponentCondition 
    : IGameEventCondition<CaptureOpponentPieceEvent>
{
    public ConditionResponse Evaluate(
        Game game,
        GameState state,
        CaptureOpponentPieceEvent @event)
    {
        // 1. Get active player
        if (!state.TryGetActivePlayer(out var activePlayer))
        {
            return ConditionResponse.NotApplicable();
        }
        
        // 2. Find piece on target tile
        var targetPieceState = state.GetStates<PieceState>()
            .FirstOrDefault(ps => ps.Tile?.Equals(@event.TargetTile) ?? false);
        
        if (targetPieceState == null)
        {
            return ConditionResponse.Invalid("No piece on target tile");
        }
        
        // 3. Check ownership
        if (targetPieceState.Artifact.Owner?.Equals(activePlayer) ?? false)
        {
            return ConditionResponse.Invalid("Cannot capture own piece");
        }
        
        return ConditionResponse.Valid();
    }
}
```

Condition responses:
- **Valid**: Event is allowed
- **Invalid**: Event is not allowed (blocks event, may throw exception)
- **Ignore**: Event doesn't apply in this context (skip silently)
- **NotApplicable**: Condition cannot be evaluated (rare)

## Compilation & Execution

### Compiling a Game

```csharp
var builder = new LudoGameBuilder();
var progress = builder.Compile();  // Returns GameProgress
```

`GameProgress` contains:
- `Game`: The compiled game definition
- `State`: Current game state
- `History`: Previous states (zipper pattern)

### Executing Events

```csharp
// Execute a single event
progress = progress.HandleEvent(new RollDiceGameEvent<int>(dice, 6));

// Check for errors (InvalidGameEventException thrown automatically)
try
{
    progress = progress.HandleEvent(someEvent);
}
catch (InvalidGameEventException ex)
{
    Console.WriteLine($"Invalid move: {ex.Message}");
}
```

### Checking Game Status

```csharp
// Check if game has ended
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome();
    if (outcome is LudoOutcomeState ludoOutcome)
    {
        Console.WriteLine($"Winner: {ludoOutcome.Winner.Id}");
    }
}
```

### Accessing Game State

```csharp
// Get piece states
var pieceStates = progress.State.GetStates<PieceState>();
foreach (var ps in pieceStates)
{
    Console.WriteLine($"{ps.Artifact.Id} on {ps.Tile?.Id ?? "none"}");
}

// Get dice state
var diceState = progress.State.GetDiceState(dice);
Console.WriteLine($"Dice value: {diceState?.Value}");

// Get active player
if (progress.State.TryGetActivePlayer(out var activePlayer))
{
    Console.WriteLine($"Active player: {activePlayer.Id}");
}

// Get custom state extras
var extras = progress.State.GetState<LudoStateExtras>();
Console.WriteLine($"Turns without capture: {extras?.TurnsWithoutCapture}");
```

## Testing Strategy

### Test Organization

```
test/Veggerby.Boards.Tests/<GameName>/
├── <GameName>GameBuilderTests.cs          # Builder initialization
├── <GameName>EventConditionTests.cs       # Event validation
├── <GameName>StateMutatorTests.cs         # State transformations
├── <GameName>PhaseTests.cs                # Phase gating
└── <GameName>FullGameTests.cs             # Integration tests
```

### Unit Test Pattern (AAA)

```csharp
[Fact]
public void GivenValidDiceRoll_WhenHandleEvent_ThenDiceStateUpdated()
{
    // arrange
    var builder = new LudoGameBuilder();
    var progress = builder.Compile();
    var dice = progress.Game.GetArtifact<Dice<int>>("d6");
    var @event = new RollDiceGameEvent<int>(dice, 6);

    // act
    var updated = progress.HandleEvent(@event);

    // assert
    var diceState = updated.State.GetDiceState(dice);
    diceState.Should().NotBeNull();
    diceState!.Value.Should().Be(6);
}
```

### Condition Testing

Test all branches:

```csharp
[Fact]
public void GivenValidMove_WhenEvaluate_ThenValid()
{
    // arrange
    var game = CreateTestGame();
    var state = CreateTestState();
    var condition = new CanMoveCondition();
    var @event = new MovePieceGameEvent(piece, path);

    // act
    var response = condition.Evaluate(game, state, @event);

    // assert
    response.Should().Be(ConditionResponse.Valid());
}

[Fact]
public void GivenInvalidMove_WhenEvaluate_ThenInvalid()
{
    // arrange (setup invalid scenario)
    // act
    var response = condition.Evaluate(game, state, @event);

    // assert
    response.Status.Should().Be(ResponseStatus.Invalid);
    response.Reason.Should().Contain("blocked");
}

[Fact]
public void GivenNotApplicable_WhenEvaluate_ThenIgnore()
{
    // arrange (setup not-applicable scenario)
    // act
    var response = condition.Evaluate(game, state, @event);

    // assert
    response.Should().Be(ConditionResponse.Ignore());
}
```

### Mutator Testing

```csharp
[Fact]
public void GivenMovePiece_WhenMutate_ThenPieceRelocated()
{
    // arrange
    var engine = CreateTestEngine();
    var state = CreateTestState();
    var mutator = new MovePieceStateMutator();
    var @event = new MovePieceGameEvent(piece, path);

    // act
    var newState = mutator.MutateState(engine, state, @event);

    // assert
    newState.Should().NotBe(state);  // New state object
    var pieceState = newState.GetState<PieceState>(piece);
    pieceState.Tile.Should().Be(targetTile);
}
```

### Integration Test Pattern

```csharp
[Fact]
public void Should_play_complete_game_without_errors()
{
    // arrange
    var builder = new LudoGameBuilder();
    var progress = builder.Compile();

    // act & assert - play through a complete game
    var dice = progress.Game.GetArtifact<Dice<int>>("d6");
    
    // Roll to enter
    progress = progress.HandleEvent(new RollDiceGameEvent<int>(dice, 6));
    
    // Move piece
    var piece = progress.Game.GetPiece("red-token-1");
    var path = ResolvePath(progress.Game, "start-red", "track-0");
    progress = progress.HandleEvent(new MovePieceGameEvent(piece, path));
    
    // Continue playing...
    
    // Eventually reach end condition
    progress.IsGameOver().Should().BeTrue();
    var outcome = progress.GetOutcome();
    outcome.Should().NotBeNull();
}
```

### Test Helpers

Create reusable helpers:

```csharp
public static class LudoTestHelpers
{
    public static GameProgress CreateGameAtPosition(
        Dictionary<string, string> piecePlacements)
    {
        var builder = new LudoGameBuilder();
        var progress = builder.Compile();
        
        foreach (var (pieceId, tileId) in piecePlacements)
        {
            // Move piece to position
        }
        
        return progress;
    }
    
    public static TilePath ResolvePath(Game game, string fromId, string toId)
    {
        var from = game.GetTile(fromId);
        var to = game.GetTile(toId);
        var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
        return visitor.ResultPath;
    }
}
```

## Common Patterns by Game Type

### Turn-Based Racing Games (Ludo, Parcheesi, Backgammon)

**Characteristics:**
- Linear or ring track topology
- Dice-driven movement
- Safe spaces and capture mechanics
- Home stretch branching
- Win condition: all pieces reach goal

**Builder Pattern:**
```csharp
protected override void Build()
{
    BoardId = "ludo";
    
    // Players
    AddPlayer("red");
    AddPlayer("blue");
    AddPlayer("green");
    AddPlayer("yellow");
    
    // Directions
    AddDirection("clockwise");
    
    // Main track (ring)
    for (int i = 0; i < 52; i++)
    {
        var tile = AddTile($"track-{i}");
        if (i > 0)
            tile.WithRelationTo($"track-{i-1}").InDirection("clockwise");
    }
    AddTile("track-0").WithRelationTo("track-51").InDirection("clockwise");
    
    // Home stretches (branching from main track)
    foreach (var color in new[] { "red", "blue", "green", "yellow" })
    {
        for (int i = 1; i <= 5; i++)
        {
            var tile = AddTile($"{color}-home-{i}");
            if (i == 1)
                tile.WithRelationFrom($"track-{GetHomeEntryPoint(color)}").InDirection("clockwise");
            else
                tile.WithRelationFrom($"{color}-home-{i-1}").InDirection("clockwise");
        }
    }
    
    // Pieces
    foreach (var color in new[] { "red", "blue", "green", "yellow" })
    {
        for (int i = 1; i <= 4; i++)
        {
            AddPiece($"{color}-token-{i}")
                .WithOwner(color)
                .HasDirection("clockwise").CanRepeat();
        }
    }
    
    // Dice
    AddDice<int>("d6").HasNoValue();
    
    // Phases
    AddGamePhase("roll")
        .If<NoDiceValueCondition>()
        .Then()
            .ForEvent<RollDiceGameEvent<int>>()
                .Then().Do<DiceStateMutator<int>>();
    
    AddGamePhase("move")
        .If<HasDiceValueCondition>()
        .Then()
            .ForEvent<MovePieceGameEvent>()
                .If<ValidMoveCondition>()
                .Then()
                    .Do<MovePieceStateMutator>()
                    .Do<CaptureOpponentMutator>()
                    .Do<ClearDiceStateMutator>()
                    .Do<NextPlayerStateMutator>();
}
```

### Territory Control Games (Risk, Go)

**Characteristics:**
- Graph topology (territories/intersections)
- Ownership transfer
- Capture/conquest mechanics
- Multiple game phases (reinforcement, attack, etc.)

**Builder Pattern:**
```csharp
protected override void Build()
{
    BoardId = "risk";
    
    // Players
    AddPlayer("player-1");
    AddPlayer("player-2");
    // ...
    
    // Direction for adjacency
    AddDirection("adjacent");
    
    // Territories as tiles
    foreach (var territory in TerritoryList)
    {
        AddTile(territory.Id);
    }
    
    // Adjacencies (bidirectional)
    foreach (var (from, to) in Adjacencies)
    {
        AddTile(from).WithRelationTo(to).InDirection("adjacent");
        AddTile(to).WithRelationFrom(from).InDirection("adjacent");
    }
    
    // Army tokens (pieces)
    for (int i = 1; i <= 100; i++)  // Pool of armies
    {
        AddPiece($"army-{i}");
    }
    
    // Phases
    AddGamePhase("reinforcement")
        .Then()
            .ForEvent<PlaceArmyEvent>()
                .Then().Do<PlaceArmyStateMutator>();
    
    AddGamePhase("attack")
        .Then()
            .ForEvent<AttackTerritoryEvent>()
                .Then()
                    .Do<CombatResolutionStateMutator>()
                    .Do<TransferOwnershipStateMutator>();
    
    AddGamePhase("fortify")
        .Then()
            .ForEvent<MoveArmiesEvent>()
                .Then().Do<MoveArmiesStateMutator>();
}
```

### Card Games (Blackjack, Poker)

**Characteristics:**
- Deck artifacts with shuffle
- Hidden state (player-specific visibility)
- Pile management (deck, hand, discard)
- Scoring based on card combinations

**Builder Pattern:**
```csharp
protected override void Build()
{
    BoardId = "blackjack";
    
    // Players
    AddPlayer("dealer");
    AddPlayer("player-1");
    
    // Minimal board (cards module requires tiles/directions)
    AddDirection("none");
    AddTile("table");
    
    // Deck artifact (from Cards module)
    var deck = AddDeck("main-deck");
    
    // Initialize with standard 52 cards
    var cards = CreateStandardDeck();
    WithDeck("main-deck").HasCards(cards);
    
    // Phases
    AddGamePhase("deal")
        .Then()
            .ForEvent<DealCardEvent>()
                .Then().Do<DealCardStateMutator>();
    
    AddGamePhase("player-turn")
        .Then()
            .ForEvent<HitEvent>()
                .Then().Do<DrawCardStateMutator>()
            .ForEvent<StandEvent>()
                .Then().Do<NextPlayerStateMutator>();
    
    AddGamePhase("dealer-turn")
        .If<PlayerStoodCondition>()
        .Then()
            .ForEvent<DealerHitEvent>()
                .Then().Do<DealerDrawStateMutator>();
    
    AddGamePhase("scoring")
        .Then()
            .ForEvent<ScoreHandsEvent>()
                .Then().Do<ScoreHandsStateMutator>();
}
```

### Abstract Strategy (Chess, Checkers)

**Characteristics:**
- Grid topology with directional relations
- Diverse movement patterns (sliding, leaping, conditional)
- Capture mechanics
- Win condition: capture/immobilize opponent king/pieces

See existing Chess module for comprehensive example.

## Common Pitfalls

### ❌ Mutable State in Mutators

**Wrong:**
```csharp
public GameState MutateState(GameEngine engine, GameState gameState, MyEvent @event)
{
    gameState.SomeProperty = newValue;  // WRONG - mutating input!
    return gameState;
}
```

**Right:**
```csharp
public GameState MutateState(GameEngine engine, GameState gameState, MyEvent @event)
{
    var newState = new MyState(newValue);
    return gameState.Next([newState]);  // Return new snapshot
}
```

### ❌ Hidden Randomness

**Wrong:**
```csharp
var random = new Random();
var roll = random.Next(1, 7);  // WRONG - non-deterministic!
```

**Right:**
```csharp
// Use dice artifacts
var dice = game.GetArtifact<Dice<int>>("d6");
progress = progress.HandleEvent(new RollDiceGameEvent<int>(dice, roll));
```

### ❌ LINQ in Hot Paths

**Wrong:**
```csharp
// In a mutator called frequently
var opponents = gameState.GetStates<PieceState>()
    .Where(ps => ps.Artifact.Owner != activePlayer)
    .Select(ps => ps.Artifact)
    .ToList();  // WRONG - allocations in hot path
```

**Right:**
```csharp
// Use explicit loop
var opponents = new List<Piece>(capacity: 16);
foreach (var ps in gameState.GetStates<PieceState>())
{
    if (ps.Artifact.Owner != activePlayer)
    {
        opponents.Add(ps.Artifact);
    }
}
```

### ❌ Missing Phase Gating

**Wrong:**
```csharp
// No phase condition - rules always active
AddGamePhase("play")
    .Then()
        .ForEvent<MoveEvent>()
            .Then().Do<MoveMutator>();
```

**Right:**
```csharp
// Gate by game state
AddGamePhase("play")
    .If<GameNotEndedCondition>()  // Only active if game not over
    .Then()
        .ForEvent<MoveEvent>()
            .Then().Do<MoveMutator>();
```

### ❌ Implicit Type Conversions

**Wrong:**
```csharp
int value = someObject;  // WRONG - implicit conversion can hide bugs
```

**Right:**
```csharp
int value = (int)someObject;  // Explicit cast shows intent
```

### ❌ Skipping Tests

Every rule branch needs test coverage:
- Valid path ✅
- Invalid path ✅
- Ignore/NotApplicable path ✅
- Edge cases ✅

### ❌ Not Following .editorconfig

Run formatter before committing:
```bash
dotnet format
```

## Example: Implementing Ludo

Complete step-by-step walkthrough coming in a future update.

See existing modules for reference:
- **Chess**: Complex movement patterns, special moves, endgame detection
- **Go**: Custom placement event, group detection, territory scoring
- **Backgammon**: Dice-driven movement, bearing-off mechanics
- **DeckBuilding**: Multi-phase turns, deck/pile management

---

## Next Steps

1. Review [core-concepts.md](core-concepts.md) for fundamentals
2. Study an existing module similar to your target game
3. Create your project structure
4. Start with builder and basic tests
5. Iteratively add events, mutators, and conditions
6. Test frequently
7. Add integration tests for full game playability

## References

- [Core Concepts](core-concepts.md)
- [Phase-Based Design](phase-based-design.md)
- [Extensibility Guide](extensibility.md)
- [Game Termination](game-termination.md)
- [Turn Sequencing](turn-sequencing.md)
- [Determinism & RNG](determinism-rng-timeline.md)

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) for contribution guidelines.

For questions or suggestions about this guide, open an issue on GitHub.
