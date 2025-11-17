# Phase-Based Design

## Overview

Game phases are **first-class hierarchical conditional scopes** that organize game rules into semantically meaningful segments. Phases provide deterministic, declarative structure for complex game flows such as turn segments, endgame detection, and multi-stage gameplay.

## Core Concepts

### What is a GamePhase?

A `GamePhase` is a named, conditional context that:

1. **Activates conditionally** - only when its conditions evaluate to `Valid`
2. **Scopes rules** - events are evaluated only within active phases
3. **Supports hierarchy** - composite phases can contain child phases
4. **Maintains determinism** - phases are evaluated in declaration order
5. **Enables termination** - phases can detect and mark terminal game states

### Phase Hierarchy

Phases can be organized into hierarchies using `CompositeGamePhase`:

```
Game
  └─ Phase: "main-game"
      ├─ Child: "setup"
      ├─ Child: "play"
      │   ├─ Grandchild: "normal-play"
      │   ├─ Grandchild: "check"
      │   └─ Grandchild: "checkmate"
      └─ Child: "scoring"
```

The engine evaluates phases depth-first and selects the **first valid leaf phase** for each event.

## Canonical Patterns

### Pattern 1: Single Active Phase

**Use When:** Game has one continuous gameplay mode

**Example:** Simple movement-only game

```csharp
AddGamePhase("move-pieces")
    .If<NullGameStateCondition>() // Always active
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<PieceIsActivePlayerGameEventCondition>()
        .Then()
            .Do<MovePieceStateMutator>();
```

### Pattern 2: Sequential Phases

**Use When:** Game has distinct sequential stages

**Example:** Setup → Play → Scoring

```csharp
AddGamePhase("setup")
    .If<GameNotStartedCondition>()
    .Then()
        .ForEvent<PlacePieceEvent>()
        .Then().Do<PlacePieceMutator>();

AddGamePhase("play")
    .If<GameStartedCondition>()
        .And<GameNotEndedCondition>()
    .Then()
        .ForEvent<MovePieceGameEvent>()
        .Then().Do<MovePieceStateMutator>();

AddGamePhase("scoring")
    .If<GameEndedCondition>()
    .Then()
        .ForEvent<ScoreTerritoryEvent>()
        .Then().Do<ComputeScoresMutator>();
```

### Pattern 3: Nested Turn Segments

**Use When:** Turns have multiple steps (common in board games)

**Example:** Risk-style turn structure

```csharp
AddGamePhase("reinforcement")
    .If(game => new TurnSegmentCondition(TurnSegment.Start))
    .Then()
        .ForEvent<PlaceArmyEvent>()
        .Then().Do<PlaceArmyMutator>()
              .Do<TransitionToAttackPhase>();

AddGamePhase("attack")
    .If(game => new TurnSegmentCondition(TurnSegment.Main))
    .Then()
        .ForEvent<AttackEvent>()
        .Then().Do<AttackMutator>();

AddGamePhase("fortification")
    .If(game => new TurnSegmentCondition(TurnSegment.End))
    .Then()
        .ForEvent<FortifyEvent>()
        .Then().Do<FortifyMutator>()
              .Do(game => new NextPlayerStateMutator(...));
```

### Pattern 4: Automatic Endgame Detection

**Use When:** Game has terminal conditions (checkmate, stalemate, victory)

**Example:** Chess checkmate/stalemate

```csharp
AddGamePhase("move pieces")
    .WithEndGameDetection(
        game => new CheckmateOrStalemateCondition(game),
        game => new ChessEndGameMutator(game))
    .If<GameNotEndedCondition>()
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<PieceIsActivePlayerGameEventCondition>()
        .Then()
            .Do<MovePieceStateMutator>()
            .Do(game => new NextPlayerStateMutator(...));
```

**How it works:**

1. After each event in the phase, the endgame condition is evaluated
2. If condition returns `Valid`, the endgame mutator is invoked
3. Mutator adds `GameEndedState` and game-specific outcome state
4. Subsequent moves are blocked by `GameNotEndedCondition`

### Pattern 5: Conditional Phase Switching

**Use When:** Game rules change based on game state

**Example:** Backgammon bar clearing

