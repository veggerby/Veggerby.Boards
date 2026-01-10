# Scoring Framework

## Overview

The **Scoring Framework** provides reusable helpers for implementing scoring, victory conditions, and outcome tracking across all game modules. It builds on the foundation of the `IGameOutcome` interface (introduced in [Game Termination](game-termination.md)) to offer:

- **Point-based scoring** with automatic ranking
- **Victory condition patterns** (highest score, first-to-N, elimination)
- **Fluent outcome builders** for standardized results

This framework eliminates duplication by providing common patterns that game modules can compose instead of reimplementing.

---

## Core Components

### 1. ScoreAccumulator

Reusable scoring accumulator for point-based games.

**Location:** `Veggerby.Boards.Utilities.Scoring.ScoreAccumulator`

**Purpose:** Track and aggregate player scores immutably with automatic ranking.

**Key Features:**
- Immutable operations (each method returns a new instance)
- Automatic tie handling in rankings
- Deterministic player ID-based ordering for consistency

**Example:**

```csharp
var accumulator = new ScoreAccumulator();

// Add points for players
accumulator = accumulator
    .Add(player1, 10)
    .Add(player2, 25)
    .Add(player1, 5); // player1 now has 15

// Get current score
var score = accumulator.GetScore(player1); // 15

// Get ranked scores (highest to lowest)
var ranked = accumulator.GetRankedScores();
// ranked[0]: player2, Score=25, Rank=1
// ranked[1]: player1, Score=15, Rank=2
```

**Methods:**

| Method | Description |
|--------|-------------|
| `Add(player, points)` | Adds points to a player's score (can be negative) |
| `Set(player, points)` | Sets a player's score to a specific value |
| `GetScore(player)` | Returns the current score for a player (0 if not found) |
| `GetRankedScores()` | Returns all scores ranked from highest to lowest |

**Tie Handling:**

When multiple players have the same score, they receive the same rank. The next rank skips numbers:

```csharp
// player1: 100 → Rank 1
// player2: 100 → Rank 1
// player3: 50  → Rank 3 (not 2)
```

---

### 2. PlayerScore

Record type representing a player's score with ranking information.

**Location:** `Veggerby.Boards.States.PlayerScore`

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Player` | `Player` | The player this score applies to |
| `Score` | `int` | The numeric score value |
| `Rank` | `int` | The rank position (1 = highest) |

---

### 3. Victory Conditions

Static factory helpers for creating common victory condition patterns.

**Location:** `Veggerby.Boards.Utilities.Scoring.VictoryConditions`

**Returns:** `GameStateConditionFactory` (compatible with GameBuilder fluent API)

#### VictoryConditions.HighestScore

Victory condition where the highest score wins when any player reaches the target.

```csharp
public static GameStateConditionFactory HighestScore(
    Func<GameState, Player, int> scoreGetter,
    int targetScore)
```

**Usage:**

```csharp
builder.WithPhase("EndGame")
    .When(VictoryConditions.HighestScore(
        (state, player) => GetVictoryPoints(state, player),
        targetScore: 50
    ))
    .Then()
    .Do<EndGameStateMutator>();
```

**Behavior:**
- Returns `Ignore` until at least one player reaches the target
- Returns `Valid` when any player meets or exceeds the target
- The player with the highest score is considered the winner

#### VictoryConditions.FirstToScore

Victory condition where the first player to reach the target wins.

```csharp
public static GameStateConditionFactory FirstToScore(
    Func<GameState, Player, int> scoreGetter,
    int targetScore)
```

**Usage:**

```csharp
builder.WithPhase("RaceEnd")
    .When(VictoryConditions.FirstToScore(
        (state, player) => GetPosition(state, player),
        targetScore: 100 // finish line
    ))
    .Then()
    .Do<DeclareWinnerStateMutator>();
```

**Behavior:**
- Returns `Valid` immediately when the first player reaches the target
- Does not wait for other players to catch up

#### VictoryConditions.LastPlayerStanding

Victory condition for elimination-based games.

```csharp
public static GameStateConditionFactory LastPlayerStanding()
```

**Usage:**

```csharp
builder.WithPhase("CheckElimination")
    .When(VictoryConditions.LastPlayerStanding())
    .Then()
    .Do<EndGameStateMutator>();
