# Board vs Bitboard

This document clarifies the distinct roles of the high‑level `Board` domain model and the internal bitboard representations, how they coexist, when they overlap, and the rules the engine follows to prefer one over the other. It is intended as architectural orientation for contributors extending movement, acceleration, or evaluation logic.

## 1. Conceptual Roles

| Aspect | `Board` | Bitboards (`BitboardLayout` + `BitboardSnapshot`) |
|--------|---------|--------------------------------------------------------------|
| Abstraction Level | Domain / semantic | Low-level data layout (occupancy masks) |
| Purpose | Capture immutable topology (tiles + directed relations / edges) | Provide constant‑time occupancy queries (global + per‑player) and enable fast sliding attack generation |
| Mutability | Immutable artifacts (`Tile`, `TileRelation`) | Snapshots rebuilt incrementally per move (new snapshot produced) |
| Identity Source | Authored via `GameBuilder` | Derived from `Game` (ordinal indices assigned deterministically) |
| Scope | General path & rule evaluation (authoritative geometry) | Performance optimization; never authoritative for topology |
| Feature Flag | Always present | Gated by `EnableBitboards` (and tile count ≤64 today) |
| Failure Mode | N/A (core) | Silently absent; engine falls back to non‑bitboard path |

## 2. The `Board` Model

The `Board` aggregate is a graph: a set of `Tile` nodes plus directional `TileRelation` edges (each relation: Source → Target with a `Direction`). It is:

- The single source of truth for movement geometry.
- Immutable after construction (built only through `GameBuilder`).
- Consumed directly by legacy visitor path resolution and compiled pattern resolvers.
- Used to build derived acceleration structures (`BoardShape`, adjacency tables, sliding ray maps).

`Board` never contains occupancy; piece positions live in `GameState` (via `PieceState`).

## 3. Bitboard Structures

Bitboards are an internal acceleration layer comprising:

- `BitboardLayout`: stable indices for players + pieces (deterministic ordering by id) enabling packing into mask arrays.
- `BitboardSnapshot`: immutable snapshot holding:
  - `ulong Global` (all occupied tiles)
  - `ulong[] PerPlayer` (one mask per player) — currently only for ≤64 tile boards.
// (Legacy wrappers like `BitboardServices` have been removed; layout + snapshot now bind directly into the internal acceleration context.)

These are constructed in `GameBuilder` only when:

1. `FeatureFlags.EnableBitboards` is true.
2. `game.Board.Tiles.Count() <= 64` (current guard to keep a single 64‑bit wide representation).

If conditions fail, the engine omits bitboard services; callers must fall back gracefully.

### 3.1 Snapshot Lifecycle

On game creation a `BitboardSnapshot` is built from the initial `GameState`. For each move event, an incremental mutator produces a new snapshot (clear source bit, set destination bit; plus per‑player masks). Snapshots are immutable—never modified in place.

### 3.2 Provided Capabilities

- O(1) global occupancy test (bit AND).
- O(1) per‑player occupancy test.
- Constant‑time emptiness checks for sliding ray traversal.
- Efficient reconstruction of sliding piece paths via precomputed directional attack maps (`SlidingAttackGenerator`).

Bitboards deliberately DO NOT encode:

- Tile adjacency (that remains in `BoardShape`).
- Piece type semantics (future: optional type‑specific masks).
- Rule gating, phase state, or historical ordering (those live in `GameState`).

## 4. Overlap & Cooperation

| Concern | Source of Truth | Bitboard Usage |
|---------|-----------------|----------------|
| Tile topology / directions | `Board` / `BoardShape` | Precomputed rays use topology but bitboards do not redefine it |
| Occupancy (who is where) | `GameState` (PieceState) | Mirrors into masks for fast queries |
| Path generation (non-sliding) | Compiled patterns / legacy visitor using `Board` | Bitboards unused (fast-path bypasses) |
| Path generation (sliding) | `Board` + attack rays | Bitboards accelerate blocker detection & path reconstruction |
| Validation / rules | Rules over `GameState` + patterns | Bitboards may short-circuit occupancy checks but never replace rule evaluation |

