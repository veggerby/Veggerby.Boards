# Core Engine Capability Backlog

This document enumerates missing or under-served core engine capabilities for Veggerby.Boards. Each item includes intent, purpose, abstract acceptance criteria, and impact.

## 1) Player Views / Hidden Information

**Impact:** Very High

**Intent**
Provide a first-class, deterministic projection layer so each player (or observer role) can view the same underlying game state with masked or redacted information.

**Purpose**
Enable imperfect-information games (cards, fog-of-war, hidden hands) without duplicating logic across modules or leaking information in UI, AI, or replay outputs.

**Acceptance Criteria (abstract)**
- Core exposes a stable API for creating a view of `GameState` scoped to a player/role.
- Projection is deterministic and does not mutate underlying state.
- Hidden data cannot be accessed through the projected view without explicit capability.
- Supports at least: full visibility, per-player visibility, and observer visibility.

## 2) Legal Move Generation API

**Impact:** Very High

**Intent**
Provide a standard, core-level way to enumerate legal `IGameEvent` candidates and explain rejections.

**Purpose**
Support AI, tooling, UI hinting, and validation consistency across modules without ad hoc move generation logic.

**Acceptance Criteria (abstract)**
- Core defines a contract for legal move enumeration that integrates with phases/rules.
- Produces deterministic candidate ordering or explicitly documents ordering guarantees.
- Provides structured diagnostics for illegal events beyond a generic rejection.
- Can be used without requiring module-specific reflection or internal access.

## 3) Public Save/Load + Replay Format

**Impact:** High

**Intent**
Define and implement a stable serialization format for `GameState` plus event history suitable for persistence and replay.

**Purpose**
Enable long-lived saves, sharing, deterministic replays, and reproducible bug reports across versions.

**Acceptance Criteria (abstract)**
- A documented schema exists for a save/replay envelope (metadata + state + events).
- Format supports versioning and forward compatibility guidance.
- Serialization and deserialization are deterministic and validated via hashes.
- Can round-trip a game without loss of information needed for replay.

## 4) History/Undo Integration

**Impact:** High

**Intent**
Make undo/redo and branching timelines a first-class part of `GameProgress` or an official companion type.

**Purpose**
Enable UI navigation, analysis tools, and scenario exploration without bespoke state bookkeeping.

**Acceptance Criteria (abstract)**
- Core offers an official API to step backward/forward through state history.
- Undo/redo preserves determinism and does not mutate existing states.
- Branching after undo is supported and explicitly specified.
- Integrates cleanly with event history and hashing.

## 5) Rule Priority & Conflict Resolution

**Impact:** High

**Intent**
Allow explicit rule priorities or resolution strategies beyond declaration order.

**Purpose**
Reduce accidental rule conflicts, make complex flows more maintainable, and permit modular rule composition.

**Acceptance Criteria (abstract)**
- Rules can declare a priority or strategy identifier.
- The engine resolves competing rules deterministically and transparently.
- Strategy can be chosen per phase or per game.
- Observability includes which rule won and why.

## 6) Simultaneous Turns / Secret Commit

**Impact:** Medium-High

**Intent**
Support simultaneous-action games (commit/reveal, hidden selection, parallel actions) in core.

**Purpose**
Unlock a broader set of game types without custom synchronization layers per module.

**Acceptance Criteria (abstract)**
- Core provides a synchronization primitive (commit, reveal, barrier) usable in phases/rules.
- Events can be staged and resolved deterministically as a set.
- Works with hidden information and projections without leaking.
- Clear failure modes for missing or invalid commitments.

## 7) Outcome/Scoring Framework

**Impact:** Medium

**Intent**
Offer reusable scoring and victory condition helpers beyond module-specific implementations.

**Purpose**
Reduce duplication and provide consistent result structures for multi-player games.

**Acceptance Criteria (abstract)**
- Core defines a standard scoring contract and result shape.
- Supports win/loss/draw and ranked outcomes.
- Can be integrated into endgame detection without special casing.

## 8) Turn Clocks / Time Controls

**Impact:** Medium

**Intent**
Add core support for time-limited turns and time-based losses or penalties.

**Purpose**
Enable tournament-style play, AI limits, and real-time constraints without ad hoc time tracking.

**Acceptance Criteria (abstract)**
- Core defines clock artifacts and state transitions for time accounting.
- Rules can enforce time-based termination or penalties deterministically.
- Time modeling is explicit and testable (no hidden wall-clock dependency).