```

**Behavior:**
- Checks for `PlayerEliminatedState` markers
- Returns `Valid` when only one player remains active
- Returns `Ignore("No players remaining")` if all players are eliminated
- Returns `Ignore("Multiple players remaining")` if 2+ players remain

**Requires:** Players must be marked as eliminated using `PlayerEliminatedState`.

---

### 4. PlayerEliminatedState

Marker state indicating a player has been eliminated from the game.

**Location:** `Veggerby.Boards.States.PlayerEliminatedState`

**Usage:**

```csharp
// Mark player as eliminated
var state = state.Next(new IArtifactState[]
{
    new PlayerEliminatedState(player, "Bankruptcy")
});
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Player` | `Player` | The eliminated player |
| `Reason` | `string` | The reason for elimination (e.g., "Defeated", "Bankruptcy") |

**Common Reasons:**
- `"Defeated"` - Lost in combat
- `"Bankruptcy"` - Ran out of resources
- `"Eliminated"` - Generic elimination
- Custom reasons specific to your game

---

### 5. OutcomeBuilder

Fluent builder for constructing game outcomes.

**Location:** `Veggerby.Boards.Utilities.Scoring.OutcomeBuilder`

**Purpose:** Simplify creation of `IGameOutcome` instances with ranked players and terminal conditions.

**Example:**

```csharp
var outcome = new OutcomeBuilder()
    .WithTerminalCondition("Scoring")
    .WithWinner(player1, new Dictionary<string, object>
    {
        ["VictoryPoints"] = 100,
        ["Armies"] = 42
    })
    .WithLoser(player2, new Dictionary<string, object>
    {
        ["VictoryPoints"] = 50
    })
    .Build();
```

**Methods:**

| Method | Description |
|--------|-------------|
| `WithWinner(player, metrics?)` | Adds a winning player at rank 1 |
| `WithLoser(player, metrics?)` | Adds a losing player with auto-incremented rank |
| `WithRankedPlayers(scores)` | Adds players from `PlayerScore` results |
| `WithTiedPlayers(players, metrics?)` | Adds tied players (all rank 1 with Draw outcome) |
| `WithTerminalCondition(condition)` | Sets the terminal condition (default: "GameEnded") |
| `Build()` | Produces the final `IGameOutcome` instance |

**Automatic Rank Assignment:**

```csharp
builder.WithWinner(player1);  // Rank 1
builder.WithLoser(player2);   // Rank 2
builder.WithLoser(player3);   // Rank 3
```

**From ScoreAccumulator:**

```csharp
var accumulator = new ScoreAccumulator()
    .Add(player1, 100)
    .Add(player2, 75)
    .Add(player3, 50);

var outcome = new OutcomeBuilder()
    .WithRankedPlayers(accumulator.GetRankedScores())
    .WithTerminalCondition("Scoring")
    .Build();

// Result:
// player1: Rank 1, Win, Metrics["Score"] = 100
// player2: Rank 2, Loss, Metrics["Score"] = 75
// player3: Rank 3, Loss, Metrics["Score"] = 50
```

---

### 6. StandardGameOutcome

General-purpose implementation of `IGameOutcome`.

**Location:** `Veggerby.Boards.States.StandardGameOutcome`

**Usage:**

Typically created via `OutcomeBuilder.Build()`, but can be constructed directly:

```csharp
var outcome = new StandardGameOutcome
{
    TerminalCondition = "Checkmate",
    PlayerResults = new[]
    {
        new PlayerResult
        {
            Player = winner,
            Rank = 1,
            Outcome = OutcomeType.Win
        },
        new PlayerResult
        {
            Player = loser,
            Rank = 2,
            Outcome = OutcomeType.Loss
        }
    }
};
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `TerminalCondition` | `string` | The terminal condition (e.g., "Checkmate", "Scoring") |
| `PlayerResults` | `IReadOnlyList<PlayerResult>` | Ordered player results |

---

## Common Patterns

### Pattern 1: Point-Based Scoring (Deck Building)

**Scenario:** Players accumulate victory points. Highest score wins when the game ends.

