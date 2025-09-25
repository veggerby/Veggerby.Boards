# Unified Turn & Round Sequencing Design

Status: Draft (2025-09-25)
Feature Flag: `EnableTurnSequencing`
Owners: Core Engine Architecture

## 1. Motivation

Current games (Chess, Backgammon) model active player and limited phase gating without a first-class, extensible notion of turns, rounds, segments, passes, or simultaneous resolution. Upcoming games (Go, Risk, Ludo, Kalaha/Mancala, Settlers of Catan, auction / bidding styles) require richer temporal semantics:

- Backgammon doubling cube needs authoritative “same vs next turn” boundaries.
- Go scoring & termination rely on consecutive passes.
- Risk & Catan split a turn into structured segments (Reinforcement / Roll / Trade / Build / Fortify / End, etc.).
- Ludo / Kalaha grant replay (extra turn) conditions.
- Auction / simultaneous order systems need provisional commitments resolved deterministically.

Ad‑hoc inference (e.g., inspecting specialized cube state or absence of player rotation) is brittle and undermines determinism. A dedicated immutable TurnState chain introduces explicit, testable, and replayable temporal facts.

## 2. Goals & Non‑Goals

### Goals

- Deterministic, immutable representation of progression (TurnNumber, RoundNumber, Segment, etc.).
- Pluggable turn ordering strategies (fixed, dynamic insertion/removal, priority, score-sorted).
- Configurable segment pipelines per game (TurnProfile) without polluting core with game-specific branching.
- Explicit events / mutators for advancing turns & segments (no implicit fallthrough).
- Clean gating for rules (e.g., doubling only allowed in Roll segment at start of opponent's turn).
- Simulation & metrics enrichment (turn length, pass counts, replay counts) with negligible hot-path overhead.
- Backward compatibility via feature flag until parity validated.

### Non‑Goals (Initial)

- Real-time clocks / time controls.
- Partial information commitment protocols beyond single batch simultaneous commit.
- Multi-lane (team parallel) turn execution (deferred).

## 3. Core Artifacts & States

| Element | Type | Purpose |
|---------|------|---------|
| `TurnArtifact` | Artifact | Anchor identity for TurnState chain (one per Game). |
| `TurnState` | IArtifactState | Snapshot of temporal progression. |
| `TurnProfile` | Config object | Defines allowed segments and transitions per game. |
| `TurnSegment` | Enum | { Upkeep, Roll, Main, Commit, Resolution, End } (extensible). |
| `TurnAdvanceStateMutator` | Mutator | Advances to next turn (increments TurnNumber / RoundNumber, rotates ActivePlayer). |
| `BeginSegmentStateMutator` | Mutator | Switches to next configured segment. |
| `EndSegmentStateMutator` | Mutator | Validates completion & transitions forward (or finalizes turn). |
| `TurnPassEvent` | Event | Player explicitly declines action (e.g., Go pass, bidding pass). |
| `TurnCommitEvent` | Event | Ends simultaneous commit collection phase (Commit → Resolution). |
| `TurnOrderingStrategy` | Strategy | Provides next player ordering abstraction. |

### TurnState Fields (Tentative)

- `int TurnNumber` (1-based monotonic)
- `int RoundNumber` (increments after last eligible player in cycle)
- `Player ActivePlayer`
- `TurnSegment Segment`
- `int SegmentOrdinal` (count of times current segment loop executed; e.g., multiple Attack cycles in Risk)
- `int PassCount` (consecutive pass tracker; resets on non-pass action except in games needing cumulative count)
- `bool IsReplay` (flag that this turn is an extra/replay following rule-driven condition)
- `Player[] ProvisionalActors` (populated only in simultaneous commit collection)
- `int LastActionEventIndex` (debug / replay anchor; optional)
- `int LastDoubledTurn` (Backgammon – optionally stays on DoublingDiceState instead, referencing TurnNumber)

## 4. Invariants

1. Immutability: Every change creates a new TurnState – no mutation in place.
2. Uniqueness: At most one TurnState per TurnNumber & SegmentOrdinal pair in the chain history.
3. Determinism: Same prior state + identical ordered events yields identical TurnState sequence.
4. Single active player except during Commit collection (when ActivePlayer may be null and ProvisionalActors non-empty).
5. Segment transitions must match TurnProfile; illegal transitions → Invalid event.
6. No redouble allowed in Backgammon when `currentTurn.TurnNumber == lastDoubledTurn`.
7. Replay turns (e.g., extra Ludo roll) increment TurnNumber but maintain same ActivePlayer; RoundNumber unaffected until rotation.
8. RoundNumber increments only after the final eligible player completes End segment (or Main if simplified profile excludes End).

## 5. TurnProfile DSL (Concept Sketch)

```csharp
var profile = TurnProfile.Builder()
    .Segments(TurnSegment.Upkeep, TurnSegment.Roll, TurnSegment.Main, TurnSegment.End)
    .AllowReplay()                 // enables IsReplay turns
    .EnableSimultaneousCommit(commitSegment: TurnSegment.Commit, resolutionSegment: TurnSegment.Resolution)
    .Build();
```

Profile Metadata:

- Segment list (ordered)
- Optional simultaneous commit pairing
- Replay enabled flag
- Segment-specific hooks (pre / post mutator injection lists) – minimal at start to avoid complexity.

## 6. Ordering Strategies

Interface: `ITurnOrderingStrategy` with method `Player GetNextPlayer(GameState state, IReadOnlyList<Player> allPlayers)`.

Implementations:

- `FixedOrderTurnOrderingStrategy` (round-robin stable order)
- `DynamicRemovalTurnOrderingStrategy` (skips eliminated players)
- `PriorityTurnOrderingStrategy` (player queue managed externally via state, e.g., initiative systems)
- Future: `ScoreSortedTurnOrderingStrategy` (re-sorts at round boundaries)

## 7. Events & Mutators

| Event / Mutator | Responsibility |
|-----------------|----------------|
| `BeginTurnSegmentEvent` | Request transition to next segment. |
| `EndTurnSegmentEvent` | Validate closure, maybe auto-invoke TurnAdvance if at final segment. |
| `TurnPassEvent` | Increment PassCount; check termination in Go (two consecutive passes). |
| `TurnCommitEvent` | Close Commit segment; advance to Resolution. |
| `TurnAdvanceStateMutator` | Create next TurnState (compute next ActivePlayer, increment counters). |
| `TurnSegmentStateMutator` | Update Segment / SegmentOrdinal. |
| `ReplayTurnStateMutator` (optional) | Mark next turn as replay without rotating player. |

## 8. Backgammon Integration Example

Before:

- Doubling cube redouble guard heuristically inspects specialized cube state.

After:

- Condition: `Segment == TurnSegment.Roll` AND `TurnNumber > LastDoubledTurn`.
- Mutator sets `DoublingDiceState.LastDoubledTurn = currentTurn.TurnNumber`.
- Immediate second attempt in same turn returns Ignore; multi-turn (2→4→8) tests explicit.

## 9. Chess Integration Example

- Profile: Segments(Main) only.
- TurnAdvanceStateMutator rotates player every turn; RoundNumber increments after Black (2 players) completes Main.
- Fifty-move or repetition detectors can leverage TurnNumber directly.

## 10. Go Integration Example

- Profile: Segments(Main) only.
- Pass captured by `TurnPassEvent`; consecutive passes detected by tracking previous TurnState.PassCount.
- Game termination rule triggers scoring phase (either separate Resolution segment if desired or custom end condition).

## 11. Simultaneous Commit (Auction / Hidden Orders)

Flow:

1. Segment set to Commit; players submit provisional events (validated, applied producing per-player sub-state or queued intent objects – design TBD but must remain deterministic).
2. `TurnCommitEvent` closes collection; `Resolution` segment applies aggregated results via deterministic ordering (e.g., player order, numeric bid value tie-breakers, hash stable).
3. Advance to next turn.

Initial simplification: store provisional intents in a TurnCommitState (list of (Player, IntentPayload hash)) to avoid embedding large mutable structures.

## 12. Simulation & Metrics

Extend `PlayoutMetrics`:

- `TotalTurns`, `AverageTurnLength`, `Passes`, `ReplayTurns`, `SegmentCounts[segment]`.
- Deterministic metrics derivable solely from TurnState chain & Event count.

Benchmarks: Add `TurnSequencingOverheadBenchmark` comparing HandleEvent p50/p95 and allocations with flag OFF vs ON (empty segments case) – target overhead <3% p50, zero extra allocations in normal move path.

## 13. Hashing & Replay

- When flag ON: incorporate TurnNumber, ActivePlayer.Id, Segment, RoundNumber, PassCount, IsReplay into 128-bit hash input buffer.
- Ensure disabling feature flag yields identical hashes to legacy path (Backwards compatibility test suite).
- Provide migration doc section explaining expected hash evolution when feature becomes default.

## 14. Feature Flag Phases

| Phase | Description | Tests |
|-------|-------------|-------|
| 0 | Shadow TurnState emission (not consulted by rules). | Parity: Active player equivalence. |
| 1 | Backgammon adopts TurnState for doubling + rotation. | Doubling invariants updated. |
| 2 | Remove legacy ActivePlayerState writes (derive if needed). | Legacy removal tests. |
| 3 | Introduce segment splitting for Backgammon (Roll vs Move). | Segment gating tests. |
| 4 | Add Go pass / termination; Ludo replay semantics. | Pass & replay tests. |
| 5 | Simultaneous commit prototype. | Commit / resolution ordering tests. |

## 15. Tasks (Condensed)

- TurnArtifact + TurnState implementation.
- TurnSegment enum + TurnProfile builder DSL.
- TurnAdvanceStateMutator & Segment mutators.
- Flag scaffolding + shadow emission (Phase 0).
- Backgammon doubling refactor (Phase 1).
- Removal of legacy active player path (Phase 2).
- Segment gating for Backgammon (Phase 3).
- Go pass logic (Phase 4).
- Replay semantics (Ludo / Kalaha) (Phase 4).
- Simultaneous commit (Phase 5).
- Metrics & hashing integration.
- Benchmarks & parity tests.
- Documentation updates & migration guide.

## 16. Risks & Mitigations

- Performance regression: isolate logic to boundary mutators; microbenchmark gating.
- Complexity explosion: small initial segment set; DSL validation; no dynamic reflection.
- Hash churn: keep behind flag until stable + provide deterministic diff tooling.
- Incomplete adoption: run shadow mode first to build confidence.

## 17. Open Questions

- Should `LastDoubledTurn` live on `DoublingDiceState` only (referencing TurnNumber) or inside TurnState? (Leaning: keep on cube state; isolates Backgammon domain specifics.)
- Do we need a dedicated RoundAdvance event or implicit detection is sufficient at final player boundary? (Lean implicit with explicit test.)
- Representation of provisional intents: store as lightweight command objects or hashed payload references? (Defer until Phase 5.)

## 18. Exit Criteria

- All phases through 3 implemented & green + performance budget met; phases 4–5 optional before defaulting flag ON.
- Backgammon & Chess parity; multi-turn doubling invariants use TurnState.
- Documentation (`turn-sequencing.md`, backlog, core concepts) updated & CHANGELOG entry.

---

This document will evolve; changes require updating the backlog Workstream 10 tasks and adding a short note to CHANGELOG when semantics materially shift.