```csharp
AddGamePhase("bar-clearing-required")
    .If<HasPiecesOnBarCondition>()
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<SourceIsBarCondition>()
            .And<DestinationIsLegalEntryCondition>()
        .Then()
            .Do<MovePieceFromBarMutator>();

AddGamePhase("normal-play")
    .If(game => new NegateCondition(new HasPiecesOnBarCondition()))
    .Then()
        .ForEvent<MovePieceGameEvent>()
            // ... normal movement rules
```

### Pattern 6: Exclusive Phases

**Use When:** Multiple phases are mutually exclusive

**Example:** Dice-rolled vs dice-needed states

```csharp
AddGamePhase("dice-rolled")
    .Exclusive("dice-state")
    .If<HasDiceValueCondition>()
    .Then()
        .ForEvent<MovePieceGameEvent>()
        .Then().Do<ConsumeDiceValueMutator>();

AddGamePhase("dice-needed")
    .Exclusive("dice-state")
    .If<NoDiceValueCondition>()
    .Then()
        .ForEvent<RollDiceGameEvent<int>>()
        .Then().Do<DiceStateMutator<int>>();
```

## Phase Evaluation Flow

```
1. Event arrives (e.g., MovePieceGameEvent)
2. DecisionPlan iterates compiled phase hierarchy in order
3. For each phase:
   a. Evaluate phase condition
   b. If condition returns Valid:
      - Evaluate rules within that phase
      - If rule matches:
        * Execute mutators
        * If phase has endgame detection:
          - Evaluate endgame condition
          - If Valid: execute endgame mutator
        * Return result
   c. If condition returns Ignore/NotApplicable:
      - Skip phase, continue to next
4. If no phase/rule matched: return Ignore
```

## Best Practices

### ✅ DO

- **Use phases to model semantic game stages** (setup, play, endgame)
- **Name phases descriptively** (`"reinforcement"`, `"checkmate"`, `"bar-clearing-required"`)
- **Order phases from specific to general** (most restrictive conditions first)
- **Use `.WithEndGameDetection()` for terminal conditions** instead of external detectors
- **Document phase hierarchy** in module README or inline comments
- **Test phase transitions explicitly** (verify condition logic)

### ❌ DON'T

- **Avoid overlapping phase conditions** without clear precedence (use `.Exclusive()` when appropriate)
- **Don't embed game-specific phase logic in core** (phases are module-level constructs)
- **Don't use phases for micro-optimizations** (phases are about semantics, not performance shortcuts)
- **Don't skip phase conditions** (use `NullGameStateCondition` if always active, for clarity)
- **Don't create deeply nested hierarchies without justification** (flat is often clearer than deep)

## Testing Phases

### Unit Tests

Test individual phase conditions in isolation:

```csharp
[Fact]
public void GivenPieceOnBar_WhenEvaluatingBarClearingCondition_ThenValid()
{
    // arrange
    var condition = new HasPiecesOnBarCondition();
    var state = CreateStateWithPieceOnBar();

    // act
    var result = condition.Evaluate(state);

    // assert
    result.ShouldBeValid();
}
```

### Integration Tests

Test phase transitions across game flow:

```csharp
[Fact]
public void GivenGameInProgress_WhenCheckmateDetected_ThenTransitionsToEndgamePhase()
{
    // arrange
    var progress = SetupCheckmatePosition();

    // act
    progress = progress.Move(...); // Trigger checkmate

    // assert
    progress.IsGameOver().ShouldBeTrue();
    var outcome = progress.GetOutcome();
    outcome.TerminalCondition.ShouldBe("Checkmate");
}
```

## Phase Hierarchy Design Guidance

### When to Use CompositeGamePhase

Use composite phases when:

1. **Logical grouping** - Related rules belong together semantically
2. **Conditional activation** - An entire subtree activates/deactivates as a unit
3. **Code organization** - Builder readability improves with nesting

Example: Chess could theoretically nest phases by game status:

```
"chess-game"
  ├─ "in-progress"
  │   ├─ "normal-play"
  │   └─ "check"
  └─ "terminal"
      ├─ "checkmate"
      └─ "stalemate"
```

However, **flat is often better** unless the nesting provides clear semantic value.

### When to Keep Phases Flat

Keep phases flat when:

1. **Phases are mutually exclusive** - Use `.Exclusive()` instead of nesting
2. **Nesting adds no semantic value** - Don't nest just for organization
3. **Simpler mental model suffices** - Sequential phases (setup → play → scoring)

## Module-Specific Examples

### Chess

**Current Implementation:**

```csharp
AddGamePhase("move pieces")
    .WithEndGameDetection(
        game => new CheckmateOrStalemateCondition(game),
        game => new ChessEndGameMutator(game))
    .If<GameNotEndedCondition>()
    .Then()
        // Castling, captures, normal moves, etc.
```

**Future Enhancement (Optional):**

Could explicitly model check vs non-check phases:

```csharp
AddGamePhase("normal-play")
    .If<GameNotEndedCondition>()
        .And<NotInCheckCondition>()
    .Then()
        // All legal moves

AddGamePhase("check")
    .WithEndGameDetection(...)
    .If<InCheckCondition>()
    .Then()
        // Only moves that resolve check
```

### Backgammon

**Current Implementation:**

Three phases: initial roll, dice rolled, dice needed

**Recommended Enhancement:**

Add explicit bar-clearing phase:

```csharp
AddGamePhase("bar-clearing-required")
    .If<HasPiecesOnBarCondition>()
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<SourceIsBarCondition>()
        .Then().Do<MoveFromBarMutator>();

AddGamePhase("normal-play")
    .If(game => new NegateCondition(new HasPiecesOnBarCondition()))
    .Then()
        // Existing dice-rolled / dice-needed phases as children
```

### Go

**Current Implementation:**

Single play phase with pass detection in mutator

**Recommended Enhancement:**

Add explicit scoring phase:

```csharp
AddGamePhase("play")
    .If<GameNotEndedCondition>()
    .Then()
        .ForEvent<PlaceStoneEvent>()
        .Then().Do<PlaceStoneMutator>();

AddGamePhase("territory-scoring")
    .If<ConsecutivePassesCondition>()
    .Then()
        .ForEvent<ScoreTerritoryEvent>()
        .Then().Do<ComputeTerritoryScoresMutator>()
              .Do<AddGoOutcomeStateMutator>()
              .Do<MarkGameEndedMutator>();
```

## Migration from External Detectors

If you have external detector classes (e.g., `ChessEndgameDetector`), migrate to phase-based detection:

### Before (External Detector)

```csharp
// In sample/demo code
var detector = new ChessEndgameDetector(game);
if (detector.IsGameOver(state))
{
    var status = detector.GetEndgameStatus(state);
    // Handle endgame
}
```

### After (Phase-Based + Unified API)

```csharp
// In GameBuilder
AddGamePhase("move pieces")
    .WithEndGameDetection(
        game => new CheckmateOrStalemateCondition(game),
        game => new ChessEndGameMutator(game))
    // ...

// In sample/demo code
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome();
    Console.WriteLine($"Game ended: {outcome.TerminalCondition}");
    // Handle endgame uniformly
}
```

**Benefits:**

- ✅ Declarative: endgame logic visible in builder
- ✅ Automatic: no manual polling required
- ✅ Unified: same API across all game modules
- ✅ Deterministic: part of event processing, captured in state

## Advanced: Pre-Processors

Phases can register pre-processors that transform events before rule evaluation:

```csharp
AddGamePhase("dice-rolled")
    .If<HasDiceValueCondition>()
    .Then()
        .PreProcessEvent(game => new SingleStepMovePieceGameEventPreProcessor(...))
        .ForEvent<MovePieceGameEvent>()
        .Then().Do<ConsumeDiceValueMutator>();
```

Pre-processors expand a single high-level event (e.g., multi-square move) into granular steps (single-square moves) for validation.

**Use sparingly** - pre-processors add complexity. Prefer simple rules when possible.

## Summary

Game phases provide:

1. **Semantic clarity** - Rules organized by game stage
2. **Declarative structure** - Phase hierarchy visible in builder
3. **Automatic termination** - `.WithEndGameDetection()` replaces external polling
4. **Determinism** - Phase evaluation is ordered and reproducible
5. **Testability** - Conditions and transitions are discrete units

**Prefer phase-first architecture** for new game modules. Apply these patterns consistently to maintain architectural uniformity.
