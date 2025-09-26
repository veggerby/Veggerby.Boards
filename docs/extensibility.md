# Extensibility Guide

Practical steps for adding games & extending behavior (condensed).

## New Game Checklist

1. Subclass `GameBuilder`.
2. Define players, directions, tiles & relations.
3. Add artifacts (pieces with patterns, dice, custom).
4. Place initial states (pieces on tiles, dice values/null).
5. Define phases & rules (conditions + mutators).
6. Compile & write tests (rule branches: Valid, Invalid, Ignore, NotApplicable).

## Adding Events & Mutators

* Event: implement `IGameEvent` (payload only).
* Rule: derive `GameEventRule<T>` or use simple rule builder.
* Mutator: implement `IStateMutator<T>` returning new `GameState` (or original if no change). Keep logic tight & pure.

## Conditions

Implement `IGameStateCondition` (phase gating) or `IGameEventCondition` (rule validation). Compose via composite helpers.

## Patterns

Prefer existing pattern types; add new `IPattern` only if semantics cannot be expressed through direction/fixed combinations. Provide visitor + (optionally) compiled mapping.

## Exclusivity Groups

Use `.Exclusive("id")` for mutually exclusive phase branches to enable optional masking optimization.

## Custom Artifacts

Subclass `Artifact`, define state representation, and integrate through events + mutators. Keep identity semantics (type + id equality).

## Testing Guidelines

* AAA pattern in tests.
* One test per rule branch.
* No randomness outside dice events.
* Use provided helpers for path resolution rather than duplicating visitor logic.

## Performance Discipline

Hot paths (movement, rule evaluation) must avoid LINQ & allocations; prefer loops and reusing arrays/spans when internal.

## When Not to Extend

Do NOT reach into internal acceleration contexts; request new public surface or contribute internally if needed.
