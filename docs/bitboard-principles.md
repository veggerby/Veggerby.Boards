# Bitboard Principles

Authoritative internal contract for bitboard acceleration. These MUST remain true; violating them risks semantic drift or hidden state bugs.

## 1. Board Is Geometry; Bitboards Encode Occupancy

- `Board`, `Tile`, `TileRelation`, and `BoardShape` define immutable topology.
- Bitboards are a derived encoding of current piece occupancy only.
- No connectivity assumptions (adjacency, rays) may originate from bitboards; always defer to `BoardShape` / relations.

## 2. Always Derived, Never Authoritative

- `BitboardSnapshot` is rebuilt (or incrementally updated) from `GameState` + layouts.
- It stores no extra game truth (no piece types, no rule state); if lost it can be recomputed deterministically.
- Parity and regression tests must assert identical movement results with and without bitboards enabled.

## 3. Immutable + Incremental

- Snapshots are immutable value objects. Updates create new instances.
- Incremental `UpdateForMove` paths are strictly optional optimizations; fall back to full rebuild if pre‑conditions fail.
- No sharing snapshots across divergent `GameProgress` branches.

## 4. Optional Capability, Not Hard Dependency

- Bitboards are gated by `EnableBitboards` AND board size (≤ 64 tiles current implementation).
- Public API never exposes bitboard types or masks.
- Engine behavior (rules, path resolution) must remain correct when capability is absent.

## 5. Narrow, Explicit Capability Surface

- Exposed internally via `EngineCapabilities.Bitboards`, `EngineCapabilities.Shape`, `EngineCapabilities.Attacks`, `EngineCapabilities.Occupancy`.
- No generic service locator (removed `EngineServices`).
- No leaking raw `ulong` masks beyond internal occupancy / attack generators.

## 6. Determinism First

- Fast paths (sliding reconstruction, occupancy shortcuts) must yield identical paths as compiled / legacy resolution.
- On any doubt, prefer correctness path and emit a metrics skip counter.

## 7. Swappable Implementations

- Future: `Bitboard128`, piece-type masks, hybrid sparse encodings.
- Keep interfaces (`IOccupancyIndex`, `IAttackRays`, `IPathResolver`) stable so alternative encodings can register transparently.

## 8. Observability & Metrics

- Every fast-path attempt increments `Attempts`.
- Outcomes: `FastPathHits`, `CompiledHits`, `LegacyHits`, plus granular skip reasons (`NotSlider`, `NoServices`, `SkippedNoPrereq`, `AttackMiss`, `ReconstructFail`).
- Failures silently fall back; never throw due to acceleration logic.

## 9. Zero Hidden Mutation

- No in-place mutation of snapshots or shared arrays (clone before write).
- Capability objects are immutable references after builder completion (except `PathResolver` decoration which is a one-time wrap during build).

## 10. Simple Error Semantics

- If prerequisites missing (shape, piece map, snapshot mismatch) => skip fast path, never partial results.
- Mismatched indices in incremental updates return original snapshot (idempotent safe failure).

## 11. Testing Requirements

- Parity tests: sliders (rook, bishop, queen), non-sliders (knight, king), occupancy-blocked paths.
- Incremental update tests: move updates bitboards and piece map consistently (global + per-player masks).
- Negative tests: move with stale from-index leaves snapshot unchanged.

## 12. Performance Guidance

- No LINQ in hot loops (attack generation, path reconstruction, incremental update).
- Avoid allocations: reuse temporary buffers where safe; only allocate new immutable snapshot objects.
- Benchmarks must demonstrate non-regression before enabling new fast path by default.

---
Implementation note: These principles supersede prior references to the removed `EngineServices` container. Use `EngineCapabilities` for all new acceleration features.
