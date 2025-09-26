# Roadmap (Condensed)

High-level forward plan distilled from internal planning. Subject to change; any semantic adjustments update charters, documentation, and tests first.

## Near Term

* Turn sequencing: custom segment profiles, ordering strategies.
* Enable sliding fast-path by default after extended parity soak.
* DecisionPlan: potential static mask precomputation refinements.
* Simulation: richer metrics (depth histograms, rejection counts) & benchmark harness.

## Mid Term

* 128-bit-only hashing mode & hash interning.
* Bitboard128 for >64 tile boards.
* Simultaneous commit (auction / hidden orders) segment.
* External reproduction envelope (seed + events + final hash JSON).

## Long Term

* Advanced ordering strategies (priority / initiative, dynamic insertion/removal).
* Multi-lane (team) turn streams.
* Movement pattern expansion (conditional, leaper, wildcard search semantics) with updated charter.
* Analyzer suite enforcing determinism & style rules (forbidden APIs, hidden globals, hot-path LINQ).

## Principles Recap

1. Determinism before performance.
2. Flags isolate experimental features.
3. Charters define semantics; tests enforce them.
4. Public surface stays minimal & explicit.

## Contributing to Roadmap

Open an issue describing: problem, proposed abstraction, determinism considerations, and benchmark target. Provide a minimal semantics draft; discussion precedes implementation.
