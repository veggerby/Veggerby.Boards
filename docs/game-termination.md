# Game Termination and Outcome Tracking

## Overview

Veggerby.Boards provides a **unified, standardized API** for detecting game termination and extracting outcome information across all game modules. This document describes the termination infrastructure, outcome representation patterns, and integration guidelines.

## Core Abstractions

### GameEndedState

`GameEndedState` is the **universal marker** indicating game termination. It resides in the `Veggerby.Boards.States` namespace and is used by all game modules.

```csharp
namespace Veggerby.Boards.States;

/// <summary>
/// Immutable marker state indicating the game has reached a terminal condition.
/// </summary>
public sealed class GameEndedState : IArtifactState
{
    public Artifact Artifact => Marker;
    public bool Equals(IArtifactState other) => other is GameEndedState;
}
```

**Key Properties:**

- Singleton artifact (internal `GameEndedMarker`)
- Equality based on type only (all instances equivalent)
- Added to `GameState` when game reaches terminal condition
- Checked by `GameProgress.IsGameOver()`

### IGameOutcome

`IGameOutcome` is the **standardized outcome interface** providing terminal condition details and player results.

```csharp
namespace Veggerby.Boards.States;

/// <summary>
/// Represents the final outcome of a terminated game.
/// </summary>
public interface IGameOutcome
{
    /// <summary>
    /// Terminal condition (e.g., "Checkmate", "Stalemate", "Scoring", "TerritoryScoring").
    /// </summary>
    string TerminalCondition { get; }

    /// <summary>
    /// Ordered player results from winner to loser (or tied).
    /// </summary>
    IReadOnlyList<PlayerResult> PlayerResults { get; }
}
```

**Game modules implement this interface** on their outcome states to expose results uniformly.

### PlayerResult

`PlayerResult` represents a single player's outcome in a terminated game.

```csharp
public sealed record PlayerResult
{
    public required Player Player { get; init; }
    public required OutcomeType Outcome { get; init; }
    public required int Rank { get; init; }
    public IReadOnlyDictionary<string, object>? Metrics { get; init; }
}

public enum OutcomeType
{
    Win,      // Player won the game
    Loss,     // Player lost the game
    Draw,     // Game ended in a draw
    Eliminated // Player was eliminated during play
}
```

**Key Fields:**

- `Player` - The player this result applies to
- `Outcome` - Win/Loss/Draw/Eliminated
- `Rank` - Final placement (1 = winner, 2 = second, etc.)
- `Metrics` - Optional game-specific data (e.g., `{"VictoryPoints": 42, "Territory": 85}`)

## Unified API

### GameProgress.IsGameOver()

Checks if the game has reached a terminal state.

```csharp
if (progress.IsGameOver())
{
    Console.WriteLine("Game has ended!");
}
```

**Implementation:**

```csharp
public static bool IsGameOver(this GameProgress progress)
{
    return progress.State.GetStates<GameEndedState>().Any();
}
```

**Returns:**

- `true` - Game has ended (GameEndedState present)
- `false` - Game still in progress

### GameProgress.GetOutcome()

Extracts game outcome details if available.

```csharp
var outcome = progress.GetOutcome();
if (outcome != null)
{
    Console.WriteLine($"Terminal Condition: {outcome.TerminalCondition}");
    foreach (var result in outcome.PlayerResults.OrderBy(r => r.Rank))
    {
        Console.WriteLine($"  {result.Rank}. {result.Player.Id}: {result.Outcome}");
    }
}
```

**Implementation:**

```csharp
public static IGameOutcome? GetOutcome(this GameProgress progress)
{
    if (!progress.IsGameOver())
    {
        return null;
    }

    // Look for module-specific outcome states implementing IGameOutcome
    return progress.State.ChildStates.OfType<IGameOutcome>().FirstOrDefault();
}
```

**Returns:**

- `IGameOutcome` - If game ended and module implemented outcome tracking
- `null` - If game in progress OR module doesn't track outcomes

## Module Integration Patterns

### Pattern 1: Checkmate/Stalemate (Chess)

**Outcome State:**

