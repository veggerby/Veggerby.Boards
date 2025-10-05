# Turn Sequencing (Graduated)

Feature flag: `EnableTurnSequencing` (default: ON). Provides deterministic TurnState (TurnNumber, Segment, PassStreak, optional replay) and explicit events for advancing, passing, committing, and replaying turns. Active player rotation is handled via a centralized helper; modules should use `GetActivePlayer` to consult the authoritative projection, which reflects TurnState-driven rotation.

## Segments (Current Minimal Profile)

Start → Main → End. Advancing from End increments TurnNumber and rotates the active player. Future profiles will allow custom segment sets.

## Events & Mutators (Summary)

| Event | Mutator | Effect |
|-------|---------|--------|
| EndTurnSegmentEvent | TurnAdvanceStateMutator | Advance segment or increment turn (End→Start + rotation) |
| TurnPassEvent | TurnPassStateMutator | Increment turn, rotate player, increment PassStreak |
| TurnCommitEvent | TurnCommitStateMutator | Main→End shortcut (no turn increment) |
| TurnReplayEvent | TurnReplayStateMutator | Extra turn (same player), increment TurnNumber, reset streak |

## Invariants

1. TurnNumber strictly monotonic.
2. No implicit transitions—only explicit events mutate TurnState.
3. PassStreak increments only on TurnPassEvent (resets on advancement / replay / commit→end completion).
4. Replay keeps ActivePlayer constant.

## Metrics

When enabled, playout metrics add: PassEvents, ReplayEvents, TurnAdvancements, AverageTurnLength.

## Migration Path

Flag OFF: no TurnState artifact; legacy active player projection remains.
Flag ON: TurnState emitted; active player is derived via the centralized projection helper (reflecting TurnState-driven rotation); existing rules continue to work via centralized helpers.

## Future

Custom segment profiles, ordering strategies, simultaneous commit envelope. See Roadmap.

## Sequencing note

- TurnState is immutable and included in hashing when state hashing is enabled; its presence explains expected hash deltas when comparing flag OFF vs ON, even if piece placements are identical.
- Active player rotation occurs on End→Start advancement and on pass. Replay increments the turn while keeping the same active player.
- Modules should avoid direct scans for ActivePlayerState. Use GameStateExtensions.GetActivePlayer to respect the centralized projection.
- Prefer GameStateExtensions.TryGetActivePlayer(out Player) in conditions and gates where the absence of an active player should lead to Ignore/NotApplicable rather than throwing. Use GetActivePlayer() in strict flows and mutators that require exactly one active player and should fail fast otherwise.