Bitboards are strictly a cache/derived view. Any divergence would be a bug; parity tests ensure identical outcomes whether the fast-path (bitboards) or compiled/legacy path is used.

## 5. Engine Integration Decision Flow

The `GameBuilder` composes capabilities in this order (internal only):

1. Always create `BoardShape` (topology abstraction over `Board`).
2. Optionally compile patterns (`EnableCompiledPatterns`).
3. If bitboards enabled and tile count ≤64 build layouts + snapshots + occupancy index + attack rays (all inside acceleration context).
4. Construct path resolver (compiled or visitor) and optionally layer sliding fast-path (feature flagged) on top.

At runtime, sliding path resolution proceeds:

```text
ResolveSlidingPath (conceptual):
  if (sliding fast-path enabled AND internal bitboards present AND piece is slider):
      attempt ray mask + occupancy reconstruction
      if success: return path
      else: fall through
  Fallback chain: compiled resolver → visitor (legacy)
```

The decision path ensures deterministic behavior: identical inputs produce identical outputs independent of acceleration availability.

## 6. When Bitboards Are Not Used

Bitboards (and sliding fast-path) are skipped when:

- Flag disabled (`EnableBitboards == false`).
- Board has >64 tiles (current guard; future Bitboard128 extension).
- Piece pattern is not a repeatable directional slider (e.g., knight, pawn single-step, fixed pattern piece).
// Legacy skip reason (services missing) removed with service-locator elimination; presence is now deterministic with feature flags.

In all cases, the compiled pattern resolver (or legacy visitor) is authoritative fallback.

## 7. Future Extension (Bitboard128)

A planned upgrade (see `docs/perf/bitboard128-design.md`) introduces a dual-`ulong` structure for boards up to 128 tiles. Selection logic would branch on tile count:

- ≤64 → current 64-bit snapshot.
- 65–128 → 128-bit snapshot variant.
- >128 → disable bitboard acceleration.

No public API changes anticipated; only internal layout & fast-path discrimination.

## 8. Design Principles & Invariants

- Immutability: Every state transition yields a new `GameState`, and bitboards produce a new snapshot reflecting that change.
- Determinism: Fast-path never alters semantics; failures fall back without side effects.
- Isolation: Bitboards never mutate or store references back into `Board`; they consume IDs & indices only.
- Observability: Metrics capture attempts, hits, and granular skip reasons for regression detection.
- Non-intrusive: Absence of bitboards must not impact correctness—only performance.

## 9. Decision Matrix (Simplified)

| Condition | Fast-Path Eligibility | Action |
|-----------|-----------------------|--------|
| Flags off | No | Use compiled / legacy |
| >64 tiles | No | Use compiled / legacy |
| Non-slider piece | No | Use compiled / legacy |
| Services missing | No (SkipNoServices) | Use compiled / legacy |
| All prerequisites + slider | Yes | Attempt bitboard attack + path reconstruction |

## 10. Contributor Guidance

- Use `Board` (or `BoardShape`) for topology questions (adjacency, directional expansion).
- Use bitboard services ONLY when you need occupancy set logic in hot paths and prerequisites are met; otherwise prefer existing abstractions for clarity.
- Do not introduce public API types referencing bitboards; they remain an internal optimization boundary.
- Any new acceleration (e.g., mobility heuristics) must preserve determinism and be guarded by feature flags until fully validated.

## 11. Glossary

| Term | Definition |
|------|------------|
| Board | Graph of tiles + relations (source of geometry) |
| BoardShape | Derived structure describing directional adjacency & classification |
| Bitboard | Packed occupancy mask (global or per-player) in an unsigned integer |
| BitboardLayout | Deterministic index mapping for players/pieces used by snapshots |
| BitboardSnapshot | Immutable occupancy state (global + per-player masks) |
| SlidingAttackGenerator | Precomputed directional ray masks for sliding pieces |
| Fast-Path | Short-circuit sliding path resolver using bitboards + attack rays |

---

Last updated: 2025-09-24

## 12. Convergence Strategy (Service Layer, Not Artifact Layer)

