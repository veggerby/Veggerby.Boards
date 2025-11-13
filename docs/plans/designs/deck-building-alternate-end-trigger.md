---
slug: deck-building-alternate-end-trigger
name: "Deck-building Alternate End Trigger"
status: implemented
last_updated: 2025-10-12
owner: games
related_workstreams: [17]
---

# Deck-building Alternate End Trigger (Supply Depletion)

## 1. Context & Motivation
The current deck-building module ends via scoring + explicit `EndGameEvent` after a minimal turn threshold. Classic deck-building games also end when certain supply depletion conditions occur (e.g., key pile empty or N piles empty). This design introduces an *optional* deterministic supply depletion end trigger without changing existing scoring or termination semantics.

## 2. Goals

- Deterministically end the game when either (configurable):
    - A specified number (threshold) of supply piles are empty, OR
    - Any pile from a configured key set becomes empty.
- Preserve existing scoring ordering (ComputeScoresEvent precedes EndGameEvent) and idempotency.
- Zero additional randomness and no mutation of existing state objects.
- Minimal performance overhead (no per-gain full scans unless acceptable O(M) when evaluating EndGameEvent).

## 3. Non-Goals

- Dynamic thresholds that change mid-game.
- Complex boolean composition (no nested AND/OR trees beyond simple either-condition semantics).
- Additional scoring metrics.
- Immediate automatic emission of `ComputeScoresEvent` (scoring remains explicit / phase-driven).

## 4. Configuration API

Provide a builder extension (or method on `DeckBuildingGameBuilder`):

```csharp
builder.WithEndTrigger(emptyPileThreshold: 3, keyPiles: new[]{"province"});
```

Validation:

- `emptyPileThreshold` null or >= 1.
- `keyPiles` distinct, non-empty, all registered in definitions (validated after definitions known; else throw).
- At least one of threshold or keyPiles supplied; otherwise throw (no-op config is invalid).

## 5. Artifact / Options Representation

Immutable artifact attached at build time:

```csharp
public sealed class DeckBuildingEndTriggerOptions : Artifact
{
    public int? EmptyPileThreshold { get; }
    public IReadOnlyList<string> KeyPiles { get; }
    public DeckBuildingEndTriggerOptions(int? emptyPileThreshold, IEnumerable<string> keyPiles) : base("db-end-trigger-options") { ... }
}
```

(Artifact chosen so it can be retrieved via `engine.Game.GetArtifact<T>()` or from state snapshot consistently; simpler than a dedicated state type.)

## 6. Rule Evaluation Changes

Update `EndGameEventCondition` logic:

1. Still ignore if already ended.
2. Still require `ScoreState` presence.
3. Determine if turn threshold OR supply depletion satisfied.

Supply depletion satisfied if either:

- `EmptyPileThreshold` met: count of supply entries with value == 0 >= threshold.
- Any `KeyPiles` entry has value == 0.

If satisfied, allow EndGameEvent regardless of turn number (turn gating effectively ORed with depletion condition).

Pseudo-code snippet:

```csharp
var options = engine.Game.TryGetArtifact<DeckBuildingEndTriggerOptions>();
bool depletionSatisfied = false;
if (options is not null) {
    int emptyCount = 0;
    foreach (var kv in deckState.Supply) {
        if (kv.Value == 0) { emptyCount++; }
    }
    if (options.EmptyPileThreshold.HasValue && emptyCount >= options.EmptyPileThreshold.Value) { depletionSatisfied = true; }
    if (!depletionSatisfied && options.KeyPiles.Count > 0) {
        foreach (var kp in options.KeyPiles) { if (!deckState.Supply.TryGetValue(kp, out var c) || c == 0) { depletionSatisfied = true; break; } }
    }
}
if (!depletionSatisfied && turn is not null && turn.TurnNumber < MaxTurns) return Ignore(...);
return Valid;
```

## 7. Algorithm & Complexity

- Counting empty piles is O(M) where M = number of defined supply entries (expected low, ≤~20 typical). Evaluation occurs only when `EndGameEvent` is proposed, *not* every supply mutation, keeping overhead negligible.
- No additional allocation beyond ephemeral loop variables.

## 8. Determinism & Purity

- Outcome is pure function of prior `DeckState.Supply` dictionary snapshot and options artifact contents.
- No randomness, no external time/state sources.
- Replaying the same event sequence yields identical depletion detection timing.

## 9. Invariants & Safety

- Does not weaken existing ordering invariant (ComputeScores before EndGame).
- If depletion condition reaches true prior to scoring, EndGameEvent remains ignored until scoring computed (ensuring scores always present at termination).
- Options artifact immutable; absence means feature disabled (maintains backward compatibility).

## 10. Test Matrix (Planned)

| Scenario | Setup | Events | Expected |
|----------|-------|--------|----------|
| Threshold end | threshold=2, no key piles | Exhaust 2 piles, compute scores, submit EndGameEvent | Valid -> GameEndedState added |
| Threshold not yet | threshold=3 | Only 2 piles empty, scores computed, submit EndGameEvent | Ignore ("threshold not met") |
| Key pile end | keyPiles=[province] | Province pile reaches 0, scores computed, EndGameEvent | Valid |
| Key pile not empty | keyPiles=[province] | Other piles empty, province >0 | Ignore |
| Either condition (key first) | threshold=3, key=[province] | Province empties first, scores computed | Valid (before threshold) |
| Scores missing | threshold=1 | Pile empty, EndGameEvent before ComputeScoresEvent | Ignore (scores missing) |
| Already ended | any config | Second EndGameEvent | Ignore (already ended) |
| Invalid threshold | threshold=0 | Build | Throws ArgumentOutOfRangeException |
| Unknown key pile | keyPiles include undefined id | Build/config | Throws InvalidOperationException |
| Replay determinism | threshold=2 | Script deterministic exhaustion + end | Re-run: identical states & hash parity |

## 11. Migration / Compatibility

- Existing games without invoking `WithEndTrigger` behave unchanged.
- No signature drift in baseline unless new events added (none needed); only condition logic path broadens. Baseline diff tests should pass (no phase/event ordering change).

## 12. Implementation Steps (Condensed)

1. Add `DeckBuildingEndTriggerOptions` artifact class.
2. Add builder method `WithEndTrigger(int? emptyPileThreshold = null, IEnumerable<string>? keyPiles = null)` (validate + register artifact).
3. Amend `EndGameEventCondition` to OR supply depletion logic.
4. Add tests listed in matrix under `DeckBuilding` test folder.
5. Update `docs/deck-building.md` with a new "Alternate End Trigger" section including configuration snippet.
6. Update CHANGELOG (Added: optional supply depletion end trigger) and status docs (remove from deferred list once implemented).
7. (Optional) Micro-benchmark: measure overhead of condition evaluation (likely trivial—may skip unless gating future optimization).

## 13. Future Extensions

- Multi-trigger composition (e.g., threshold AND province empty) via expression builder DSL.
- Dynamic thresholds (e.g., scaling by player count).
- Tracker state for O(1) empties count (premature now; add only if M grows large or profiling demands).

## 14. Open Questions

- Should end trigger auto-submit `ComputeScoresEvent` if missing? (Currently no; retain explicit scoring to keep deterministic phase script clarity.)
- Provide convenience method `WithStandardDepletion()` mapping to threshold=3 + keyPiles=["province"]? (Defer pending demand.)

---
*Status: Implemented. Deployed in `DeckBuildingEndTriggerOptions` + updated `EndGameEventCondition`. Options stored as extras state instead of dedicated artifact class (leaner); design otherwise realized as specified.*
