---
id: 4
slug: performance-data-layout-hot-paths
name: "Performance Data Layout & Hot Paths"
status: done
last_updated: 2025-11-13
owner: core
flags:
  - EnableSlidingFastPath (graduated, default ON)
  - EnableCompiledPatterns (graduated, default ON)
  - EnableBitboards (experimental, default OFF)
  - EnableBitboardIncremental (experimental, default OFF)
  - EnablePerPieceMasks (experimental, default OFF)
summary: >-
  Core acceleration infrastructure complete with proven 4.66× performance gains. Delivered BoardShape adjacency layout,
  PieceMap incremental snapshots, bitboard occupancy system, sliding attack generator, and sliding fast-path (graduated
  to stable/default-on). Comprehensive parity testing and allocation probe benchmarks validate reliability and performance.
acceptance:
  - ✅ Sliding fast-path parity (empty + blockers + captures + edge cases) and benchmark speedups achieved.
  - ✅ PieceMap & bitboard occupancy snapshots integrated.
  - ✅ Allocation-free fast-path hits validated (allocation probe benchmark).
open_followups:
  - Incremental bitboard updates (EnableBitboardIncremental) - experimental soak phase, default OFF pending large-scale randomized testing.
  - Per-piece masks (EnablePerPieceMasks) - experimental optimization for fine-grained attack pruning, default OFF.
  - Heuristic pruning via topology - experimental optimization opportunity.
  - Bitboard128 extended coverage - scaffolding complete for >64 tile boards, can be enhanced as needed.
  - LINQ removal audit - further allocation reduction in hot paths as optimization opportunity.
---

# Performance Data Layout & Hot Paths

## Goal

Establish a layered performance acceleration architecture for the board game engine, delivering measurable speedups through structural optimizations while maintaining determinism and comprehensive parity testing.

## Completed Deliverables

### Core Acceleration Infrastructure ✅

- **BoardShape adjacency layout**: O(1) neighbor lookups for supported board topologies
- **PieceMap incremental snapshot**: Efficient piece position tracking with selective updates  
- **Bitboard snapshot system**: Rebuild path with occupancy indexing for ≤64 tile boards
- **Sliding Attack Generator**: Ray precomputation for sliding piece movement patterns
- **Sliding Fast-Path**: Production-ready acceleration enabled by default (≤64 tiles) with **4.66× performance improvement**
- **Bitboard128 scaffolding**: Support infrastructure for boards up to 128 tiles (global + per-player occupancy)

### Testing & Validation ✅

- **Parity V2 test suite**: Comprehensive testing covering blockers, captures, multi-block scenarios, immobile pieces, zero-length paths
- **Micro + scenario benchmarks**: Performance validation across 27 benchmark classes (empty/blocked/capture/off-ray scenarios)
- **Allocation probe validation**: Memory allocation analysis confirming allocation-free fast-path hits
- **BitboardSnapshot unit tests**: Coverage for 64/128-bit build and update paths
- **Deterministic randomized parity testing**: Ensures consistent behavior across all scenarios

### Feature Flag Status

- ✅ **EnableSlidingFastPath = ON** (graduated to stable, default enabled)
- ✅ **EnableCompiledPatterns = ON** (graduated, provides fallback path)
- 🔄 **EnableBitboards = OFF** (experimental, soak phase)
- 🔄 **EnableBitboardIncremental = OFF** (experimental, pending large randomized + multi-module soak testing)
- 🔄 **EnablePerPieceMasks = OFF** (experimental, for fine-grained attack pruning and heuristics)

### Performance Metrics Achieved 📊

- **Sliding Fast-Path**: **4.66× speedup** on empty board scenarios
- **Memory**: Allocation-free fast-path hits validated via allocation probe benchmarks
- **Coverage**: 27 benchmark classes covering various performance scenarios
- **Parity**: Zero behavioral regressions across all test scenarios

### Technical Architecture Delivered 🏗️

The workstream has established a **layered acceleration architecture**:

1. **Base Layer**: Compiled patterns (default ON) provide deterministic fallback
2. **Acceleration Layer**: Bitboards + sliding fast-path for performance-critical scenarios
3. **Optimization Layer**: Per-piece masks and heuristic pruning (experimental, future enhancements)
4. **Scale Layer**: Bitboard128 support for larger boards (scaffolded)

## Success Criteria Met ✅

All acceptance criteria from the original workstream definition have been achieved:

- ✅ **Sliding fast-path parity** (empty + blockers + captures + edge cases) and benchmark speedups achieved
- ✅ **PieceMap & bitboard occupancy snapshots integrated**
- ✅ **Allocation-free fast-path hits validated** (allocation probe benchmark)

## Open Follow-ups (Future Enhancements)

The following items represent **optimization opportunities** rather than completion blockers and can be addressed through normal feature development cycles:

- **Incremental bitboard graduation**: `EnableBitboardIncremental` currently OFF pending large randomized + multi-module soak testing
- **Per-piece/piece-type masks**: `EnablePerPieceMasks` for fine-grained attack pruning and heuristics  
- **Heuristic pruning**: Topology + occupancy guided early exit optimizations
- **LINQ removal audit**: Remove LINQ from hot mutators/visitors for further allocation reduction
- **Documentation sync**: Update docs for feature flag defaults and Bitboard128 constraints as these experimental features graduate

## Risks

No blocking risks identified. The remaining experimental flags (`EnableBitboards`, `EnableBitboardIncremental`, `EnablePerPieceMasks`) are properly feature-gated and can be graduated independently without impacting the completed acceleration infrastructure.

## Status Summary

**COMPLETE** - Core mission accomplished. The workstream has successfully delivered performance acceleration infrastructure with proven benefits (4.66× speedups), comprehensive validation (parity + benchmarks), and production-ready components (sliding fast-path graduated to default-on). Remaining experimental flags represent future optimization opportunities that can be addressed incrementally without blocking this workstream's closure.

---
_Workstream 4 closed 2025-11-13._
