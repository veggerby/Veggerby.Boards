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

🆕 **Planned.**

* Scope: state extensions (castling rights, en passant), occupancy-aware generation, legality (king safety) filter, special move events, full SAN (#, promotion, en passant), stalemate detection.
* Pending: implementation kickoff; acceptance & metrics defined in workstream file.
* Risks: performance regressions in hot path if generation naive; test surface expansion (many edge cases) requiring careful property tests.

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
