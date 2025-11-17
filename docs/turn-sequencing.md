# Turn Sequencing (Graduated)

**Status**: Default-enabled, fully integrated with Chess, Backgammon, and Go modules.

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

## Lifecycle Guide

### 1. Initialization

When a game is compiled with turn sequencing enabled, a `TurnState` artifact is automatically created and initialized:

- **TurnNumber**: 1 (1-based indexing)
- **Segment**: Start
- **PassStreak**: 0
- **Active Player**: First player in the player list is marked active via `ActivePlayerState`

### 2. Segment Flow (Standard Turn)

A complete turn follows the segment progression:

1. **Start** → (optional phase-specific logic)
2. **Main** → (core gameplay actions: moves, placements, etc.)
3. **End** → (cleanup, scoring updates, etc.)

Advancing from **End** automatically:
- Increments `TurnNumber`
- Resets segment to **Start**
- Rotates to the next player via `TurnSequencingHelpers.ApplyTurnAndRotate`

### 3. Turn Advancement Variants

#### Standard Advancement (Segment Progression)
```csharp
// Emit EndTurnSegmentEvent to advance through segments
progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
// TurnState now: Segment=Main, TurnNumber unchanged

progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));
// TurnState now: Segment=End, TurnNumber unchanged

progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.End));
// TurnState now: Segment=Start, TurnNumber+1, active player rotated
```

#### Pass (Immediate Turn Skip)
```csharp
// Emit TurnPassEvent to skip to next turn immediately
progress = progress.HandleEvent(new TurnPassEvent());
// TurnState now: TurnNumber+1, Segment=Start, PassStreak+1, player rotated
```

**Use Case**: Games like Go where players can pass their turn entirely.

#### Commit (Early End Transition)
```csharp
// Emit TurnCommitEvent to jump from Main to End without incrementing turn
progress = progress.HandleEvent(new TurnCommitEvent());
// TurnState now: Segment=End, TurnNumber unchanged, PassStreak reset
```

**Use Case**: Player explicitly ends their turn early (e.g., in deck-building games).

#### Replay (Extra Turn)
```csharp
// Emit TurnReplayEvent to grant an extra turn without rotation
progress = progress.HandleEvent(new TurnReplayEvent());
// TurnState now: TurnNumber+1, Segment=Start, PassStreak=0, same active player
```

**Use Case**: Roll-again mechanics (Monopoly doubles), bonus turn cards.

### 4. PassStreak Management