```csharp
public sealed class ChessOutcomeState : IArtifactState, IGameOutcome
{
    public EndgameStatus Status { get; }
    public Player? Winner { get; }
    
    public string TerminalCondition => Status.ToString();
    
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            if (Status == EndgameStatus.Stalemate)
            {
                // Draw - both players tied
                return new[]
                {
                    new PlayerResult { Player = player1, Outcome = OutcomeType.Draw, Rank = 1 },
                    new PlayerResult { Player = player2, Outcome = OutcomeType.Draw, Rank = 1 }
                };
            }
            
            // Checkmate - winner and loser
            return new[]
            {
                new PlayerResult { Player = Winner, Outcome = OutcomeType.Win, Rank = 1 },
                new PlayerResult { Player = loser, Outcome = OutcomeType.Loss, Rank = 2 }
            };
        }
    }
}
```

**Endgame Detection:**

```csharp
AddGamePhase("move pieces")
    .WithEndGameDetection(
        game => new CheckmateOrStalemateCondition(game),
        game => new ChessEndGameMutator(game))
    .If<GameNotEndedCondition>()
    .Then()
        // Movement rules...
```

**Mutator:**

```csharp
public sealed class ChessEndGameMutator : IStateMutator<IGameEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        var detector = new ChessEndgameDetector(_game);
        
        if (detector.IsCheckmate(state))
        {
            var winner = GetWinner(state);
            var outcomeState = new ChessOutcomeState(EndgameStatus.Checkmate, winner);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }
        
        if (detector.IsStalemate(state))
        {
            var outcomeState = new ChessOutcomeState(EndgameStatus.Stalemate, null);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }
        
        return state;
    }
}
```

**Usage:**

```csharp
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome();
    Console.WriteLine($"Game ended: {outcome.TerminalCondition}");
    // Output: "Game ended: Checkmate"
}
```

### Pattern 2: Victory Point Scoring (DeckBuilding)

**Outcome State:**

```csharp
public sealed class DeckBuildingOutcomeState : IArtifactState, IGameOutcome
{
    private readonly IReadOnlyList<ScoreState> _scores;
    
    public string TerminalCondition => "Scoring";
    
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var ranked = _scores.OrderByDescending(s => s.VictoryPoints).ToList();
            return ranked.Select((score, index) => new PlayerResult
            {
                Player = score.Artifact,
                Rank = index + 1,
                Outcome = index == 0 ? OutcomeType.Win : OutcomeType.Loss,
                Metrics = new Dictionary<string, object>
                {
                    ["VictoryPoints"] = score.VictoryPoints
                }
            }).ToList();
        }
    }
}
```

**Endgame Mutator:**

```csharp
public sealed class EndGameStateMutator : IStateMutator<EndGameEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, EndGameEvent @event)
    {
        // Check if already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return state;
        }
        
        // Collect scores
        var scores = state.GetStates<ScoreState>().ToList();
        var outcomeState = new DeckBuildingOutcomeState(scores);
        
        return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
    }
}
```

**Usage:**

```csharp
var outcome = progress.GetOutcome();
foreach (var result in outcome.PlayerResults.OrderBy(r => r.Rank))
{
    var vp = result.Metrics?["VictoryPoints"];
    Console.WriteLine($"{result.Rank}. {result.Player.Id}: {vp} VP");
}
// Output:
// 1. player-1: 42 VP
// 2. player-2: 38 VP
```

### Pattern 3: Territory Scoring (Go)

**Outcome State:**

```csharp
public sealed class GoOutcomeState : IArtifactState, IGameOutcome
{
    public IReadOnlyDictionary<Player, int> TerritoryScores { get; }
    public IReadOnlyDictionary<Player, int> Captures { get; }
    public Player? Winner { get; }
    
    public string TerminalCondition => "TerritoryScoring";
    
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var results = TerritoryScores
                .OrderByDescending(kv => kv.Value)
                .Select((kv, index) => new PlayerResult
                {
                    Player = kv.Key,
                    Rank = index + 1,
                    Outcome = kv.Key == Winner ? OutcomeType.Win : OutcomeType.Loss,
                    Metrics = new Dictionary<string, object>
                    {
                        ["Territory"] = TerritoryScores[kv.Key],
                        ["Captures"] = Captures[kv.Key]
                    }
                })
                .ToList();
            
            return results;
        }
    }
}
```

**Territory Scoring Phase:**

```csharp
AddGamePhase("territory-scoring")
    .If(game => new ConsecutivePassesCondition(2))
    .Then()
        .ForEvent<ScoreTerritoryEvent>()
        .Then()
            .Do<ComputeTerritoryScoresMutator>()
            .Do<AddGoOutcomeStateMutator>()
            .Do<MarkGameEndedMutator>();
```

**Usage:**