```csharp
// In EndGameStateMutator
public GameState MutateState(GameEngine engine, GameState state, EndGameEvent @event)
{
    var accumulator = new ScoreAccumulator();
    
    foreach (var player in state.Game.Artifacts.OfType<Player>())
    {
        var vp = CalculateVictoryPoints(state, player);
        accumulator = accumulator.Add(player, vp);
    }
    
    var outcome = new OutcomeBuilder()
        .WithRankedPlayers(accumulator.GetRankedScores())
        .WithTerminalCondition("Scoring")
        .Build();
    
    return state.Next(new IArtifactState[]
    {
        new GameEndedState(),
        new StandardGameOutcome
        {
            TerminalCondition = outcome.TerminalCondition,
            PlayerResults = outcome.PlayerResults
        }
    });
}
```

**Victory Condition:**

```csharp
builder.WithPhase("EndGame")
    .When(VictoryConditions.HighestScore(
        (state, player) => CalculateVictoryPoints(state, player),
        targetScore: 50
    ))
    .Then()
    .Do<EndGameStateMutator>();
```

---

### Pattern 2: Elimination Victory (Risk)

**Scenario:** Players are eliminated when defeated. Last player standing wins.

```csharp
// In EliminatePlayerStateMutator
public GameState MutateState(GameEngine engine, GameState state, PlayerDefeatedEvent @event)
{
    return state.Next(new IArtifactState[]
    {
        new PlayerEliminatedState(@event.Player, "Defeated")
    });
}
```

**Victory Condition:**

```csharp
builder.WithPhase("CheckElimination")
    .When(VictoryConditions.LastPlayerStanding())
    .Then()
    .Do<EndGameStateMutator>();
```

**End Game Mutator:**

```csharp
public GameState MutateState(GameEngine engine, GameState state, EndGameEvent @event)
{
    var eliminatedPlayers = state.GetStates<PlayerEliminatedState>()
        .Select(s => s.Player)
        .ToHashSet();
    
    var winner = state.Game.Artifacts.OfType<Player>()
        .First(p => !eliminatedPlayers.Contains(p));
    
    var outcome = new OutcomeBuilder()
        .WithWinner(winner)
        .WithTerminalCondition("Elimination")
        .Build();
    
    return state.Next(new IArtifactState[]
    {
        new GameEndedState(),
        new StandardGameOutcome
        {
            TerminalCondition = outcome.TerminalCondition,
            PlayerResults = outcome.PlayerResults
        }
    });
}
```

---

### Pattern 3: First-to-N Victory (Racing Game)

**Scenario:** First player to reach the finish line wins.

```csharp
builder.WithPhase("CheckFinish")
    .When(VictoryConditions.FirstToScore(
        (state, player) => GetPosition(state, player),
        targetScore: 100 // finish line
    ))
    .Then()
    .Do<DeclareWinnerStateMutator>();
```

**Mutator:**

```csharp
public GameState MutateState(GameEngine engine, GameState state, DeclareWinnerEvent @event)
{
    var positions = state.Game.Artifacts.OfType<Player>()
        .Select(p => new { Player = p, Position = GetPosition(state, p) })
        .OrderByDescending(x => x.Position)
        .ToList();
    
    var winner = positions.First().Player;
    
    var outcome = new OutcomeBuilder()
        .WithWinner(winner, new Dictionary<string, object>
        {
            ["Position"] = positions.First().Position
        });
    
    foreach (var loser in positions.Skip(1))
    {
        outcome.WithLoser(loser.Player, new Dictionary<string, object>
        {
            ["Position"] = loser.Position
        });
    }
    
    outcome.WithTerminalCondition("FinishLine");
    
    return state.Next(new IArtifactState[]
    {
        new GameEndedState(),
        new StandardGameOutcome
        {
            TerminalCondition = outcome.Build().TerminalCondition,
            PlayerResults = outcome.Build().PlayerResults
        }
    });
}
```

---

## Integration with IGameOutcome

The scoring framework is designed to integrate seamlessly with the `IGameOutcome` interface introduced in [Game Termination](game-termination.md).

**Key Integration Points:**

