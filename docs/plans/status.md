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

⏳ **Partial.**

* Bitboards exist, but incremental update path disabled.
* Sliding fast-path enabled (≤64 tiles) and strong perf numbers.
  ⚠️ Missing:

  * Bitboard128 path (>64 tiles).
  * Re-enable incremental path behind soak.
  * Per-piece masks overhead validation.
  * Heuristic pruning (topology-aware early exit, mask assisted).
  * Full LINQ removal across hot paths.
    ⚠️ Docs mismatch: defaults (`EnableBitboards=false`, `EnableSlidingFastPath=true`) not consistently reflected.

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

⏳ **Partial.**

* TurnState, segments, events, metrics overhead all in place.
  ⚠️ Missing:

  * Hash parity validation (flag vs baseline).
  * Real adoption example (e.g., Go two-pass termination).
  * Legacy active player path removal.
  * Default graduation (currently `false`).

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

* **Feature Flag Governance:** Many flags experimental. Needs central table (Graduated / Experimental / Deprecated) with graduation criteria.
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

⏳ **Planned.**

* Scope: deterministic supply, deck/hand/discard zones, draw & shuffle (seeded), action/buy/cleanup phases, scoring.
* Pending: shuffle artifact, phase sequencer conditions, gain/trash mutators, scoring aggregator, tests & benchmarks.
* Risks: overbuilding effect system, shuffle allocation cost, premature variant phases.
