# History/Undo Integration

## Overview

The Veggerby.Boards engine provides first-class support for game history navigation through two complementary APIs:

- **`GameHistory`**: Linear history with undo/redo and timeline navigation
- **`BranchingGameHistory`**: Full branching support for "what-if" scenario exploration

These APIs enable:
- UI back/forward navigation buttons
- Teaching tools that step through games move-by-move
- Analysis mode for exploring alternative strategies
- Debugging by navigating to exact states where bugs occurred
- AI training through alternative move sequence evaluation

## Core Principles

### Immutability

All history operations return new instances—no existing state is mutated. This preserves determinism and enables safe concurrent access.

```csharp
var history = new GameHistory(initialProgress);
var history2 = history.Apply(event1);
// history remains unchanged; history2 is a new instance
```

### Determinism

Navigation operations (undo/redo/goto) produce **identical state hashes**—same sequence of events always yields the same state.

```csharp
var history = new GameHistory(initialProgress)
    .Apply(event1)
    .Apply(event2);

var originalHash = history.Current.State.Hash;

// Navigate and return
var navigated = history.Undo().Undo().Redo().Redo();

Assert.Equal(originalHash, navigated.Current.State.Hash);
```

### Memory Efficiency

History nodes share immutable `GameProgress` snapshots through structural sharing—no state duplication.

## GameHistory API

### Basic Usage

```csharp
using Veggerby.Boards.States;
using Veggerby.Boards.Chess;

// Initialize game
var builder = new ChessGameBuilder();
var progress = builder.Compile();
var history = new GameHistory(progress);

// Apply moves
history = history.Apply(moveEvent1);  // Index 0 → 1
history = history.Apply(moveEvent2);  // Index 1 → 2
history = history.Apply(moveEvent3);  // Index 2 → 3

// Undo
history = history.Undo();  // Index 3 → 2
history = history.Undo();  // Index 2 → 1

// Redo
history = history.Redo();  // Index 1 → 2

// Jump to specific point
history = history.GoTo(0);  // Back to start
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Current` | `GameProgress` | Current game progress at active timeline position |
| `CurrentIndex` | `int` | Zero-based index of current position |
| `Length` | `int` | Total number of states in timeline |
| `CanUndo` | `bool` | `true` if not at start of history |
| `CanRedo` | `bool` | `true` if not at end of timeline |

### Methods

#### `Undo()`

Moves back one step in history.

```csharp
public GameHistory Undo()
```

**Returns**: New `GameHistory` instance positioned at previous state  
**Throws**: `InvalidOperationException` if already at start

#### `Redo()`

Moves forward one step in history.

```csharp
public GameHistory Redo()
```

**Returns**: New `GameHistory` instance positioned at next state  
**Throws**: `InvalidOperationException` if already at end

#### `Apply(IGameEvent)`

Applies a new event and appends to history.

```csharp
public GameHistory Apply(IGameEvent @event)
```

**Behavior**: If not at timeline end when applying, future states are truncated and replaced with new branch starting from applied event (linear branching).

```csharp
var history = new GameHistory(progress)
    .Apply(event1)
    .Apply(event2)
    .Apply(event3);  // Timeline: [0, 1, 2, 3]

history = history.Undo().Undo();  // Index 1, Timeline: [0, 1, 2, 3]
history = history.Apply(altEvent);  // Index 2, Timeline: [0, 1, 2] (3 discarded)
```

#### `GoTo(int)`

Jumps to specific timeline position.

```csharp
public GameHistory GoTo(int index)
```

**Throws**: `ArgumentOutOfRangeException` if index out of bounds

#### `GetEventHistory()`

Returns all events applied to reach current state.

```csharp
public IReadOnlyList<IGameEvent> GetEventHistory()
```

**Note**: First event is always `NullGameEvent` (initial state marker).

## BranchingGameHistory API

Enables full branching timeline management for scenario exploration.

### Basic Usage