1. **StandardGameOutcome implements IGameOutcome**
   - Can be used as a drop-in replacement for custom outcome states
   - Provides standard format for cross-module consistency

2. **OutcomeBuilder produces IGameOutcome**
   - Fluent API simplifies construction
   - Handles common patterns (winner/loser, ties, rankings)

3. **Compatible with GameProgress.GetOutcome()**
   - Scoring framework outcomes work with the standard outcome API
   - No special integration code needed

**Example:**

```csharp
// Create outcome using scoring framework
var outcome = new OutcomeBuilder()
    .WithWinner(player1)
    .WithLoser(player2)
    .Build();

// Add to state
state = state.Next(new IArtifactState[]
{
    new GameEndedState(),
    (StandardGameOutcome)outcome
});

// Retrieve via standard API
var retrievedOutcome = progress.GetOutcome();
// retrievedOutcome.TerminalCondition == "GameEnded"
// retrievedOutcome.PlayerResults[0].Outcome == OutcomeType.Win
```

---

## Custom Outcome Extensions

While the scoring framework provides standard outcomes, game modules can extend it with module-specific implementations.

### Option 1: Module-Specific Outcome State

Keep module-specific state while using scoring helpers:

```csharp
public sealed class ChessOutcomeState : IArtifactState, IGameOutcome
{
    public EndgameStatus Status { get; }
    public Player? Winner { get; }
    
    // IGameOutcome implementation uses OutcomeBuilder internally
    public string TerminalCondition => Status.ToString();
    
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var builder = new OutcomeBuilder();
            
            if (Status == EndgameStatus.Stalemate)
            {
                return builder.WithTiedPlayers(new[] { Winner, Loser }).Build().PlayerResults;
            }
            
            return builder
                .WithWinner(Winner!)
                .WithLoser(Loser!)
                .Build()
                .PlayerResults;
        }
    }
}
```

### Option 2: StandardGameOutcome with Custom Metrics

Use `StandardGameOutcome` directly with game-specific metrics:

```csharp
var outcome = new OutcomeBuilder()
    .WithWinner(player1, new Dictionary<string, object>
    {
        ["Territory"] = 85,
        ["CapturedStones"] = 12,
        ["Komi"] = 6.5
    })
    .WithLoser(player2, new Dictionary<string, object>
    {
        ["Territory"] = 79,
        ["CapturedStones"] = 8
    })
    .WithTerminalCondition("TerritoryScoring")
    .Build();
```

---

## Best Practices

### 1. Prefer Immutability

All scoring framework components are immutable:

```csharp
// ✅ Correct
var accumulator = new ScoreAccumulator();
accumulator = accumulator.Add(player, 10);

// ❌ Wrong (won't update accumulator)
var accumulator = new ScoreAccumulator();
accumulator.Add(player, 10); // returns new instance, discarded
```

### 2. Use Factory Pattern for Conditions

Victory conditions use `GameStateConditionFactory` for proper player resolution:

```csharp
// ✅ Correct (factory resolves players from Game)
builder.When(VictoryConditions.HighestScore(scoreGetter, 50))

// ❌ Wrong (players not available at builder time)
builder.When(new HighestScoreCondition(players, scoreGetter, 50))
```

### 3. Deterministic Score Calculation

Score getters must be deterministic:

```csharp
// ✅ Correct (deterministic)
int GetVictoryPoints(GameState state, Player player)
{
    return state.GetStates<ScoreState>()
        .Where(s => s.Player == player)
        .Sum(s => s.Points);
}

// ❌ Wrong (non-deterministic)
int GetVictoryPoints(GameState state, Player player)
{
    return Random.Next(100); // breaks determinism
}
```

### 4. Clear Terminal Conditions

Use descriptive terminal condition names:

```csharp
// ✅ Correct
.WithTerminalCondition("TerritoryScoring")
.WithTerminalCondition("Checkmate")
.WithTerminalCondition("Elimination")

// ❌ Unclear
.WithTerminalCondition("End")
.WithTerminalCondition("Victory")
```

### 5. Include Relevant Metrics

Add metrics that provide insight into the outcome:

```csharp
// ✅ Helpful metrics
.WithWinner(player, new Dictionary<string, object>
{
    ["VictoryPoints"] = 100,
    ["RoundsPlayed"] = 12,
    ["BonusPoints"] = 25
})

// ⚠️ Too sparse (no context)
.WithWinner(player)
```

---

## Performance Considerations

### ScoreAccumulator

- **O(n log n)** for `GetRankedScores()` (sorting)
- **O(1)** for `GetScore()`
- **O(n)** space complexity

**Optimization Tip:** Cache ranked scores if called multiple times:

```csharp
// ✅ Cache when ranking doesn't change
var ranked = accumulator.GetRankedScores();
foreach (var score in ranked)
{
    ProcessScore(score);
}

// ❌ Avoid repeated sorting
foreach (var player in players)
{
    var rank = accumulator.GetRankedScores()
        .First(s => s.Player == player).Rank; // O(n log n) per player!
}
```

### Victory Conditions

- **O(n)** for player iteration in score conditions
- **O(n)** for elimination checking

**Minimal Overhead:** Victory conditions add negligible overhead to phase evaluation (&lt; 1% of game logic).

---

## Testing

All scoring framework components include comprehensive unit tests:

**Test Coverage:**
- `ScoreAccumulatorTests` - 8 tests covering accumulation, ranking, ties, immutability
- `HighestScoreConditionTests` - 5 tests covering valid/ignore cases, edge cases
- `FirstToScoreConditionTests` - 5 tests covering first-to-win logic
- `LastPlayerStandingConditionTests` - 5 tests covering elimination scenarios
- `OutcomeBuilderTests` - 10 tests covering all builder methods

**Run Tests:**

```bash
dotnet test --filter "FullyQualifiedName~Scoring"
```

---

## Migration Guide

### From Custom Scoring to ScoreAccumulator

**Before:**

```csharp
var scores = new Dictionary<Player, int>();
foreach (var player in players)
{
    scores[player] = CalculateScore(state, player);
}

var ranked = scores
    .OrderByDescending(kv => kv.Value)
    .Select((kv, index) => new { Player = kv.Key, Rank = index + 1 })
    .ToList();
```

**After:**

```csharp
var accumulator = new ScoreAccumulator();
foreach (var player in players)
{
    accumulator = accumulator.Add(player, CalculateScore(state, player));
}

var ranked = accumulator.GetRankedScores();
```

### From Custom Outcome to OutcomeBuilder

**Before:**

```csharp
var results = new List<PlayerResult>();
results.Add(new PlayerResult
{
    Player = winner,
    Rank = 1,
    Outcome = OutcomeType.Win,
    Metrics = new Dictionary<string, object> { ["Score"] = 100 }
});
results.Add(new PlayerResult
{
    Player = loser,
    Rank = 2,
    Outcome = OutcomeType.Loss,
    Metrics = new Dictionary<string, object> { ["Score"] = 50 }
});

return new CustomOutcomeState(results);
```

**After:**

```csharp
var outcome = new OutcomeBuilder()
    .WithWinner(winner, new Dictionary<string, object> { ["Score"] = 100 })
    .WithLoser(loser, new Dictionary<string, object> { ["Score"] = 50 })
    .Build();

return new StandardGameOutcome
{
    TerminalCondition = outcome.TerminalCondition,
    PlayerResults = outcome.PlayerResults
};
```

---

## Related Documentation

- [Game Termination](game-termination.md) - `IGameOutcome` interface and outcome tracking
- [Phase-Based Design](phase-based-design.md) - Victory condition integration with phases
- [Core Concepts](core-concepts.md) - Immutability and determinism principles
- [Extensibility](extensibility.md) - Creating custom game modules

---

## Summary

The Scoring Framework provides:

✅ **Reusable Components** - ScoreAccumulator, VictoryConditions, OutcomeBuilder  
✅ **Standard Patterns** - Point-based, elimination, first-to-N  
✅ **Seamless Integration** - Works with `IGameOutcome` from Epic #38  
✅ **Deterministic** - No ambient randomness, reproducible results  
✅ **Tested** - 17+ unit tests ensuring correctness  
✅ **Extensible** - Custom metrics and module-specific outcomes supported

Use this framework to reduce boilerplate and ensure consistency across all game modules.
