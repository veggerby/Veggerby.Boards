 Implementations:

* `FixedOrderTurnOrderingStrategy` (round-robin stable order)
* `DynamicRemovalTurnOrderingStrategy` (skips eliminated players)
* `PriorityTurnOrderingStrategy` (player queue managed externally via state, e.g., initiative systems)
## 15. Tasks (Condensed)

* TurnArtifact + TurnState implementation.
* TurnSegment enum + TurnProfile builder DSL.
* TurnAdvanceStateMutator & Segment mutators.
* Flag scaffolding + shadow emission (Phase 0).
* Backgammon doubling refactor (Phase 1).
* Removal of legacy active player path (Phase 2).
* Segment gating for Backgammon (Phase 3).
* Go pass logic (Phase 4).
* Replay semantics (Ludo / Kalaha) (Phase 4).
* Simultaneous commit (Phase 5).
* Metrics & hashing integration.
* Benchmarks & parity tests.
* Documentation updates & migration guide.
## 16. Risks & Mitigations

* Performance regression: isolate logic to boundary mutators; microbenchmark gating.
* Complexity explosion: small initial segment set; DSL validation; no dynamic reflection.
* Hash churn: keep behind flag until stable + provide deterministic diff tooling.
* Incomplete adoption: run shadow mode first to build confidence.
# Turn Sequencing Model (Experimental – Flag `EnableTurnSequencing`)

Status: Experimental (feature-flag gated). Hash inputs include the `TurnState` artifact when enabled; disabling the flag removes the artifact and may yield different state hashes for otherwise identical piece positions.

## Overview

Turn sequencing provides a deterministic, extensible timeline for games that progress through ordered player turns and optional intra-turn segments. It introduces:

* `TurnArtifact` – singleton identity representing the global turn timeline.
* `TurnState` – immutable snapshot containing:
  * `TurnNumber` (int ≥ 1)
  * `Segment` (`TurnSegment` enum: Start, Main, End)
  * `PassStreak` (int ≥ 0, consecutive passes count)
* Events & Mutators: `EndTurnSegmentEvent` / `TurnAdvanceStateMutator`, `TurnPassEvent` / `TurnPassStateMutator`, `TurnCommitEvent` / `TurnCommitStateMutator`, `TurnReplayEvent` / `TurnReplayStateMutator`.

Determinism Principle: Same initial state + identical ordered event list + same feature flag configuration ⇒ identical `TurnState` chain and resulting game state.

## Segments

Current minimal profile: Start → Main → End.

* Start: Optional prelude (e.g., upkeep, roll) – currently structural placeholder.
* Main: Primary action window (moves, plays, placements).
* End: Terminal segment; advancing from End increments `TurnNumber` and resets segment to Start.

Future profiles (not yet implemented) will allow custom segment sets (e.g., Roll, Commit, Resolution) via a `TurnProfile` DSL.

## Mutators & Semantics

| Event | Mutator | Preconditions | Effects | Notes |
|-------|---------|---------------|---------|-------|
| `EndTurnSegmentEvent` | `TurnAdvanceStateMutator` | Sequencing enabled | If Segment != End: advance to next segment; if Segment == End: increment `TurnNumber`, reset `Segment=Start`, rotate active player (legacy projection), reset `PassStreak` | Active player rotation temporary until TurnState becomes authoritative |
| `TurnPassEvent` | `TurnPassStateMutator` | Sequencing enabled | Increment `TurnNumber`, reset `Segment=Start`, rotate active player, increment `PassStreak` (since a pass ended previous turn) | Pass counts accumulate consecutively (e.g., Go two-pass termination) |
| `TurnCommitEvent` | `TurnCommitStateMutator` | Sequencing enabled & `Segment==Main` | Transition to `Segment=End` (no `TurnNumber` change, no rotation, `PassStreak` unchanged) | Provides explicit Main→End shortcut |
| `TurnReplayEvent` | `TurnReplayStateMutator` | Sequencing enabled & existing `TurnState` present | Increment `TurnNumber`, reset `Segment=Start`, do NOT rotate active player, reset `PassStreak` | Models earned extra turns (Ludo / Mancala) |

`PassStreak` Handling:

* Incremented only on `TurnPassEvent`.
* Reset when a turn advances naturally (End→Start), on replay, or on commit→end followed by advancement.
* Used for future end-of-game detection (e.g., Go: two consecutive passes).

## Metrics Integration

`PlayoutMetrics` additions (when sequencing enabled during simulation):

* `PassEvents`
* `ReplayEvents`
* `TurnAdvancements` (count of End→Start progressions OR any mutator that increments `TurnNumber`)
* `AverageTurnLength` (AppliedEvents / TurnAdvancements; zero if no advancements)

Collection Strategy: `SequentialSimulator.RunDetailed` captures `TurnState` before and after each applied event; if `TurnNumber` changed ⇒ increment `TurnAdvancements`. Event type tests classify pass/replay counters.

## Feature Flag Behavior

Flag OFF: No `TurnArtifact` / `TurnState` emitted. Sequencing mutators become inert (return original state). Existing active player logic (legacy) remains unaffected.

