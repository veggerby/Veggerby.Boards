# Work Status

## Outstanding by Workstream

### 1. Rule Evaluation Engine

✅ **Done.** DecisionPlan fully replaced legacy traversal.
⚠️ Remaining: finer skip categorization (Invalid vs Ignored). Tracked under Observability.

---

### 2. Deterministic RNG & State History

✅ **Done.** Replay determinism, zipper, dual hashing all landed.
📦 Deferred: external replay envelope, hash interning, timeline diff. Non-blocking.

---

### 3. Movement & Pattern Compilation

🔒 **Closed** for milestone.
⚠️ Deferred throughput goal (aggregate ≥5× speed).
⚠️ Missing compilation kinds: conditional, leaper, wildcard.
⚠️ LINQ sweep in visitors still outstanding.

---

### 4. Performance Data Layout & Hot Paths

⏳ **Partial (Expanded).**

* Bitboards: incremental update path reintroduced behind `EnableBitboardIncremental` (default off) with scripted, soak, and deterministic randomized parity suites. Added focused `BitboardSnapshot` unit tests (64/128 build + update paths) to strengthen coverage.
* Bitboard128 scaffolding added (global + per-player occupancy up to 128 tiles); acceleration selection extended (≤128 uses bitboards) while ≤64 fast path unchanged.
* Sliding fast-path enabled (≤64 tiles) with strong perf numbers.
  ⚠️ Remaining:
  * Graduate incremental path (large randomized + multi-module soak; hash parity post hashing flag).
  * Per-piece / piece-type masks and overhead validation (both 64 and 128 variants).
  * Heuristic pruning (topology + occupancy guided early exit).
  * LINQ removal across hot mutators / visitors.
  * Documentation sync for feature flag defaults & Bitboard128 constraints.

---

### 5. Concurrency & Simulation

✅ **Done.** Simulators + metrics in place.
📦 Deferred: histograms, expanded rejection metrics.

---

### 6. Observability & Diagnostics

⏳ **Partial.**

* Observer hooks and trace capture exist.
  ⚠️ Missing:

  * CLI trace viewer (consumability).
  * Skip reason classification (Invalid vs Ignored).
  * Composite skip capture.
    ⚠️ Graduation blockers: without a viewer, trace flag must stay off.

---

### 7. Developer Experience & Quality Gates

✅ **Done.** Style, property tests, rejection exhaustiveness guards in place.
📦 Deferred:

* CI benchmark regression gate.
* Roslyn analyzers for Random, LINQ in hot path, namespace enforcement.

---

### 8. Structural Refactors

⏳ **Partial.**

* EngineCapabilities replaced service locator.
  ⚠️ Missing: residual LINQ sweeps, record struct wrappers, analyzer enforcement.

---

### 9. Unified Turn / Round Sequencing

⏳ **Partial (Graduated Core).**

* Turn sequencing flag now defaults ON; initial TurnState emitted and advancement mutators (advance, pass, replay, commit) consolidated with shared rotation helper.
* Determinism tests added (scripted advancement + pass/replay streak reset). Docs updated with clear guidance on `TryGetActivePlayer(out Player)` vs `GetActivePlayer()` usage (conditions/gates vs strict flows).
  ⚠️ Remaining:
  * Two-pass termination adoption in Go module (terminal condition wiring).
  * Legacy active player projection replacement (derive from TurnState in projection layer) – rotation helper still used.
  * Hash parity snapshot test once state hashing feature flag is activated.
  * Documentation note for sequencing lifecycle (planned `turn-sequencing.md`).

---

### 10. Chess Full Move Legality

⏳ **Partial.**

