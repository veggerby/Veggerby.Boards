# Veggerby.Boards Documentation

Unified, task‑oriented documentation for the Veggerby.Boards engine focusing on: clarity, determinism guarantees, extension points, and performance charters.

## Navigation

| Topic | Purpose |
|-------|---------|
| [Quickstart](quickstart.md) | Build & run a minimal game in minutes |
| [Core Concepts](core-concepts.md) | Foundational model (Artifacts, Game, GameState, Events, Rules, Phases) |
| [Architecture](architecture.md) | Layered design, engine pipeline, capability seam |
| [Movement & Patterns](movement-and-patterns.md) | Pattern kinds, movement semantics charter summary, compiled vs legacy |
| [DecisionPlan & Acceleration](decision-plan-and-acceleration.md) | Rule evaluation pipeline & optimization flags |
| [Turn Sequencing](turn-sequencing.md) | Deterministic turn / segment model (feature‑flag gated) |
| [Determinism, RNG & Timeline](determinism-rng-timeline.md) | RNG abstraction, hashing, replay & timeline zipper |
| [Extensibility Guide](extensibility.md) | Adding games, events, mutators, conditions, phases |
| [Feature Flags](feature-flags.md) | List of flags, stability, defaults, related docs |
| [Performance & Benchmarks](performance.md) | Benchmarks schema, movement & fast-path metrics |
| [Benchmark Results](benchmark-results.md) | Latest consolidated benchmark summary (generated) |
| [Diagnostics & Observability](diagnostics.md) | Tracing, observers, metrics, rejection reasons |
| [Deck-building Module](deck-building.md) | Dominion-like deterministic deck-building core (supply, phases, scoring) |
| [Roadmap](ROADMAP.md) | Forward-looking capabilities & planned milestones |

## Mental Model (TL;DR)

Artifacts are immutable identities. GameState is an immutable snapshot chain. Events declare intent. Rules gate and transform via pure mutators. Phases scope rules. Feature flags opt into acceleration layers that preserve semantic parity (determinism first, speed second).

## Guarantees

* Deterministic transitions: same initial state + identical ordered event list + identical feature flag set ⇒ identical successor states.
* Immutability: no in-place mutation of GameState or artifact states.
* Acceleration Parity: compiled movement, sliding fast-path, decision plan optimizations cannot change externally observable results (guarded by parity tests & charters).
* Explicitness: no hidden side effects—every change emerges from an applied mutator.

## When to Read What

| If you need to… | Read |
|-----------------|------|
| Understand fundamentals | Core Concepts |
| Add a new game module | Extensibility Guide + Architecture |
| Investigate move legality | Movement & Patterns + DecisionPlan & Acceleration |
| Tune performance | Performance & Benchmarks + DecisionPlan & Acceleration |
| Enable deterministic replay tooling | Determinism, RNG & Timeline |
| Add a new optimization layer | DecisionPlan & Acceleration (parity charter) |
| Diagnose why an event was rejected | Diagnostics & Observability |

## Contributing to Docs

Keep sections small, link instead of duplicating, and update parity / invariants tables alongside code changes. Every semantic change (movement, sequencing, hashing) requires: updated charter, new/updated tests, CHANGELOG entry.
