# Architecture upgrades

## 1) Compile rules into a fast decision plan

* **What**: At build/`Compile()` time, transform rules/conditions into a table-driven “decision plan” (think partial evaluation): pre-bind constant predicates, hoist common checks, and flatten phase gating into a compact jump table.
* **Why**: Cuts branching and LINQ during `HandleEvent`, improves determinism (same plan every run), and makes tracing easier (“Rule #7 failed: Precondition X=false”).
* **How**: Emit an immutable `DecisionPlan` that stores:

  * Precomputed condition bitsets per phase
  * A stable rule order + short-circuit masks
  * Pre-linked mutator delegates

## 2) Deterministic RNG boundary

* **What**: Introduce a mandatory `IRandomSource` (seeded) passed through `GameState` to own all randomness (dice, shuffles, etc.).
* **Why**: Reproducible simulations and history. Enables Monte-Carlo playouts and distributed verification.
* **How**: Provide default Xoshiro/Xoroshiro implementation; snapshot/restore the RNG state in history.

## 3) State history as a zipper + content-addressed snapshots

* **What**: Represent history as a **persistent zipper**: `(Past<…>, Focus<GameState>, Future<…>)`, with snapshots **Merkle-hashed** (content addressing).
* **Why**: O(1) undo/redo semantics, dedup of identical states, and cache keys for transposition tables or server caches.
* **How**: Hash structural + state fields; store move/event metadata alongside the hash to speed diff views.

## 4) Pattern engine as a compiled automaton

* **What**: Convert movement “patterns” (directional, repeatable, fixed sequences) into a small **NFA/DFA** per piece type at build time.
* **Why**: Removes per-move pattern interpretation, enabling fast path resolution and easy legality checks.
* **How**:

  * Compile directions → adjacency bitsets
  * Repeatable patterns → DFA with bounded depth
  * Result: hot-path becomes bitset stepping rather than list walking

## 5) Hot-path data layout for speed

* **What**: Keep the public model immutable, but in the evaluator use **packed, array-backed, struct-of-arrays** mirrors (no allocations) for tiles, relations, and occupancy.
* **Why**: 10–30× faster tight loops vs. object graphs + LINQ. Keeps purity, boosts perf.
* **How**: Internal `BoardShape` (arrays of neighbor indices), `PieceMap` (pieceId→tileIndex), and small `Span<int>` helpers.

## 6) Concurrency-safe simulation API

* **What**: Expose `Simulator` that takes a `Game` + `GameState` and runs **pure** playouts in parallel (no shared statics).
* **Why**: Enables search, hints, and bulk validation (e.g., generate all legal moves) without impact on the UI thread.
* **How**: `Simulator.Run(Func<GameState, Event> policy, int playouts, CancellationToken)` with cancellation and progress hooks.

## 7) Typed error channels with reasons

* **What**: Replace generic invalids with a small algebraic type: `EventResult = Accepted(nextState) | Rejected(Reason, Trace)`.
* **Why**: Better UX (why a move is invalid), vastly easier debugging and automated tests (“should reject because PhaseMismatch”).
* **How**: `Reason` union (PhaseClosed, BlockedPath, Ownership, OutOfBounds, DiceMismatch, etc.) + optional rule id.

## 8) Module isolation & versioned DTOs

* **What**: Keep game modules (Chess, Backgammon) in separate packages; publish **versioned** API DTOs (`v1`, `v2`).
* **Why**: Clean extension surface, non-breaking API over time, easy to add new games without touching core.
* **How**: `Veggerby.Boards.Chess`/`Backgammon` packages; API layer maps engine → DTOs with `ApiModelV1`.

## 9) Observability hooks

* **What**: Provide structured events around evaluation: `OnPhaseEnter`, `OnRuleEvaluated`, `OnMutatorApplied`, `OnStateHashed`.
* **Why**: Pluggable logging, metrics, flamegraphs for rule costs, and a great REPL/visualizer experience.
* **How**: `IEvaluationObserver` injected at `Compile()`; default is no-op.

## 10) Bitboards where it pays

* **What**: For Chess-like boards, keep an internal bitboard representation to accelerate attack masks/legal move generation.
* **Why**: It’s the de-facto standard for speed. You still expose the generic graph model externally.
* **How**: A chess module service that syncs `GameState` ⇄ bitboards only during evaluation.

# Developer experience (DX) upgrades

## 11) Model checker + property tests

* FsCheck/QuickCheck style invariants:

  * “Number of pieces is constant per color unless captured”
  * “Moves that don’t change RNG state are deterministic”
  * “History undo/redo returns to identical hash”
* Catch “it works until observed” flakiness early and preserves your determinism guarantees.

## 12) Decision trace & visualizer

* A minimal web or CLI visualizer that shows:

  * Active phase, attempted event, failing predicates
  * Resolved path overlays on the board
  * Before/after diffs and hashes
* Makes debugging rules/conditions delightful.

## 13) Performance guardrails

* Benchmarks pinned to **allocs/op and ns/op** for hot paths (`ResolvePath`, `HandleEvent`, `AllLegalMoves`).
* CI fails on >1–2% regression. Include a “first-run” cold benchmark to catch JIT/caching surprises.

## 14) Replayable bug reports

* Ship a `BugReport` envelope (seed, inputs, decision plan id, event stream) so any issue can be replayed verbatim.
* Great for OSS contributions and CI re-runs.

## 15) Public extension points (stable)

* Light interfaces for **Conditions**, **Mutators**, **PreProcessors**, **Heuristics**.
* Document the minimal lifecycle and guarantees (pure, no I/O, no time, no static state).

# Small but high-impact refactors

* **No LINQ in hot paths**: replace with simple loops; keep LINQ for builder/document-like code.
* **Record structs for tiny value types** (tile index, player id) to reduce GC pressure.
* **Adjacency caches** keyed by board hash (safe due to immutability).
* **Guard against hidden globals**: require any “environmental” data (time, RNG) to flow through `GameState`.

# What you’ll get

* Faster `HandleEvent` and move generation without giving up immutability.
* Reproducible simulations & tests (seeded RNG + hashed states).
* Much clearer diagnostics when a move is rejected.
* Cleaner module boundaries and future-proofed API.

If you want, I can sketch the `DecisionPlan` and `IEvaluationObserver` shapes next, and outline a migration path that doesn’t break existing Backgammon/Chess modules.
