# Rule Priority & Conflict Resolution

**Status:** ✅ Implemented (v1.0)  
**Priority:** P2 - Important  
**Complexity:** Medium

## Overview

The rule priority and conflict resolution system enables explicit control over which rule applies when multiple rules match the same event. This makes complex game flows more maintainable, supports modular rule composition, and provides clear visibility into decision-making.

## Quick Start

### Basic Usage

```csharp
// Traditional API (implicit declaration order)
AddGamePhase("move pieces")
    .If<GameNotEndedCondition>()
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<CastlingGameEventCondition>()
        .Then()
            .Do<CastlingMoveMutator>();

// With explicit priority
AddGamePhase("move pieces")
    .If<GameNotEndedCondition>()
    .WithPriority(RulePriority.High)              // Explicit priority
    .WithStrategy("special-moves")                 // Optional grouping hint
    .WithConflictResolution(ConflictResolutionStrategy.HighestPriority)  // Resolution strategy
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<CastlingGameEventCondition>()
        .Then()
            .Do<CastlingMoveMutator>();
```

## Priority Levels

```csharp
public enum RulePriority
{
    Lowest = 0,       // Evaluated last
    Low = 25,
    Normal = 50,      // Default (no explicit priority)
    High = 75,        // Special-case rules
    Highest = 100,
    Override = 200    // Variant overrides (use sparingly)
}
```

**Best Practices:**
- Use `Normal` (default) for most rules
- Reserve `High` for special-case rules (e.g., castling, en passant)
- Use `Override` only when intentionally replacing base module behavior
- Same priorities resolve via declaration order (first declared wins)

## Conflict Resolution Strategies

### 1. FirstWins (Default, Backward Compatible)

**Behavior:** First matching rule wins (declaration order)  
**Performance:** Optimized fast path (no overhead)  
**Use When:** Default behavior is sufficient, no conflicts expected

```csharp
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.FirstWins)  // Default
    // ...
```

### 2. HighestPriority

**Behavior:** Highest priority rule wins; ties resolve via declaration order  
**Performance:** Minimal overhead (sorting only when needed)  
**Use When:** Explicit precedence required (e.g., special moves > normal moves)

```csharp
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.HighestPriority)
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<CastlingCondition>()
            .WithPriority(RulePriority.High)  // Evaluated first
        .Then()
            .Do<CastlingMutator>()
        .ForEvent<MovePieceGameEvent>()
            .If<NormalMoveCondition>()
            .WithPriority(RulePriority.Normal)  // Fallback
        .Then()
            .Do<NormalMoveMutator>();
```

### 3. LastWins

**Behavior:** Last matching rule wins (reverse declaration order)  
**Use When:** Override pattern (derived modules override base rules)

```csharp
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.LastWins)
    // Base rule (declared first, overridden)
    .Then()
        .ForEvent<MovePieceGameEvent>()
        .Then()
            .Do<BaseMutator>()
    // Variant rule (declared last, wins)
    .Then()
        .ForEvent<MovePieceGameEvent>()
        .Then()
            .Do<VariantMutator>();
```

### 4. Exclusive (Fail-Fast)

**Behavior:** Throws exception if multiple rules match  
**Use When:** Development/debugging to catch unintended rule overlaps

```csharp
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.Exclusive)
    // Will throw BoardException if both rules match
```

**Exception Example:**
```
BoardException: Exclusive conflict resolution failed: 2 rules matched for event MovePieceGameEvent.
Conflicting rules: rule-a, rule-b
```

### 5. ApplyAll (Composition)

**Behavior:** Apply all matching rules in priority order (highest first)  
**Use When:** Compositional effects (e.g., card game effect stacking)  
**Caution:** Requires careful state management for determinism

```csharp
AddGamePhase("apply-effects")
    .WithConflictResolution(ConflictResolutionStrategy.ApplyAll)
    .Then()
        .ForEvent<CardPlayedEvent>()
            .If<EffectA>()
            .WithPriority(RulePriority.High)  // Applied first
        .Then()
            .Do<EffectAMutator>()
        .ForEvent<CardPlayedEvent>()
            .If<EffectB>()
            .WithPriority(RulePriority.Normal)  // Applied second
        .Then()
            .Do<EffectBMutator>();
```

## Strategy Identifiers

Optional grouping hints for related rules (e.g., "castling", "en-passant"):

```csharp
AddGamePhase("move")
    .WithStrategy("special-moves")  // Semantic grouping
    .WithPriority(RulePriority.High)
    // ...
```

**Purpose:** Documentation and future observability/diagnostics

## Performance

- **FirstWins (default):** Zero overhead (optimized fast path)
- **Other strategies:** Minimal overhead when rules match (< 5% in benchmarks)
- **Compile-time:** Priority metadata stored in `DecisionPlan` (no runtime cost)

## Determinism

All conflict resolution is deterministic:
- Same priorities → declaration order tiebreaker
- Sorting uses stable sort (preserves declaration order for ties)
- No ambient randomness or time-based ordering

## Migration from Declaration Order

