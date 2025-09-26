# Movement & Patterns

Authoritative semantics for movement pattern kinds and resolution layers.

## Pattern Kinds

| Pattern | Description | Repeatable | Compiled Support |
|---------|-------------|------------|------------------|
| DirectionPattern | Single direction | opt | Yes (Ray) |
| MultiDirectionPattern | Multiple alternative directions | opt | Yes (MultiRay) |
| FixedPattern | Explicit ordered steps | No | Yes (Fixed) |
| NullPattern | No movement placeholder | No | Ignored |
| AnyPattern | Wildcard / future expansion | N/A | Not compiled |

Compilation yields IR forms: Fixed, Ray, MultiRay. Unsupported patterns fall back to the legacy visitor implementation.

## Sliding Semantics

1. Single blocker authority: only the first occupied tile along a ray determines legality.
2. Friendly blocker before or at target ⇒ move invalid (cannot pass or capture friendly).
3. Enemy blocker before target ⇒ invalid (cannot jump); enemy exactly at target ⇒ capture (path ends there).
4. Zero‑length (source==target) requests are never legal (null path).
5. Deterministic tie-breaking: when multiple rays or patterns can reach the same target with equal length, the first discovered according to the piece's pattern declaration order wins.
6. Multiple blockers (friendly then enemy, enemy then friendly, two enemies) reduce to rule 1: earliest blocker decides outcome; no further inspection.
7. Path reconstruction never returns a partial prefix; either a full source→target sequence or null.

## Fast-Path & Bitboards

When enabled (bitboards + sliding fast-path flags), sliding reachability is computed via precomputed directional rays + occupancy bitmasks. A successful membership test triggers deterministic path reconstruction via neighbor arrays; otherwise compiled IR / visitor fallback resolves geometrically. Occupancy legality is uniformly enforced post-resolution (same blocker logic for all layers).

### Acceleration Components

| Component | Responsibility |
|-----------|----------------|
| BoardShape | Tile index mapping + directional neighbor arrays |
| PieceMapSnapshot | Piece ownership + tile indices (incremental update on moves) |
| BitboardSnapshot | Global + per-player occupancy bitmasks (≤64 tiles) |
| SlidingAttackGenerator | Precomputed directional rays + blocker truncation to produce attack set |
| Fast-Path Resolver | Combines attack membership + reconstruction for candidate sliding moves |

All components are internal; semantics are defined solely by pattern definitions + GameState.

## Parity Guarantees

Compiled + fast-path layers must produce identical legal path sets to the visitor for supported patterns (guarded by curated + randomized parity tests). Any semantic change requires updating this document and associated tests before merging.

## Edge Cases & Guards

* Adjacent capture uses same blocker rules (enemy at target allowed, friendly forbidden).
* Multiple rays reaching same target choose earliest by pattern order (no global shortest-path search across heterogeneous pattern kinds).
* Non-sliders (fixed patterns, non-repeatable directions) never invoke the sliding fast-path.
* Any reconstruction failure (topology inconsistency) causes silent fallback; visitors still must yield identical legality (tests would fail if divergence appears).
* No implicit jump mechanics: leaper patterns must be modeled via explicit fixed sequences (e.g., knight) where intermediate occupancy is ignored by construction, not by skipping rules here.

See also: `decision-plan-and-acceleration.md` for rule evaluation integration.