```csharp
using Veggerby.Boards.States;

// Initialize
var progress = builder.Compile();
var history = new BranchingGameHistory(progress);

// Main line
history = history.Apply(event1);
history = history.Apply(event2);
var mainBranchId = history.CurrentBranch.Id;

// Create alternative branch
history = history.Undo();  // Step back
history = history.CreateBranch("what-if-analysis");
history = history.Apply(alternativeEvent);

// Switch branches
history = history.SwitchToBranch(mainBranchId);  // Back to main
history = history.Apply(event3);  // Continue main line
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Current` | `GameProgress` | Current progress at active position in current branch |
| `CurrentBranch` | `HistoryBranch` | Currently active branch |
| `Branches` | `IReadOnlyDictionary<string, HistoryBranch>` | All branches |

### Methods

#### `CreateBranch(string)`

Creates new branch at current position.

```csharp
public BranchingGameHistory CreateBranch(string name)
```

**Returns**: New instance with created branch as active  
**Behavior**: New branch shares history nodes up to branch point (structural sharing)

#### `SwitchToBranch(string)`

Switches to different branch.

```csharp
public BranchingGameHistory SwitchToBranch(string branchId)
```

**Throws**: `InvalidOperationException` if branch doesn't exist

#### `Apply(IGameEvent)`

Applies event to current branch.

```csharp
public BranchingGameHistory Apply(IGameEvent @event)
```

**Behavior**: Truncates current branch's future if not at end

#### `Undo()` / `Redo()` / `GoTo(int)`

Navigation within current branch (same semantics as `GameHistory`).

#### `GetEventHistory()`

Returns event history for current branch.

### HistoryBranch

Represents a single branch in the branching timeline.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Unique branch identifier |
| `Name` | `string` | Human-readable branch name |
| `ParentBranchId` | `string?` | Parent branch ID (null for main) |
| `BranchPointIndex` | `int?` | Index in parent where branch was created |
| `Current` | `GameProgress` | Current progress in this branch |
| `CurrentIndex` | `int` | Current position in this branch |
| `Length` | `int` | Total positions in this branch |
| `CanUndo` / `CanRedo` | `bool` | Navigation availability |

## Use Cases

### 1. UI Undo/Redo Buttons

```csharp
class GameUI
{
    private GameHistory _history;

    public void OnUndoClicked()
    {
        if (_history.CanUndo)
        {
            _history = _history.Undo();
            UpdateDisplay(_history.Current);
        }
    }

    public void OnRedoClicked()
    {
        if (_history.CanRedo)
        {
            _history = _history.Redo();
            UpdateDisplay(_history.Current);
        }
    }

    public void UpdateUI()
    {
        UndoButton.Enabled = _history.CanUndo;
        RedoButton.Enabled = _history.CanRedo;
        StatusLabel.Text = $"Move {_history.CurrentIndex} of {_history.Length - 1}";
    }
}
```

### 2. Teaching Tool: Step-Through Famous Game

```csharp
// Load famous game replay
var events = LoadImmortalGame();  // Anderssen vs Kieseritzky
var history = new GameHistory(initialProgress);

foreach (var evt in events)
{
    history = history.Apply(evt);
}

// Step through move by move
for (int i = 0; i < history.Length; i++)
{
    var position = history.GoTo(i);
    Console.WriteLine($"Move {i}: {GetMoveDescription(i)}");
    DisplayPosition(position.Current.State);
    WaitForUser();
}
```

### 3. Analysis Mode: Explore Alternatives

```csharp
var history = new BranchingGameHistory(progress);

// Play main line to critical position
history = PlayMoves(history, mainLineMoves);

// Create analysis branches
var mainId = history.CurrentBranch.Id;

history = history.CreateBranch("Aggressive Line");
history = PlayMoves(history, aggressiveMoves);
var aggressiveOutcome = EvaluatePosition(history.Current);

history = history.SwitchToBranch(mainId).CreateBranch("Defensive Line");
history = PlayMoves(history, defensiveMoves);
var defensiveOutcome = EvaluatePosition(history.Current);

// Compare outcomes
Console.WriteLine($"Aggressive: {aggressiveOutcome}");
Console.WriteLine($"Defensive: {defensiveOutcome}");
```

