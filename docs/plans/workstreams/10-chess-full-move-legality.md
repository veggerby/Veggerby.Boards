---
id: 10
slug: chess-full-move-legality
name: "Chess Full Move Legality"
status: planned
last_updated: 2025-09-26
owner: chess
summary: >-
  Implement complete chess move legality: state extensions (castling rights, en passant), occupancy-aware generation,
  pseudo-legal move enumeration, king safety filtering, special move events, and full SAN (captures, disambiguation,
  check, mate, castling, promotion, en passant, stalemate) while preserving engine determinism and immutability.
acceptance:
  - Castling rights & en passant target represented in immutable ChessStateExtras.
  - Pseudo-legal generator produces correct counts from starting position (20 moves) with allocation target met.
  - Legality filter rejects all self-check moves (pin / discovered check scenarios covered by tests).
  - Special events (castle, promotion, en passant) implemented as explicit IGameEvent types + mutators.
  - SAN outputs all required symbols including # for mate and e.p. for en passant captures.
  - Comprehensive unit + property tests for edge cases (castling denial, en passant timing, promotion capture, double check, stalemate).
  - Benchmarks recorded (baseline + post-optimization) and documented.
open_followups:
  - Multiple promotion piece selection (UI / API extension) beyond default queen.
  - Draw rule instrumentation (50-move, threefold repetition) outside initial scope.
  - PGN export / ingestion as separate workstream.
---

# Chess Full Move Legality (WS-CHESS-LEG-001)

> Source plan integrated from former `docs/plans/workstreams.md` root draft.

## Goal

Provide a complete, deterministic, testable implementation of chess move legality (pseudo-legal generation + king safety filtering) and full SAN coverage (captures, disambiguation, check, mate, castling, promotion, en passant, stalemate detection) while preserving engine core principles (immutability, explicit events, deterministic transitions).

## Current State Snapshot

