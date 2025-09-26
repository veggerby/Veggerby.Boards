# Architecture

Layered design separating structural definition from runtime evaluation & optimization.

```txt
Game Modules (Chess, Backgammon, …)
        ↓
      Core Engine
  (Artifacts • State • Rules • Phases • Builders)
        ↓
  Acceleration Layers (feature-flag gated)
```

## Capability Seam

`EngineCapabilities` exposes only: `Topology` (graph neighbor access), `PathResolver` (movement), and opaque internal acceleration context (not for module consumption). Guarantees: enabling or disabling acceleration does not change externally observable state transitions.

## Evaluation Flow

1. External code submits an `IGameEvent`.
2. Active phase resolved (first valid leaf).
3. Pre-processors (0..n) expand event.
4. DecisionPlan evaluates rules (respecting optimization flags) until a mutator applies or all rules exhausted.
5. New immutable `GameState` produced (or original if no rule applied) → wrapped in new `GameProgress`.

## Design Principles

Determinism > raw speed; optimizations must prove parity. Immutability everywhere. Explicit gating via conditions. Small, pure mutators. No LINQ in hot paths.

## Feature Flags & Evolution

All experimental subsystems (compiled patterns, bitboards, sliding fast-path, decision plan masks, turn sequencing, hashing, trace capture, simulation) are individually flag-gated; safe default is conservative.

## Adding Modules

Modules subclass `GameBuilder`; they do not reach into internal namespaces. See `extensibility.md`.