### 4. Bug Debugging: Navigate to Failure Point

```csharp
// Replay sequence that triggers bug
var history = new GameHistory(initialProgress);

foreach (var evt in bugReproEvents)
{
    history = history.Apply(evt);

    if (DetectBug(history.Current.State))
    {
        Console.WriteLine($"Bug detected at move {history.CurrentIndex}");

        // Navigate to see state evolution
        var before = history.Undo();
        Console.WriteLine("Before: " + before.Current.State.Hash);
        Console.WriteLine("After: " + history.Current.State.Hash);

        // Try alternative event
        var alternative = before.Apply(alternativeEvent);
        if (!DetectBug(alternative.Current.State))
        {
            Console.WriteLine("Alternative event avoids bug");
        }

        break;
    }
}
```

## Performance

### Complexity

| Operation | Time | Space |
|-----------|------|-------|
| `Undo()` / `Redo()` | O(1) | O(1) |
| `GoTo(index)` | O(1) | O(1) |
| `Apply(event)` | O(n) | O(n) |
| `GetEventHistory()` | O(n) | O(n) |

Where **n** is the current index (for timeline copy during `Apply`).

### Memory Efficiency

History nodes share immutable `GameProgress` instances through structural sharing:

```
Timeline: [P0] → [P1] → [P2] → [P3]
                   ↓
After branch:    [P2'] → [P2'a] → [P2'b]
                   ↑
            (shares P0, P1, P2)
```

No state duplication—only new nodes store new progress snapshots.

### Benchmarks

Typical performance (from test suite):

- Undo/Redo: < 1ms for timelines up to 1000 moves
- GoTo: < 1ms for any position
- Apply: 2-10ms depending on event complexity (dominated by rule evaluation, not history overhead)

## Integration with Other Features

### Save/Load/Replay

