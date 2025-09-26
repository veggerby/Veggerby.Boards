# Turn Sequencing (Experimental)

Feature flag: `EnableTurnSequencing`. Provides deterministic TurnState (TurnNumber, Segment, PassStreak, optional replay) and explicit events for advancing, passing, committing, and replaying turns.

## Segments (Current Minimal Profile)

Start → Main → End. Advancing from End increments TurnNumber and rotates active player (legacy projection until fully adopted). Future profiles will allow custom segment sets.

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

Flag OFF: no TurnState artifact; legacy active player remains.
Flag ON: TurnState emitted; existing rules unaffected unless they gate on sequencing events.

## Future

Custom segment profiles, ordering strategies, simultaneous commit envelope. See Roadmap.
