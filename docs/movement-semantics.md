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

## Edge Cases & Detailed Semantics (Charter Extension 2025-09-25)

This section codifies nuanced / rarely hit behaviors to lock determinism and guide future pattern or rule extensions. All items are testable invariants; introducing new movement kinds must not invalidate them without explicit doc + test updates.

### 1. Zero-Length & Identity Moves

- A request where source == target MUST yield null (no implicit “wait” move). Such waiting is modeled only via rule-level state events, never movement patterns.
- Fast-path MUST early skip (zero-length) without incrementing hit counters.

### 2. Multiple Candidate Rays (MultiDirection Tie-Breaking)

- When multiple rays (from a `MultiDirectionPattern`) reach the same target with identical length, the FIRST discovered ray (pattern direction enumeration order) wins.
- Discovery order is deterministic: underlying `Directions` array order captured at construction → compiler preserves order → resolver iterates sequentially.
- No secondary heuristics (e.g., prefer orthogonal vs diagonal). Any future heuristic requires new flag + parity tests.

### 3. Shortest Path Preference

- If both a multi-step ray and a fixed pattern can reach the same target, the resolver reports whichever pattern succeeds FIRST in iteration order; there is no global shortest-path search across heterogeneous pattern types in this phase.
- Rationale: Avoid implicit prioritization that could silently change module semantics when pattern lists are reordered. Game designers control precedence by ordering the piece’s pattern collection.

### 4. Fixed Pattern Partial Progress Prohibition

- Fixed patterns require all constituent steps to exist; partial segments MUST NOT yield a shorter path. (Guard: resolver aborts on first missing relation and never emits partial path.)
- Post-filter MUST still validate occupancy even if the final step fails (no intermediate capture rescue allowed).

### 5. Repeatable Rays and Early Termination

- Repeatable (`IsRepeatable=true`) rays scan until: (a) target found, (b) relation missing, or (c) occupancy blocker encountered.
- Scanning stops immediately at first blocker (friendly or enemy). Enemy at target allowed as capture; enemy before target halts with null.
- Rays MUST NOT continue past an enemy capture square (no chained captures / no pass-through).

### 6. Mixed Blocker Ordering (Multi-Block Scenarios)

| Scenario | Example Sequence (S→ ... →T) | Outcome | Rationale |
|----------|-------------------------------|---------|-----------|
| Friendly before enemy target | S F E==T | Null | Friendly blocker prohibits reaching later enemy |
| Enemy before friendly target | S E F==T | Null | First blocker (enemy) not at target; cannot pass |
| Enemy at target (no earlier blockers) | S .. E==T | Capture | Standard capture semantics |
| Two enemies, first before target | S E1 E2==T | Null | Cannot jump over E1 |
| Friendly at target (no earlier blockers) | S .. F==T | Null | No friendly capture |

F = Friendly piece, E = Enemy piece. Only first occupied tile matters.

### 7. Non-Sliding Multi-Step Patterns vs Rays

- A multi-step non-repeatable `DirectionPattern` (IsRepeatable=false) requiring length > 1 is NOT currently expressible; longer sequences require `FixedPattern`. If introduced later, semantics must mirror Fixed (all-or-nothing) with early occupancy checks.

### 8. AnyPattern & NullPattern Handling

- `AnyPattern`: Not compiled. Visitor interpretation currently results in exhaustive attempt via other semantics; compiled resolver skips to avoid ambiguous explosion. Treated as producing no direct geometric path itself (null) — rule authors must supply concrete patterns for movement.
- `NullPattern`: Explicit no-op; returns null in all resolvers. Used to anchor placeholder pieces (e.g., blockers) or semantic “non-movers.”
- Future introduction of wildcard expansion requires a design addendum describing search boundaries + performance constraints.

### 9. Post-Filter Canonical Scan

- Occupancy filtering scans path steps in order; the first occupied tile determines result. Implementation MUST avoid branching that depends on hash codes, iteration order of unordered sets, or randomization.
- Path objects are immutable; post-filter returns original path instance (for legal move) or null; it never mutates internal relation collections.

### 10. Fast-Path Reconstruction Failure Modes

- If geometric attack set includes target but reconstruction via neighbor chaining fails (missing relation due to inconsistent board), fast-path MUST gracefully fall back (return null) allowing compiled/legacy to attempt resolution. Such a mismatch is a test failure candidate (board topology invariant) but must not throw at runtime.

### 11. Determinism Invariants (Expanded List)

Same (GameState snapshot hash, FeatureFlags bitmap, Piece.Id, From.Id, To.Id) implies:

1. Same decision: fast-path skipped vs attempted.
2. Same resulting path nullability & relation sequence (ids).
3. Same capture classification (friendly vs enemy) when applicable.
4. Metrics counters may differ only in aggregate proportions but per-call outcome classification (hit/skip category) is identical.

### 12. Illegal Jump Prevention

- No pattern currently allows jumping over blockers (e.g., knights are modeled via discrete `FixedPattern` L shape with no intermediate occupancy inspection). Introducing a “leaper” modifier must specify whether intermediate tiles are ignored (knight-style) or conditionally examined.

### 13. Piece Pattern Ordering as Precedence Contract

- Designer-provided pattern list order is the ONLY precedence mechanism. The engine will not reorder, sort, or deduplicate. Duplicate patterns are allowed and evaluated in list order (may be optimized later with grouping if provably semantics-neutral).

### 14. Error Handling & Exceptions

- Path resolution NEVER throws for absence of connectivity; it returns null.
- Only board construction / pattern construction may throw (invalid inputs). Runtime resolution invariants (e.g., broken topology) should be surfaced via explicit tests rather than exceptions in hot paths.

### 15. Future Extension Hooks

- Hooks for conditional moves (context-dependent legality) should layer ABOVE geometric resolution (rule phase) rather than adding stateful conditions into pattern evaluation. This preserves purity.

---

Charter Edge-Case Section: Initial version 2025-09-25. Update this date and enumerated sections if semantics evolve.

Last updated: 2025-09-25