Combine with replay format (Epic #68) for persistent history:

```csharp
// Save
var events = history.GetEventHistory();
var snapshot = new GameReplaySnapshot(initialState, events);
SaveToFile(snapshot);

// Load
var snapshot = LoadFromFile();
var history = new GameHistory(snapshot.InitialProgress);
foreach (var evt in snapshot.Events.Skip(1))  // Skip NullGameEvent
{
    history = history.Apply(evt);
}
```

### Legal Move Generation

Combine with legal move generator (Epic #67) for analysis:

```csharp
var history = new BranchingGameHistory(progress);
var legalMoves = legalMoveGenerator.Generate(history.Current);

foreach (var move in legalMoves)
{
    history = history.CreateBranch($"Try {move}");
    history = history.Apply(move);
    var evaluation = Evaluate(history.Current);
    history = history.SwitchToBranch(mainBranchId);  // Back to root
}
```

### State Hashing

Hash verification ensures determinism:

```csharp
var history = new GameHistory(progress);
var checkpoints = new Dictionary<int, ulong>();

// Record hashes
for (int i = 0; i < events.Length; i++)
{
    history = history.Apply(events[i]);
    checkpoints[i] = history.Current.State.Hash.Value;
}

// Verify via navigation
foreach (var (index, expectedHash) in checkpoints)
{
    var navigated = history.GoTo(index);
    Assert.Equal(expectedHash, navigated.Current.State.Hash.Value);
}
```

## Design Notes

### Why Separate `GameHistory` from `GameProgress`?

**Separation of Concerns**: `GameProgress` represents a single point-in-time snapshot (state + engine + phase). `GameHistory` is a navigation abstraction over a sequence of progress snapshots. Keeping them separate:

- Avoids polluting `GameProgress` with timeline management
- Allows optional history usage (lightweight scenarios skip it)
- Enables multiple concurrent history views over same progress chain

### Why Immutable History?

**Determinism & Safety**: Mutable history risks:
- Race conditions in concurrent scenarios
- Non-deterministic navigation (state could change between Undo/Redo)
- Accidental corruption of shared state

Immutable history guarantees:
- Thread-safe reads (multiple UIs can navigate independently)
- Reproducible navigation (GoTo(i) always yields same state)
- Safe branching (original timeline untouched)

### Linear vs Branching

**Linear (`GameHistory`)**: Simpler, lighter-weight, sufficient for 80% of use cases (undo/redo, timeline scrubbing).

**Branching (`BranchingGameHistory`)**: Full branching graph for advanced scenarios (analysis, teaching, scenario comparison).

Choose `GameHistory` unless you need explicit branch management.

## Examples

See [`samples/HistoryDemo`](../samples/HistoryDemo) for comprehensive demonstrations:

1. **Linear History**: Undo/redo and timeline navigation
2. **GoTo Navigation**: Jumping to specific positions
3. **Branching Timelines**: Creating and comparing alternative scenarios

Run the demo:

```bash
dotnet run --project samples/HistoryDemo
```

## Related Documentation

- [Core Concepts](core-concepts.md): Immutable state chain fundamentals
- [Determinism & RNG](determinism-rng-timeline.md): Hash stability guarantees
- [Save/Load/Replay](save-load-replay.md): Persistence integration (planned)
- [Legal Move Generation](legal-move-generation.md): Analysis tool integration (planned)

## FAQ

### Can I serialize history?

`GameHistory` itself isn't directly serializable (it's a navigation abstraction). Instead, serialize the event sequence via `GetEventHistory()` and reconstruct:

```csharp
var events = history.GetEventHistory();
File.WriteAllText("replay.json", JsonSerializer.Serialize(events));

// Reconstruct
var loaded = JsonSerializer.Deserialize<List<IGameEvent>>(File.ReadAllText("replay.json"));
var history = new GameHistory(initialProgress);
foreach (var evt in loaded.Skip(1))  // Skip NullGameEvent
{
    history = history.Apply(evt);
}
```

### How do I handle invalid events during replay?

Wrap `Apply` in try-catch and decide on error strategy:

```csharp
try
{
    history = history.Apply(@event);
}
catch (InvalidGameEventException ex)
{
    // Option 1: Abort replay
    throw new ReplayFailedException($"Replay failed at move {i}", ex);

    // Option 2: Skip invalid event (lossy)
    Console.WriteLine($"Skipped invalid event: {ex.Message}");

    // Option 3: Insert no-op marker
    history = history.Apply(new NoOpEvent());
}
```

### Can I annotate history nodes?

Not directly (history is intentionally minimal). For annotations, maintain a parallel dictionary:

```csharp
var annotations = new Dictionary<int, string>();
annotations[5] = "Critical tactical moment";
annotations[12] = "Blunder! Better was Qxd5";

// Display
var current = history.GoTo(5);
if (annotations.TryGetValue(5, out var note))
{
    Console.WriteLine(note);
}
```

### What's the maximum timeline length?

No hard limit, but practical constraints:

- **Memory**: Each node ~100-500 bytes (depending on game complexity); 10,000-move game ≈ 1-5 MB
- **Performance**: `Apply` is O(n) due to timeline copy; 1000+ moves may show slowdown (typically still < 10ms)

For very long games (5000+ moves), consider checkpointing: save snapshots every N moves and reconstruct shorter history segments.

### Can branches be merged?

Not automatically (deterministic merge is context-dependent). Implement domain-specific merge logic:

```csharp
// Manual merge: replay events from branch onto main
var mainBranch = history.SwitchToBranch(mainId);
var altEvents = history.SwitchToBranch(altId).GetEventHistory();

foreach (var evt in altEvents.Skip(branchPointIndex + 1))
{
    try
    {
        mainBranch = mainBranch.Apply(evt);
    }
    catch
    {
        // Handle conflicts
    }
}
```

## Future Enhancements

Potential future additions (not currently scoped):

- **History Pruning**: Auto-discard events beyond depth limit
- **Annotation API**: Built-in node metadata support
- **Diff Utilities**: Compare states/branches
- **Visual Timeline Export**: GraphViz/DOT rendering
- **Undo/Redo Grouping**: Multi-event transactions

See [Core Backlog](plans/core-backlog.md#historyundo-integration) for roadmap.
