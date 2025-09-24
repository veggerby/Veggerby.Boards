# Movement Semantics Charter

Authoritative specification for movement path resolution and occupancy rules in Veggerby.Boards.

## Scope

Covers sliding and non-sliding piece movement interpretation for path resolution APIs (`ResolvePathCompiledFirst`, compiled resolver, legacy visitor) and post-resolution occupancy validation.

## Terminology

- **Sliding Pattern**: Repeatable directional pattern (e.g., rook/bishop/queen rays) expressed via `DirectionPattern(IsRepeatable=true)` or `MultiDirectionPattern(IsRepeatable=true)`.
- **Non-Sliding Pattern**: Fixed or single-step pattern (e.g., knight L, pawn single advance) or repeat=false patterns.
- **Ray**: Ordered sequence of tiles from a source following one direction until board edge or blocker.
- **Blocker**: Occupied tile encountered along a ray before reaching (or at) the target.
- **Friendly Blocker**: Blocker occupied by a piece with the same owner as the moving piece.
- **Enemy Blocker**: Blocker occupied by an opposing piece.

## Sliding Rules

Given a piece P on source tile S attempting to move to target tile T via a sliding pattern:

1. **Empty Ray**: If every tile between S exclusive and T exclusive is unoccupied and T is unoccupied → move permitted.
2. **Friendly Blocker**:
   - If a friendly piece occupies any intermediate tile → movement invalid (cannot land on or pass through).
   - If a friendly piece occupies T → movement invalid (cannot capture friendly).
3. **Enemy Blocker**:
   - If the first blocker along the ray is an enemy on tile B:
     - If B == T → capture permitted (path terminates at B).
     - If B != T → movement invalid (cannot jump beyond enemy blocker).
4. **Multi-Blocker Precedence**:
   - Only the first occupied tile along the ray matters; tiles beyond it are not considered for legality.
5. **Edge / Zero-Length Cases**:
   - S == T → no movement; path resolution returns null.
   - Adjacent target (ray length 1) follows the same rules above (capture or move if permitted).

## Non-Sliding Rules

- Non-sliding patterns enumerate discrete destination offsets / sequences.
- Occupancy semantics (friendly block prohibition, enemy capture allowance) are enforced identically via the shared post-filter.
- Fast-path optimizations MUST NOT engage for non-sliding pieces (guard ensures parity and prevents accidental geometric leakage).

## Resolver vs Post-Filter Responsibilities

| Layer | Responsibility | Determinism Requirements |
|-------|----------------|--------------------------|
| Fast-Path (Sliding) | Geometric reachability only (target in sliding attack set + path reconstruction). Does NOT finalize occupancy legality. | Same inputs (piece id, from, to, board state snapshots) → same inclusion/exclusion in attack set. |
| Compiled Resolver | Geometric resolution via precompiled IR patterns. | Stable order of relation traversal; no random branching. |
| Legacy Visitor | Geometric resolution via runtime pattern traversal. | Deterministic traversal order by pattern definition list. |
| Post-Filter (`ApplyOccupancySemantics`) | Enforce blocker and capture rules uniformly (for ANY path origin). | Deterministic scan of path relations left→right. |

## Determinism Guarantees

- Same GameState + identical feature flag set + identical (piece, from, to) yields:
  - Identical fast-path attempt outcome (hit/skip) and identical resulting path (or null) after post-filter.
  - Hash stability: enabling fast-path must not alter resulting legal path set compared to compiled+legacy fallback.
- Metrics counters are observational and do not influence legality.

## Failure Modes & Guards

| Potential Issue | Guard / Mitigation |
|-----------------|--------------------|
| Fast-path returns single-step path for non-sliders | Non-slider guard (checks for repeatable directional pattern). |
| Path reconstruction diverges from geometric attack membership | Reconstruct step-by-step via `BoardShape` neighbor traversal; if a neighbor missing, abandon fast-path silently. |
| Occupancy semantics drift between fast-path and compiled path | Centralized post-filter applied to ALL resolved paths before returning to callers. |

## Testing Matrix (Parity V2 Reference)

A comprehensive test suite SHALL cover:

- Friendly blocker mid-ray (each orthogonal + diagonal direction) → null.
- Enemy blocker at target → capture allowed.
- Enemy blocker before target → null.
- Multi-blocker (friendly then enemy) → friendly decides (null).
- Multi-blocker (enemy then friendly) → enemy at target captured only if it is target; otherwise null.
- Adjacent capture and adjacent friendly block.
- Zero-length (from == to) → null.
- Non-slider queries (knight/pawn) → fast-path skipped.

## Out of Scope (Future Work)

- Conditional movement (e.g., en passant, castling) – involves rule-level context not expressible by geometric fast-path alone.
- Pattern modifiers (jump-over, teleport) – would bypass standard occupancy semantics and require explicit rule gating.

## References

- `GameExtensions.ResolvePathCompiledFirst`
- `FastPathMetrics` (internal)
- `ApplyOccupancySemantics` static helper
- `BoardShape` topology classification (future specialization hook)

---

Last updated: 2025-09-24