Flag ON: `TurnArtifact` + initial `TurnState(TurnNumber=1, Segment=Start, PassStreak=0)` emitted at compile time. Mutators evaluate and produce new `TurnState` snapshots.

Parity Considerations: Piece positions and other artifact states must remain identical with flag ON or OFF for identical non-turn events. State hashes may diverge (presence/absence of `TurnState`).

## Invariants

1. `TurnNumber` strictly monotonic increasing by exactly 1 per advancement/replay/pass.
2. Single active player at a time (rotation currently handled in advancement/pass mutators; future: extracted to ordering strategy).
3. No implicit segment transitions—only explicit events cause mutation.
4. `PassStreak` never negative; increments only on pass, resets on replay or natural advancement.
5. Mutators are pure: same input state + same event ⇒ identical output state.

## Hashing Impact

`TurnState` participates in canonical serialization ordering when present, affecting both 64-bit and 128-bit hashes. `TurnSequencingHashParityTests` asserts piece state parity while tolerating hash divergence.

## Future Roadmap

* Turn ordering strategies (fixed, dynamic, priority queue).
* Custom segment profiles (Roll, Commit, Resolution, Upkeep, Attack loops).
* Simultaneous commit envelope (collect provisional sub-turn states, resolve deterministically).
* Consecutive pass termination rule (Go) building on `PassStreak`.
* Replay granting rules (dice roll = 6, landing in store) via game-specific rules emitting `TurnReplayEvent`.

## Style Charter Compliance

Sequencing code (events + mutators + metrics) adheres to repository style:

1. File-scoped namespaces
2. Explicit braces for all control flow
3. No LINQ inside mutator hot paths or simulator inner loop
4. Immutable state transitions (always produce new `GameState` via `state.Next([...])`)
5. Deterministic ordering (artifact enumeration stable; no randomness outside `GameState.Random`)

Any required deviation MUST include `// STYLE-DEVIATION:` and a CHANGELOG entry.

## Testing

Existing tests cover:

* Segment transitions & advancement.
* Pass streak accumulation (two sequential passes).
* Replay semantics (extra turn without rotation; streak reset).
* Flag-off absence of `TurnState` (scope tests).
* Hash parity test (piece position parity vs hash divergence tolerance).

Planned tests (pending future work): ordering strategy integration, multi-segment profiles, consecutive pass termination rule, replay rule injection from game modules.

## Unified Turn & Round Sequencing Design (Draft Extension)

Status: Draft (2025-09-25)
Feature Flag: `EnableTurnSequencing`
Owners: Core Engine Architecture

## 1. Motivation

Current games (Chess, Backgammon) model active player and limited phase gating without a first-class, extensible notion of turns, rounds, segments, passes, or simultaneous resolution. Upcoming games (Go, Risk, Ludo, Kalaha/Mancala, Settlers of Catan, auction / bidding styles) require richer temporal semantics:

* Backgammon doubling cube needs authoritative “same vs next turn” boundaries.
* Go scoring & termination rely on consecutive passes.
* Risk & Catan split a turn into structured segments (Reinforcement / Roll / Trade / Build / Fortify / End, etc.).
* Ludo / Kalaha grant replay (extra turn) conditions.
* Auction / simultaneous order systems need provisional commitments resolved deterministically.

Ad‑hoc inference (e.g., inspecting specialized cube state or absence of player rotation) is brittle and undermines determinism. A dedicated immutable TurnState chain introduces explicit, testable, and replayable temporal facts.

## 2. Goals & Non‑Goals

### Goals