* Scope: state extensions (castling rights, en passant), occupancy-aware generation, legality (king safety) filter, special move events, full SAN (#, promotion, en passant), stalemate detection.
  * Progress update: Castling rights + safety filter implemented; explicit `Castle` helper; metadata-driven role/color predicates; identifier normalization via `ChessIds`; en-passant & castling mutators migrated to predicates; coverage guard active. Remaining: pseudo-legal generation API, promotion, mate/stalemate, SAN completion (#, =Q, e.p.).
* Pending: generation + legality filter implementation & benchmarks.
* Risks: generation perf, edge-case explosion in legality tests, predicate overhead (to benchmark).

---

### 11. Go Game Module

⏳ **Partial.**

* Scope: stone placement, capture/group & liberty resolution, suicide rule, simple ko, double-pass termination, scoring (area first), nomenclature.
  * Progress update: Builder + board topology (orthogonal liberties), stone pools, placement (emptiness only), pass event increments counter, minimal nomenclature, extras scaffold (ko/pass/size). Remaining: capture & liberty evaluation, suicide enforcement, ko detection, pass-termination -> terminal flag, scoring, coordinate nomenclature, tests & benchmarks.
* Pending: group/liberty resolver + capture logic design.
* Risks: capture evaluation performance on 19x19, ko edge-cases (snapback misclassification), scoring determinism.

---

## Cross-Cutting Gaps

* **Feature Flag Governance:** Central table exists in `feature-flags.md` and is being kept current (owner, defaults, graduation notes). Continue pruning deprecated scaffolds in next minor.
* **Benchmarks:** Numbers are scattered. Need a single summary doc (with last commit hash).
* **Cross-Platform Hash CI:** Replay determinism verified locally but not enforced in CI across OS/arch.
* **Diagnostics UX:** Trace capture exists but no viewer. CLI viewer MVP would unlock graduation.
* **LINQ Sweep:** Still pending in several hot/event paths.
* **Analyzer Coverage:** Stub only, risk of style drift.

---

## Top Priorities (Next)

1. **Acceleration Heuristics & Bitboards**

   * Re-enable incremental updates.
   * Add topology pruning / per-piece mask heuristics.
   * Stress parity tests + benchmark reruns.

2. **Turn Sequencing Graduation**

   * Add Go module proto with two-pass termination.
   * Hash parity baseline tests.
   * Remove legacy active player handling.

3. **Diagnostics Consumability**

   * Minimal CLI trace viewer.
   * Richer skip classification.
   * Observer batching perf summary.

4. **Flag Governance**

   * Create `feature-flags.md` status table.
   * Prune deprecated scaffolds next minor.

5. **Analyzer & CI Hardening**

   * Implement minimal Roslyn rules.
   * Add cross-platform determinism CI job.

---

### 12. Ludo / Parcheesi Game Module

⏳ **Planned.**

* Scope: race track + home stretches, safe squares, entry on 6, capture reset, win when all tokens home.
* Pending: full builder, movement/capture conditions, win detection tests.
* Risks: variant creep (extra-turn on 6, stacking) inflating baseline.

### 13. Checkers / Draughts Game Module

⏳ **Planned.**

* Scope: dark-square graph, forward men, bidirectional kings, mandatory capture, multi-jump chains, kinging, immobilization/ elimination win.
* Pending: capture chain enumerator, deterministic path ordering, kinging mutator, tests & benchmarks.
* Risks: branching capture explosion performance; variant divergence early.

### 14. Monopoly Game Module

⏳ **Planned.**

* Scope: board cycle, property acquisition, rent, jail, chance/community deck (subset), doubles logic, bankruptcy elimination.
* Pending: deck artifacts & deterministic shuffle, rent & cash transfer mutators, jail state flow, win detection tests.
* Risks: economic complexity creep (houses/auctions) prematurely.

### 16. Risk Game Module

⏳ **Planned.**

* Scope: territory graph, reinforcement calc (territories/3 min 3 + continent bonus), combat dice resolution, conquest ownership transfer, elimination, domination win.
* Pending: reinforcement condition implementation, combat resolution mutators, win detection tests, benchmarks.
* Risks: early card mechanic inclusion expanding surface; combat allocation overhead.

### 17. Deck-building Core Module

⏳ **Partial (Expanded).**

Delivered so far:

* Project scaffolding with `DeckBuildingGameBuilder` and `CardDefinition` artifact.
* Player zones over `Cards` piles with deterministic transitions backed by seeded RNG.
* Events/Rules/Mutators implemented and wired:
  * `RegisterCardDefinitionEvent` (register metadata definitions)
  * `CreateDeckEvent` (initialize piles and optional supply snapshot)
  * `GainFromSupplyEvent` (decrement supply, append to target pile)
  * `DrawWithReshuffleEvent` (reshuffle Discard deterministically into Draw when needed, then draw to Hand)
  * `TrashFromHandEvent` (remove specified cards from Hand)
  * `CleanupToDiscardEvent` (move all cards from Hand and InPlay to Discard)
  * `ComputeScoresEvent` (aggregate victory points -> `ScoreState` per player, idempotent)
  * `EndGameEvent` (append terminal `GameEndedState` marker post-scoring)
* Tests covering gain-from-supply acceptance/rejection, reshuffle determinism, trash validation, cleanup behavior, scoring aggregation/idempotency, termination gating (pre-score ignore, post-score success), and EndGame ordering invariant (ComputeScores precedes EndGame in cleanup phase).
* Deterministic DecisionPlan baseline locked & updated (added scoring + termination) with guard test + diff; signature advanced.
* Structural invariants + explicit ordering invariant ensure presence and sequencing (ComputeScores → EndGame) across phases.
* Feature flag guard + sequential test collection removed flakiness from shared sequencing flag.
* Action / Buy phase split completed (separate `db-action` and `db-buy`).
* Scoring + termination integrated; baseline signature advanced & ordering invariant added.

Next:

* Supply configurator scaffold (`DeckBuildingSupplyConfigurator`) delivering fluent card definition + supply registration and deterministic startup event emission (definitions + single create) with ordering, duplicate, undefined supply, and integration tests.
* Dedicated module docs page (`deck-building.md`) published (phases table, zones, shuffling determinism, supply usage, end-to-end flow, error modes, extension points).
* Benchmarks (shuffle throughput, draw cycle, zone transition overhead, scoring cost) – pending.
* Optional: alternate end-game trigger (supply depletion) + additional invariants – pending.

Risks: overbuilding effect system; maintain minimal primitives until card effects require expansion. Baseline regeneration discipline required for future phase additions.

---

## New Capability Delivered – Cards & Decks Module

✅ Initial Cards module (`Veggerby.Boards.Cards`) implemented.

* Scope: card/deck artifacts, immutable `DeckState` with named ordered piles, events (create, shuffle, draw, move, discard), builder wiring using DecisionPlan DSL.
* Determinism: shuffles use `GameState.Random`; seeding via `GameBuilder.WithSeed` yields reproducible order.
* Tests: create+draw happy path, deterministic shuffle parity across seeded builders, invalid draw rejection via rule condition.
* Invariants: minimal board topology and two players included in builder to satisfy core engine requirements.

Open follow-ups:

* Documentation page under `/docs/cards` with usage and deterministic semantics (quick start mirrors tests).
* Optional v1 extensions: peek/reveal, gain from supply, reshuffle-on-empty policy as explicit event.
* Workstream linkage: informs Workstream 17 (Deck-building Core) as a foundational subset (zones/piles and shuffle reproducibility).