Short answer: **Yes—converge at the *service* layer, not the *artifact* layer.** Keep `Board` as the single immutable topology artifact; present alternative acceleration data layouts (bitboards now, Bitboard128 later) behind narrow capability interfaces. This collapses “choose Board or Bitboard” into “always ask the same service,” preserving determinism and keeping acceleration optional.

### 12.1 What to Converge (Interfaces)

| Capability | Proposed Interface | Minimal Members (illustrative) | Implementations |
|------------|-------------------|---------------------------------|-----------------|
| Topology (adjacency, rays) | `IBoardShape` (already implicit) | `IEnumerable<Direction> Directions {get;}` / `TryGetNeighbor(tile,dir)` / `GetRays(tile)` | Backed by `BoardShape` only (no alternate impl needed) |
| Occupancy / ownership | `IOccupancyIndex` | `bool IsEmpty(Tile)`, `bool IsOwnedBy(Tile, Player)`, `ulong GlobalMask`, `ulong PlayerMask(Player)` | `NaiveOccupancyIndex`, `Bitboard64OccupancyIndex`, future `Bitboard128OccupancyIndex` |
| Sliding attack geometry | `IAttackRays` | `ReadOnlySpan<ulong> RaysFor(Tile)` (format internal) | `SlidingAttackGenerator` (topology-derived) |
| Path resolution | `IPathResolver` | `TilePath? Resolve(Piece, Tile from, Tile to)` | `CompiledPatternResolver`, `SlidingFastPathResolver` (wraps & falls back) |

Interfaces remain internal until stability; they’re seams for substitution, not public customization points.

> Status: Landed interfaces: (1) `IOccupancyIndex` (`NaiveOccupancyIndex` / `BitboardOccupancyIndex`), (2) `IAttackRays` (`SlidingAttackGenerator`), and (3) initial `IPathResolver` (compiled-only adapter; sliding fast-path decorator scaffolded). Next step: complete fast-path reconstruction logic inside `SlidingFastPathResolver` to exploit rays + occupancy before fallback.

### 12.2 Wiring (GameBuilder Sketch)

```csharp
// Pseudocode inside GameBuilder after Board constructed
services.Set(boardShape);                // IBoardShape
var occ = BuildOccupancyIndex(game, flags); // IOccupancyIndex (naive / bitboard64 / bitboard128)
services.Set(occ);
var rays = flags.EnableBitboards ? BuildAttackRays(boardShape) : NullAttackRays.Instance; // IAttackRays
services.Set(rays);
var compiled = new CompiledPatternResolver(...);              // always available
IPathResolver resolver = compiled;
if (flags.EnableSlidingFastPath && occ.SupportsSliding && rays.Available)
{
  resolver = new SlidingFastPathResolver(rays, occ, compiled); // decorator / fallback
}
services.Set(resolver);
```

Decision logic lives centrally; downstream code simply requests `IPathResolver` → determinism unaffected by presence/absence of acceleration.

### 12.3 Contributor Guidance (Updated)

- Use `IBoardShape` for geometry questions only.
- Never special‑case bitboards in rules; request `IOccupancyIndex` and rely on its polymorphism.
- Always go through `IPathResolver` when constructing movement; do not “manually” invoke fast-path helpers.
- Avoid exposing raw masks outside internal layers—keep bit-level data an implementation detail.

### 12.4 Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Interface proliferation / over-abstraction | Keep method sets minimal; add only with benchmark + parity justification. |
| Drift between naive and bitboard occupancy semantics | Run full parity suite twice (naive vs bitboard index) in CI. |
| Hidden performance regressions in decorator chain | Metrics on `IPathResolver` attempts/hits + benchmark gates before enabling by default. |
| Accidental public API bleed | Keep interfaces `internal` until two minor versions stable. |

### 12.5 Future (Bitboard128 Drop-In)

Add `Bitboard128OccupancyIndex` implementing `IOccupancyIndex`; `BuildOccupancyIndex` chooses based on tile count ranges (≤64 / 65–128 / else naive). No other caller changes required.

---

Last updated (convergence section added): 2025-09-24