* Deterministic, immutable representation of progression (TurnNumber, RoundNumber, Segment, etc.).
* Pluggable turn ordering strategies (fixed, dynamic insertion/removal, priority, score-sorted).
* Configurable segment pipelines per game (TurnProfile) without polluting core with game-specific branching.
* Explicit events / mutators for advancing turns & segments (no implicit fallthrough).
* Clean gating for rules (e.g., doubling only allowed in Roll segment at start of opponent's turn).
* Simulation & metrics enrichment (turn length, pass counts, replay counts) with negligible hot-path overhead.
* Backward compatibility via feature flag until parity validated.

### Non‑Goals (Initial)

* Real-time clocks / time controls.
* Partial information commitment protocols beyond single batch simultaneous commit.
* Multi-lane (team parallel) turn execution (deferred).

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

* `int TurnNumber` (1-based monotonic)
* `int RoundNumber` (increments after last eligible player in cycle)
* `Player ActivePlayer`
* `TurnSegment Segment`
* `int SegmentOrdinal` (count of times current segment loop executed; e.g., multiple Attack cycles in Risk)
* `int PassCount` (consecutive pass tracker; resets on non-pass action except in games needing cumulative count)
* `bool IsReplay` (flag that this turn is an extra/replay following rule-driven condition)
* `Player[] ProvisionalActors` (populated only in simultaneous commit collection)
* `int LastActionEventIndex` (debug / replay anchor; optional)
// Removed: `LastDoubledTurn` no longer considered a core TurnState field (lives on `DoublingDiceState`).

## 4. Invariants

1. Immutability: Every change creates a new TurnState – no mutation in place.
2. Uniqueness: At most one TurnState per TurnNumber & SegmentOrdinal pair in the chain history.
3. Determinism: Same prior state + identical ordered events yields identical TurnState sequence.
4. Single active player except during Commit collection (when ActivePlayer may be null and ProvisionalActors non-empty).
5. Segment transitions must match TurnProfile; illegal transitions → Invalid event.
6. No redouble allowed in Backgammon when `currentTurn.TurnNumber == DoublingDiceState.LastDoubledTurn` (gating implemented in cube mutator; TurnState remains domain-agnostic).
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

* Segment-specific hooks (pre / post mutator injection lists) – minimal at start to avoid complexity.

## 6. Ordering Strategies

Interface: `ITurnOrderingStrategy` with method `Player GetNextPlayer(GameState state, IReadOnlyList<Player> allPlayers)`.

Implementations:

- `FixedOrderTurnOrderingStrategy` (round-robin stable order)
- `DynamicRemovalTurnOrderingStrategy` (skips eliminated players)
* `PriorityTurnOrderingStrategy` (player queue managed externally via state, e.g., initiative systems)

* Future: `ScoreSortedTurnOrderingStrategy` (re-sorts at round boundaries)

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

* Doubling cube redouble guard heuristically inspects specialized cube state.

After:

* Condition (future when Roll segment introduced): `Segment == TurnSegment.Roll` AND `TurnNumber > DoublingDiceState.LastDoubledTurn`.
* Mutator sets `DoublingDiceState.LastDoubledTurn = currentTurn.TurnNumber`.

* Immediate second attempt in same turn returns Ignore; multi-turn (2→4→8) tests explicit.

## 9. Chess Integration Example

* Profile: Segments(Main) only.
* TurnAdvanceStateMutator rotates player every turn; RoundNumber increments after Black (2 players) completes Main.

* Fifty-move or repetition detectors can leverage TurnNumber directly.

## 10. Go Integration Example

* Profile: Segments(Main) only.
* Pass captured by `TurnPassEvent`; consecutive passes detected by tracking previous TurnState.PassCount.

* Game termination rule triggers scoring phase (either separate Resolution segment if desired or custom end condition).

## 11. Simultaneous Commit (Auction / Hidden Orders)

Flow:

1. Segment set to Commit; players submit provisional events (validated, applied producing per-player sub-state or queued intent objects – design TBD but must remain deterministic).
2. `TurnCommitEvent` closes collection; `Resolution` segment applies aggregated results via deterministic ordering (e.g., player order, numeric bid value tie-breakers, hash stable).
3. Advance to next turn.

Initial simplification: store provisional intents in a TurnCommitState (list of (Player, IntentPayload hash)) to avoid embedding large mutable structures.

## 12. Simulation & Metrics

Extend `PlayoutMetrics`:

* `TotalTurns`, `AverageTurnLength`, `Passes`, `ReplayTurns`, `SegmentCounts[segment]`.

* Deterministic metrics derivable solely from TurnState chain & Event count.

Benchmarks: Add `TurnSequencingOverheadBenchmark` comparing HandleEvent p50/p95 and allocations with flag OFF vs ON (empty segments case) – target overhead <3% p50, zero extra allocations in normal move path.

## 13. Hashing & Replay

* When flag ON: incorporate TurnNumber, ActivePlayer.Id, Segment, RoundNumber, PassCount, IsReplay into 128-bit hash input buffer.
* Ensure disabling feature flag yields identical hashes to legacy path (Backwards compatibility test suite).

* Provide migration doc section explaining expected hash evolution when feature becomes default.

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
* Backgammon doubling refactor (Phase 1).
* Removal of legacy active player path (Phase 2).
* Segment gating for Backgammon (Phase 3).
* Go pass logic (Phase 4).
* Replay semantics (Ludo / Kalaha) (Phase 4).
* Simultaneous commit (Phase 5).
- Metrics & hashing integration.
- Benchmarks & parity tests.

* Documentation updates & migration guide.

## 16. Risks & Mitigations

- Performance regression: isolate logic to boundary mutators; microbenchmark gating.
* Complexity explosion: small initial segment set; DSL validation; no dynamic reflection.
* Hash churn: keep behind flag until stable + provide deterministic diff tooling.

* Incomplete adoption: run shadow mode first to build confidence.

## 17. Open Questions

* Resolved: `LastDoubledTurn` lives on `DoublingDiceState` (kept out of core TurnState to avoid Backgammon-specific leakage).
* Do we need a dedicated RoundAdvance event or implicit detection is sufficient at final player boundary? (Lean implicit with explicit test.)

* Representation of provisional intents: store as lightweight command objects or hashed payload references? (Defer until Phase 5.)

## 18. Exit Criteria

* All phases through 3 implemented & green + performance budget met; phases 4–5 optional before defaulting flag ON.
* Backgammon & Chess parity; multi-turn doubling invariants use TurnState.
* Documentation (`turn-sequencing.md`, backlog, core concepts) updated & CHANGELOG entry.

---

This document will evolve; changes require updating the backlog Workstream 10 tasks and adding a short note to CHANGELOG when semantics materially shift.