### Before (Implicit Ordering)
```csharp
AddGamePhase("move")
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<CastlingCondition>()
        .Then()
            .Do<CastlingMutator>()
        .ForEvent<MovePieceGameEvent>()
            .If<NormalMoveCondition>()
        .Then()
            .Do<NormalMoveMutator>();
```

### After (Explicit Priority)
```csharp
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.HighestPriority)
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<CastlingCondition>()
            .WithPriority(RulePriority.High)  // Explicit precedence
        .Then()
            .Do<CastlingMutator>()
        .ForEvent<MovePieceGameEvent>()
            .If<NormalMoveCondition>()
            .WithPriority(RulePriority.Normal)
        .Then()
            .Do<NormalMoveMutator>();
```

**Benefits:**
- Clear intent (why castling takes precedence)
- Refactoring-safe (order doesn't matter)
- Debugging-friendly (explicit decision trace)

## Common Patterns

### Pattern 1: Special Moves
```csharp
// Chess castling takes precedence over normal king moves
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.HighestPriority)
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<CastlingCondition>()
            .WithPriority(RulePriority.High)
        .Then()
            .Do<CastlingMutator>()
        .ForEvent<MovePieceGameEvent>()
            .If<NormalKingMoveCondition>()
            .WithPriority(RulePriority.Normal)
        .Then()
            .Do<NormalMoveMutator>();
```

### Pattern 2: Variant Overrides
```csharp
// Custom variant overrides base Chess rules
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.HighestPriority)
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<BaseChessRule>()
            .WithPriority(RulePriority.Normal)
        .Then()
            .Do<BaseMutator>()
        .ForEvent<MovePieceGameEvent>()
            .If<VariantRule>()
            .WithPriority(RulePriority.Override)  // Wins over base
        .Then()
            .Do<VariantMutator>();
```

### Pattern 3: Fail-Fast Development
```csharp
// Catch accidental rule overlaps during development
AddGamePhase("move")
    .WithConflictResolution(ConflictResolutionStrategy.Exclusive)
    // Will throw if multiple rules match
```

## Testing

Example test structure:

```csharp
[Fact]
public void GivenHighestPriorityStrategy_WhenMultipleRulesMatch_ThenHighestPriorityRuleApplied()
{
    // arrange
    var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
    GamePhase.New(1, "low", new NullGameStateCondition(), new TestRule("Low"), root, 
        priority: RulePriority.Low, conflictResolution: ConflictResolutionStrategy.HighestPriority);
    GamePhase.New(2, "high", new NullGameStateCondition(), new TestRule("High"), root, 
        priority: RulePriority.High, conflictResolution: ConflictResolutionStrategy.HighestPriority);

    var engine = new GameEngine(game, root, DecisionPlan.Compile(root));
    var progress = new GameProgress(engine, GameState.New([]), EventChain.Empty);

    // act
    var result = progress.HandleEvent(new TestEvent());

    // assert
    result.State.GetExtras<string>().Should().Be("High");  // High priority wins
}
```

## Future Enhancements

### Observability (Planned)
```csharp
// Future: Diagnostic evaluation with conflict resolution details
var decision = engine.EvaluateWithDiagnostics(event, state);
Console.WriteLine($"Selected: {decision.SelectedRule.RuleName} (priority: {decision.SelectedRule.Priority})");
Console.WriteLine($"Rejected: {string.Join(", ", decision.RejectedRules.Select(r => r.RuleName))}");
```

### Observer Integration (Planned)
```csharp
// Future: Observer notifications for conflict resolution
observer.OnConflictResolved(selectedRule, rejectedRules, strategy);
```

## Related Documentation

- [Decision Plan & Acceleration](/docs/decision-plan-and-acceleration.md) - DecisionPlan internals
- [Phase-Based Design](/docs/phase-based-design.md) - Phase system overview
- [Core Concepts](/docs/core-concepts.md) - Immutability, events, mutators

## API Reference

### GamePhaseDefinition Extensions
```csharp
IGamePhaseDefinition WithPriority(RulePriority priority)
IGamePhaseDefinition WithStrategy(string identifier)
IGamePhaseDefinition WithConflictResolution(ConflictResolutionStrategy strategy)
```

### Enums
```csharp
enum RulePriority { Lowest, Low, Normal, High, Highest, Override }
enum ConflictResolutionStrategy { FirstWins, HighestPriority, LastWins, Exclusive, ApplyAll }
```

### Records (Observability - Future)
```csharp
record RuleMetadata(string RuleName, RulePriority Priority, string? StrategyIdentifier)
record RuleDecision(IGameEvent Event, ConditionResponse Response, RuleMetadata? SelectedRule, 
    IReadOnlyList<RuleMetadata>? RejectedRules, string? SelectionReason)
```

## Changelog

### v1.0 (Current)
- ✅ Core infrastructure (enums, records, metadata)
- ✅ DecisionPlan integration (priority-aware compilation)
- ✅ Conflict resolution strategies (all 5 implemented)
- ✅ GameBuilder fluent API extensions
- ✅ Comprehensive test coverage
- ✅ Full backward compatibility (default FirstWins)

### Future
- Diagnostic evaluation API (`EvaluateWithDiagnostics`)
- Observer integration for conflict events
- Visual debugging tools
- Module integration examples (Chess castling)