```csharp
var outcome = progress.GetOutcome();
foreach (var result in outcome.PlayerResults.OrderBy(r => r.Rank))
{
    Console.WriteLine($"{result.Rank}. {result.Player.Id}: " +
                      $"Territory={result.Metrics["Territory"]}, " +
                      $"Captures={result.Metrics["Captures"]}");
}
```

### Pattern 4: Elimination (Multiplayer Games)

**Example:** Risk-style elimination

```csharp
public sealed class RiskOutcomeState : IArtifactState, IGameOutcome
{
    private readonly List<(Player Player, int EliminationTurn, int Rank)> _eliminations;
    private readonly Player? _winner;
    
    public string TerminalCondition => "Conquest";
    
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            return _eliminations
                .OrderBy(e => e.Rank)
                .Select(e => new PlayerResult
                {
                    Player = e.Player,
                    Rank = e.Rank,
                    Outcome = e.Player == _winner ? OutcomeType.Win : OutcomeType.Eliminated,
                    Metrics = new Dictionary<string, object>
                    {
                        ["EliminationTurn"] = e.EliminationTurn
                    }
                })
                .ToList();
        }
    }
}
```

## Implementation Checklist

When adding termination/outcome tracking to a game module:

### 1. Define Outcome State

```csharp
public sealed class [Game]OutcomeState : IArtifactState, IGameOutcome
{
    // Marker artifact
    private static readonly [Game]OutcomeMarker Marker = new();
    public Artifact Artifact => Marker;
    
    // Game-specific terminal data
    public [TerminationType] Type { get; }
    public Player? Winner { get; }
    
    // IGameOutcome implementation
    public string TerminalCondition => Type.ToString();
    public IReadOnlyList<PlayerResult> PlayerResults { get; }
    
    // Equality
    public bool Equals(IArtifactState other) => /* ... */;
}
```

### 2. Create Endgame Condition

```csharp
public sealed class [Game]EndgameCondition : IGameStateCondition
{
    public ConditionResponse Evaluate(GameState state)
    {
        if (/* terminal condition detected */)
        {
            return ConditionResponse.Valid;
        }
        
        return ConditionResponse.Ignore("Not in endgame");
    }
}
```

### 3. Create Endgame Mutator

```csharp
public sealed class [Game]EndGameMutator : IStateMutator<IGameEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        // Determine terminal condition type
        var terminationType = DetermineTerminationType(state);
        var winner = DetermineWinner(state, terminationType);
        
        // Create outcome state
        var outcomeState = new [Game]OutcomeState(terminationType, winner);
        
        // Add both GameEndedState and outcome state
        return state.Next(new IArtifactState[] 
        { 
            new GameEndedState(), 
            outcomeState 
        });
    }
}
```

### 4. Configure Phase Detection

```csharp
AddGamePhase("main-game")
    .WithEndGameDetection(
        game => new [Game]EndgameCondition(game),
        game => new [Game]EndGameMutator(game))
    .If<GameNotEndedCondition>()
    .Then()
        // Game rules...
```

### 5. Create GameNotEndedCondition

```csharp
public sealed class GameNotEndedCondition : IGameStateCondition
{
    public ConditionResponse Evaluate(GameState state)
    {
        if (state.GetStates<GameEndedState>().Any())
        {
            return ConditionResponse.Ignore("Game has ended");
        }
        
        return ConditionResponse.Valid;
    }
}
```

### 6. Update Demos

```csharp
// Replace module-specific termination checks
// OLD:
var detector = new [Game]EndgameDetector(game);
if (detector.IsGameOver(state)) { /* ... */ }

// NEW:
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome();
    Console.WriteLine($"Game ended: {outcome.TerminalCondition}");
    // Display results...
}
```

## Best Practices

### ✅ DO

- **Always add GameEndedState** - This is the universal marker checked by `IsGameOver()`
- **Implement IGameOutcome** on outcome states for unified API access
- **Order PlayerResults consistently** - By rank (1 = winner, 2 = second, etc.)
- **Use Metrics for game-specific data** - Victory points, territory, armies, etc.
- **Prevent moves after termination** - Use `GameNotEndedCondition` in active phases
- **Test termination transitions** - Verify `IsGameOver()` and `GetOutcome()` work correctly

### ❌ DON'T