- Implemented: basic movement patterns, partial SAN (captures, disambiguation, +, castling geometry), generic move event.
- Missing: occupancy blocking, castling rights, en passant, promotion logic, move generation API, king safety filtering, mate/stalemate detection, special events, complete SAN symbols (#, =Q, e.p.).

## Success Criteria

- Deterministic: Same state + same event => identical result.
- Coverage: Unit tests for every rule branch (valid, invalid, blocked, self-check rejection, special moves).
- Performance: Baseline generation for starting position completes within acceptable allocation budget (benchmark to define; initial target < 1ms on dev machine for pseudo-legal generation).
- Isolation: Chess-specific legality logic confined to `Veggerby.Boards.Chess` namespace (no core leakage).

## Deliverables

1. State extension structure (castling rights, en passant target, half/full move counters).
2. Occupancy-aware path resolution for sliding pieces.
3. Pseudo-legal move generator API.
4. King safety / legality filter.
5. Special events & mutators (castle, en passant, promotion).
6. Enhanced SAN (promotion, mate, en passant, checkmate/stalemate recognition for notation (#)).
7. Comprehensive test suite + property tests for key invariants.
8. Benchmarks for move generation.
9. Documentation (this plan + API notes + invariants).

## Detailed Task Breakdown

| Seq | Task | Description | Output | Risk |
|-----|------|-------------|--------|------|
| 1 | Gap Analysis | Confirm exact invariants vs current code | Design notes | Low |
| 2 | State Extensions | Introduce immutable chess extras struct | `ChessStateExtras` | Medium |
| 3 | Blocking Logic | Visitor aware of occupancy to truncate sliding paths | `OccupiedResolveTilePathPatternVisitor` | Medium |
| 4 | Pseudo-legal Generation | Produce candidate moves ignoring king safety | `ChessMoveGenerator` | High |
| 5 | Legality Filter | Remove moves leaving king in check | `LegalMoveFilter` | High |
| 6 | Special Events | Explicit events & mutators for castle/en passant/promotion | New event types | Medium |
| 7 | SAN Completion | Promotion (=Q), en passant, mate (#), stalemate detection | Updated `ChessNomenclature` | Medium |
| 8 | Tests Expansion | Edge + property tests | Test files & data | High |
| 9 | Performance Pass | Benchmark and micro-optimize | Benchmark results | Medium |
| 10 | Docs Update | Update docs & README references | Doc updates | Low |

## Data Structures

```csharp
[Flags]
public enum CastlingRights
{
    None = 0,
    WhiteKingSide = 1 << 0,
    WhiteQueenSide = 1 << 1,
    BlackKingSide = 1 << 2,
    BlackQueenSide = 1 << 3
}

public sealed record ChessStateExtras(
    CastlingRights Rights,
    string EnPassantTargetTileId,
    int HalfMoveClock,
    int FullMoveNumber
);
```

## Events (Proposed)

```csharp
public sealed class CastleGameEvent : IGameEvent
{
    public Piece King { get; }
    public Piece Rook { get; }
    public TilePath KingPath { get; }
    public TilePath RookPath { get; }
    // ...ctor & validation
}

public sealed class PromotionGameEvent : IGameEvent
{
    public Piece Pawn { get; }
    public TilePath Path { get; }
    public string PromoteToRole { get; } // e.g. "queen"
}

public sealed class EnPassantCaptureGameEvent : IGameEvent
{
    public Piece Pawn { get; }
    public TilePath Path { get; }
    public Tile CapturedPawnTile { get; }
}
```

## Generation Pipeline

1. Collect piece states for side to move.
2. For each piece, run piece-type specific generator.
3. Add special candidates (castling, en passant, promotion transforms).
4. Produce `PseudoMove` records with metadata (capture, special kind, resulting square, promotion role).
5. Legality filter simulates move (fast ephemeral state) and rejects self-check.

### Simplified PseudoMove

```csharp
public enum PseudoMoveKind { Normal, Capture, Castle, EnPassant, Promotion }
public sealed record PseudoMove(Piece Piece, Tile From, Tile To, PseudoMoveKind Kind, string PromotionRole, bool IsCapture);
```

## Invariants

- Castling only if: rights flag set, path squares empty, king not in check, squares passed through not attacked.
- En passant only on immediate reply turn; capturing pawn moves diagonally into empty square; victim pawn removed from its square.
- Promotion when pawn reaches last rank; must replace pawn identity (new piece artifact predetermined or created ahead of time in builder).
- No move may leave own king attacked.

## Testing Matrix (Sampling)

| Scenario | Tests |
|----------|-------|
| Castling rights lost | Move king once; attempt castle -> invalid |
| Castle through check | Attacked intermediate square -> invalid |
| En passant timing | Available only the turn after double push |
| Promotion capture | Pawn captures into last rank -> promotion event |
| Self-check rook pin | Sliding piece pinned cannot move exposing king |
| Double check | Only king moves legal |
| Stalemate | No legal moves & not in check triggers stalemate flag |

## Performance Considerations

- Avoid LINQ inside inner loops of generation/filtering.
- Reuse temporary list buffers per generation cycle.
- Optionally pre-index piece states by tile for O(1) lookups.

## Open Questions (Deferred)

- Multiple promotion choices: need artifact provisioning strategy (pre-build vs dynamic). For now default to queen.
- Draw rules (50-move rule, threefold repetition) intentionally out of scope.
- PGN export: separate future workstream.

## Metrics / Tracking

| Metric | Target | Notes |
|--------|--------|-------|
| Starting position pseudo-legal generation allocations | < 10 KB | Initial goal, measure after v1 |
| Starting position legal move count | 20 (baseline) | Validation sanity check |
| Promotion scenario generation time | < 0.1 ms | Micro benchmark |

## Status Summary

Initial plan authored. No implementation started. Pending scheduling.

---

_End of workstream 10._