The `PassStreak` counter tracks consecutive passes for games requiring termination after N consecutive passes (e.g., Go's two-pass rule).

- **Incremented**: On `TurnPassEvent`
- **Reset**: On standard advancement, commit, replay, or game-specific actions (e.g., stone placement in Go)

**Example (Go Integration)**:
```csharp
// Go's PassTurnStateMutator increments PassStreak and checks for termination
public GameState MutateState(GameEngine engine, GameState gameState, PassTurnGameEvent @event)
{
    // ... increment TurnState.PassStreak
    if (newPassStreak >= 2)
    {
        return updatedState.Next([new GameEndedState()]);
    }
    return updatedState;
}

// Go's PlaceStoneStateMutator resets PassStreak on stone placement
if (turnState != null && turnState.PassStreak > 0)
{
    var resetTurnState = new TurnState(turnState.Artifact, turnState.TurnNumber, turnState.Segment, 0);
    return stateWithExtras.Next([resetTurnState]);
}
```

### 5. Active Player Projection

All modules should use the centralized active player projection helpers:

#### In Conditions and Gates (Non-Throwing)
```csharp
if (!state.TryGetActivePlayer(out var activePlayer))
{
    return ConditionResponse.NotApplicable();
}
// Use activePlayer safely
```

#### In Mutators (Strict Enforcement)
```csharp
var activePlayer = gameState.GetActivePlayer(); // Throws if not exactly one active player
// Proceed with guaranteed active player
```

**Rationale**: The centralized helpers (`GameStateExtensions.GetActivePlayer` / `TryGetActivePlayer`) reflect turn sequencing-driven rotation, ensuring consistent behavior across all modules.

## Module Integration Examples

### Chess Integration

Chess uses `NextPlayerStateMutator` (legacy but valid) for player rotation after each move:

```csharp
AddGamePhase("play")
    .ForEvent<ChessMoveEvent>()
        .Then()
            .Do<ChessMovePieceStateMutator>()
            .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()));
```

This approach is compatible with turn sequencing: `NextPlayerStateMutator` uses `GetActivePlayer()` to read the centralized projection.

### Go Integration

Go integrates turn sequencing for two-pass termination:

```csharp
AddGamePhase("play")
    .ForEvent<PlaceStoneGameEvent>()
        .Then()
            .Do<PlaceStoneStateMutator>()  // Resets PassStreak if > 0
    .ForEvent<PassTurnGameEvent>()
        .Then()
            .Do<PassTurnStateMutator>();   // Increments PassStreak, checks for game end
```

**Key Points**:
- `PassTurnStateMutator` increments `TurnState.PassStreak`
- Game ends when `PassStreak >= 2`
- `PlaceStoneStateMutator` resets `PassStreak` to break the streak

### Ludo / Monopoly Pattern (Future)

For roll-based track games with conditional replay:

```csharp
AddGamePhase("roll-and-move")
    .ForEvent<RollDiceEvent>()
        .Then()
            .Do<DiceStateMutator>()
    .ForEvent<MoveTokenEvent>()
        .Then()
            .Do<TokenMovementStateMutator>()
            .If<DoubleRollCondition>()
                .Do<TurnReplayStateMutator>()  // Extra turn on doubles
            .Else()
                .Do<TurnAdvanceStateMutator>(); // Normal advancement
```

## Invariants

1. **TurnNumber strictly monotonic**: Never decreases (only increments on pass, replay, or End→Start transition).
2. **No implicit transitions**: Only explicit events mutate TurnState.
3. **PassStreak semantics**: Increments only on TurnPassEvent; resets on advancement, replay, commit→end completion, or module-specific actions.
4. **Replay invariant**: Keeps ActivePlayer constant while incrementing TurnNumber.
5. **Rotation determinism**: Player rotation follows engine player list order; TurnSequencingHelpers ensures circular rotation.

## Metrics

When enabled, playout metrics add: PassEvents, ReplayEvents, TurnAdvancements, AverageTurnLength.

## Migration Path for Custom Modules

### From Legacy Active Player Projection

**Before** (manual scan):
```csharp
Player? activePlayer = null;
foreach (var aps in gameState.GetStates<ActivePlayerState>())
{
    if (aps.IsActive) { activePlayer = aps.Artifact; break; }
}
```

**After** (centralized helper):
```csharp
if (!gameState.TryGetActivePlayer(out var activePlayer))
{
    return ConditionResponse.NotApplicable();
}
```

### From Manual Player Rotation

**Before** (manual circular rotation):
```csharp
var nextPlayer = engine.Game.Players
    .Concat(engine.Game.Players)
    .SkipWhile(x => !x.Equals(activePlayer))
    .Skip(1)
    .First();
var previousProjection = new ActivePlayerState(activePlayer, false);
var nextProjection = new ActivePlayerState(nextPlayer, true);
return gameState.Next([previousProjection, nextProjection]);
```

**After** (use TurnPassEvent or TurnAdvanceStateMutator):
```csharp
// Option 1: Use core turn events (if appropriate for game semantics)
return gameState.Next([new TurnPassEvent()]);

// Option 2: Keep NextPlayerStateMutator (legacy but valid)
// It already uses centralized GetActivePlayer() projection
```

## Future Extensions

- **Custom segment profiles**: Define module-specific segment flows (e.g., Upkeep → Draw → Action → Buy → Cleanup).
- **Ordering strategies**: Non-circular rotation (e.g., bid-based turn order in auctions).
- **Simultaneous commit envelope**: Multi-player concurrent action resolution.

See [Roadmap](ROADMAP.md) for planned enhancements.

## Hash Parity & Determinism

- **TurnState is immutable** and included in state hashing; its presence explains expected hash deltas when comparing sequencing OFF vs ON, even if piece placements are identical.
- **Determinism guarantee**: Given the same event sequence, turn advancement produces identical TurnState snapshots across runs.
- **Hash parity tests** validate that turn sequencing does not introduce non-determinism (see `TurnSequencingHashParityTests`).

## Additional Notes

- Modules should avoid direct scans for `ActivePlayerState`. Use `GameStateExtensions.GetActivePlayer` / `TryGetActivePlayer` to respect the centralized projection.
- Prefer `TryGetActivePlayer(out Player)` in conditions and gates where the absence of an active player should lead to Ignore/NotApplicable rather than throwing.
- Use `GetActivePlayer()` in strict flows and mutators that require exactly one active player and should fail fast otherwise.
- Turn sequencing is compatible with legacy player rotation mechanisms (`NextPlayerStateMutator`) as long as they use the centralized projection helpers.