- **Don't add GameEndedState without outcome state** - Users expect `GetOutcome()` to return data
- **Don't compute outcomes lazily** - Store outcomes in state for deterministic replay
- **Don't use module-specific termination APIs** - Prefer unified `progress.IsGameOver()`
- **Don't forget to document TerminalCondition values** - Consumers need to know possible values
- **Don't mutate outcome states** - They are immutable snapshots

## Module Status

| Module | GameEndedState | Outcome State | Phase Detection | Unified API | Status |
|--------|----------------|---------------|-----------------|-------------|--------|
| **Chess** | ✅ Core | ✅ ChessOutcomeState | ✅ WithEndGameDetection | ✅ Implemented | **Complete** |
| **Go** | ✅ Core | ❌ Missing | ⚠️ Manual (PassTurnStateMutator) | ⚠️ Partial | **Needs GoOutcomeState** |
| **DeckBuilding** | ✅ Core | ⚠️ Has ScoreState | ⚠️ Manual (EndGameStateMutator) | ⚠️ Partial | **Needs outcome wrapper** |
| **Backgammon** | ❌ Missing | ❌ Missing | ❌ Missing | ❌ Missing | **Not implemented** |

## Migration Guide

### Migrating from External Detectors

If your module uses an external detector class (e.g., `ChessEndgameDetector`):

#### Step 1: Keep External Detector (Deprecated)

Mark as obsolete but retain for backward compatibility:

```csharp
[Obsolete("Use phase-based endgame detection with progress.IsGameOver() instead.")]
public sealed class ChessEndgameDetector
{
    // Keep existing implementation
}
```

#### Step 2: Create Outcome State

Implement `IGameOutcome` on a new state class.

#### Step 3: Create Endgame Mutator

Use the existing detector internally:

```csharp
public sealed class ChessEndGameMutator : IStateMutator<IGameEvent>
{
    private readonly ChessEndgameDetector _detector;
    
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        if (_detector.IsCheckmate(state))
        {
            var outcomeState = new ChessOutcomeState(EndgameStatus.Checkmate, ...);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }
        
        // ... other terminal conditions
        return state;
    }
}
```

#### Step 4: Update GameBuilder

Add `.WithEndGameDetection()` to appropriate phase.

#### Step 5: Update Demos

Replace detector usage with unified API.

#### Step 6: Document Migration Path

Update module README with migration examples.

## Testing Termination

### Unit Tests

```csharp
[Fact]
public void GivenCheckmatePosition_WhenEvaluatingEndgameCondition_ThenValid()
{
    // arrange
    var condition = new CheckmateOrStalemateCondition(game);
    var state = CreateCheckmateState();

    // act
    var result = condition.Evaluate(state);

    // assert
    result.ShouldBeValid();
}

[Fact]
public void GivenCheckmate_WhenMutatorApplied_ThenGameEndedStateAdded()
{
    // arrange
    var mutator = new ChessEndGameMutator(game);
    var state = CreateCheckmateState();

    // act
    var newState = mutator.MutateState(engine, state, movePieceEvent);

    // assert
    newState.GetStates<GameEndedState>().ShouldNotBeEmpty();
}
```

### Integration Tests

```csharp
[Fact]
public void GivenGameInProgress_WhenCheckmate_ThenIsGameOverReturnsTrue()
{
    // arrange
    var progress = SetupCheckmatePosition();

    // act
    progress = progress.Move(...); // Trigger checkmate

    // assert
    progress.IsGameOver().ShouldBeTrue();
}

[Fact]
public void GivenGameEnded_WhenGetOutcome_ThenReturnsValidOutcome()
{
    // arrange
    var progress = ExecuteGameToCheckmate();

    // act
    var outcome = progress.GetOutcome();

    // assert
    outcome.ShouldNotBeNull();
    outcome.TerminalCondition.ShouldBe("Checkmate");
    outcome.PlayerResults.Count.ShouldBe(2);
    outcome.PlayerResults[0].Outcome.ShouldBe(OutcomeType.Win);
}
```

## Summary

Game termination and outcome tracking in Veggerby.Boards:

1. **Unified Markers** - `GameEndedState` universally indicates termination
2. **Standardized Outcomes** - `IGameOutcome` provides consistent result format
3. **Phase Integration** - `.WithEndGameDetection()` automates terminal state addition
4. **Universal API** - `progress.IsGameOver()` and `progress.GetOutcome()` work across all modules
5. **Deterministic** - Outcomes stored in immutable state chain

**Always integrate termination tracking when implementing a game module.** This ensures consistency and enables generic tooling (scoreboards, replays, AI agents).
