# Simulation & Playouts

The Simulation API provides a lightweight, concurrency-safe mechanism for executing deterministic (or explicitly
stochastic) playouts of a game from an initial `GameProgress` snapshot. Its primary goal is to enable Monte Carlo style
rollouts, search tree expansion, heuristic evaluation, and statistical policy improvement without introducing hidden
state or non‑deterministic side effects.

## Design Principles

- Pure & Immutable: A playout never mutates an existing `GameProgress`; each event application returns a new snapshot.
- Deterministic By Default: Given identical initial state and identical policy ordering you obtain identical playout results.
- Explicit Randomness: Any stochastic variance must originate from artifacts encoded in `GameState` (e.g., `DiceState<T>`
  governed by a deterministic RNG snapshot) – never from ambient thread-local randomness.
- Policy Driven: The simulator itself does not invent events; it delegates to an `IPlayoutPolicy` for ordered candidate
  events each step.
- Fail Fast Safety Caps: `PlayoutOptions` provide `MaxEvents` and `TimeLimit` guards for runaway sequences.
- Parallel Friendly: Multiple playouts can execute concurrently because engine, game and prior states are immutable.

## Core Types

| Type | Purpose |
| ---- | ------- |
| `IPlayoutPolicy` | Strategy interface returning ordered candidate `IGameEvent` instances for the current progress. |
| `PlayoutOptions` | Execution constraints (max events, wall clock limit, optional trace capture). |
| `GameSimulator` | Orchestrates a single or many playouts (`Playout`, `PlayoutManyAsync`). |
| `PlayoutResult` | Outcome of one playout (initial/final progress, applied count, terminal reason, optional trace). |
| `PlayoutTerminalReason` | Classification: `NoMoves`, `MaxEvents`, `TimeLimit`. |
| `PlayoutBatchResult` | Aggregation metrics for parallel playout batches (count, progressed, total, min, max, average, variance, std dev, histogram, percentile helper). |

## Basic Usage

```csharp
var builder = new ChessGameBuilder();
var progress = builder.Compile();

// Simple policy: try all legal pawn single-step advances (domain-specific enumeration omitted)
class PawnAdvancePolicy : IPlayoutPolicy
{
    public IEnumerable<IGameEvent> GetCandidateEvents(GameProgress p)
    {
        // enumerate candidate MovePieceGameEvent instances deterministically
        // ordering should be stable (e.g., by piece id then target tile id)
        return EnumeratePawnSingleSteps(p); // custom helper
    }
}

var simulator = new GameSimulator(new PawnAdvancePolicy(), new PlayoutOptions { MaxEvents = 40 });
var result = simulator.Playout(progress);
Console.WriteLine($"Applied={result.AppliedEvents} Reason={result.TerminalReason}");
```

## Parallel Playouts

```csharp
var batch = await simulator.PlayoutManyAsync(progress, count: 256, degreeOfParallelism: 8);
Console.WriteLine($"Progressed {batch.ProgressedCount}/{batch.Count} total events={batch.TotalApplied}");
```

Each worker starts from the same immutable baseline. If your policy is deterministic and no random artifacts differ,
all playouts will converge to identical results; introduce per-playout variability by initializing distinct dice states
(or seeding a custom RNG artifact) before invoking the batch operation.

## Writing a Policy

Guidelines:

1. Pure Function: Always treat the provided `GameProgress` as read-only.
2. Stable Ordering: Return candidates in a deterministic sequence (e.g., sort identifiers) to ensure reproducibility.
3. Early Termination: Returning an empty sequence signals a terminal position (`PlayoutTerminalReason.NoMoves`).
4. Avoid Expensive Re-Evaluation: The simulator re-queries the policy only after an event successfully applies; you may
   perform lightweight filtering inside `GetCandidateEvents` but defer heavy computations until necessary.
5. Deterministic Randomness: If using randomness, derive it from a value encoded in state (e.g., dice that will be rolled
   by producing a `RollDiceGameEvent<int>`). Do not use `System.Random` directly unless its seed is embedded in the state.

### Example: First Applicable Move Policy

```csharp
class FirstApplicablePolicy : IPlayoutPolicy
{
    private readonly IEnumerable<IGameEvent> _ordered;
    public FirstApplicablePolicy(IEnumerable<IGameEvent> ordered) => _ordered = ordered;
    public IEnumerable<IGameEvent> GetCandidateEvents(GameProgress progress) => _ordered;
}
```

In practice you will generate candidates dynamically each step based on the evolving board configuration. The policy can
pre-filter impossible moves to reduce rule evaluation overhead.

## Tracing State Evolution

Enable `CaptureTrace` to collect every intermediate `GameState` (including initial and final):

```csharp
var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 100, CaptureTrace = true });
var traceResult = sim.Playout(progress);
foreach (var s in traceResult.Trace!)
{
    // inspect s (hash, piece positions, etc.)
}
```

Tracing incurs allocation & retention overhead; keep it disabled in large batches unless diagnosing.

## Terminal Reasons

- `NoMoves`: Policy produced no candidates or all candidates failed to apply (ignored / rejected) at the current state.
- `MaxEvents`: Playout applied the configured maximum number of events (`MaxEvents`).
- `TimeLimit`: Wall clock limit reached mid-loop (`TimeLimit`).

These reasons are orthogonal; `MaxEvents` takes precedence over elapsed time if both thresholds are crossed in the same iteration.

## Batch Aggregation

`PlayoutBatchResult` supplies quick metrics:

- `Count`: Total playouts executed.
- `ProgressedCount`: Playouts with at least one applied event.
- `TotalApplied`: Sum of applied events across all results.
- `MinApplied` / `MaxApplied`: Shortest and longest playout lengths.
- `AverageApplied`: Mean applied event count.
- `Histogram`: Indexed by applied event count; value = number of playouts with that length.
- `Variance` / `StandardDeviation`: Distribution spread (population metrics).
- `Percentile(p)`: Nearest-rank percentile (0..100) of applied event counts.

Use these to drive adaptive sampling (e.g., stop when variance falls below tolerance or percentile gap closes).

## Performance Notes

- Avoid LINQ inside hot mutators; policies may use it sparingly for candidate construction, but prefer allocation-light enumerations.
- Reuse immutable references (engine, board, artifacts) – the simulator already leverages this when cloning baseline progress.
- Bound concurrency with `degreeOfParallelism` so CPU saturation remains controlled.

## Policy Composition & Randomization

You can compose policies and then optionally randomize candidate ordering deterministically:

```csharp
var composite = simulator.WithCompositePolicy(new TacticalPolicy(), new FallbackPolicy());
var randomizingSim = composite.WithRandomizedPolicy();
var randomResult = randomizingSim.Playout(progress);
```

The shuffle uses the `GameState.Random` source, preserving reproducibility when seeds are identical. If the RNG is null
or a candidate set contains 0/1 events, ordering falls back to original policy order.

### Legal Move Helper Policy

For quick experimentation you can start with the generic single-step policy:

```csharp
var simulator = new GameSimulator(PolicyHelpers.SingleStepAllPieces(), new PlayoutOptions { MaxEvents = 50 });
var r = simulator.Playout(progress);
```

`PolicyHelpers.SingleStepAllPieces()` enumerates (deterministically by piece id then destination tile id) every one-step
movement along outgoing tile relations for each piece. Legality is deferred to the engine's existing rule system; invalid
or blocked moves will be rejected or ignored without affecting determinism. This keeps the helper generic across modules
while enabling immediate Monte Carlo style rollout prototyping.

## Diagnostics Observer

Implement `GameSimulator.IPlayoutObserver` to receive per-step callbacks (candidate count, applied flag, event attempted) and final completion. Example: gather branching factor statistics.

```csharp
sealed class MetricsObserver : GameSimulator.IPlayoutObserver
{
  public int Steps; public int Applied; public int TotalCandidates;
  public void OnStep(GameProgress p, int stepIndex, int candidateCount, bool applied, IGameEvent attempted)
  { Steps++; TotalCandidates += candidateCount; if (applied) Applied++; }
  public void OnCompleted(PlayoutResult result) { }
}

var observer = new MetricsObserver();
var r = simulator.Playout(progress, observer);
Console.WriteLine($"Avg candidates/step={(double)observer.TotalCandidates/observer.Steps:0.00}");

```

### Branching Factor Sampling

To approximate average branching factor across many playouts:

```csharp
sealed class BranchingObserver : GameSimulator.IPlayoutObserver
{
    public int Steps; public int TotalCandidates;
    public void OnStep(GameProgress p, int idx, int candidateCount, bool applied, IGameEvent attempted)
    {
        Steps++; TotalCandidates += candidateCount;
    }
    public void OnCompleted(PlayoutResult r) { }
}

double SampleBranchingFactor(GameSimulator simulator, GameProgress progress, int samples)
{
    var observer = new BranchingObserver();
    for (int i = 0; i < samples; i++)
    {
        simulator.Playout(progress, observer);
    }
    return observer.Steps == 0 ? 0 : (double)observer.TotalCandidates / observer.Steps;
}
```

This approach lets you monitor branching factor drift as you refine policies or introduce pruning.

## Early Stop Sequential Playouts

Use `PlayoutManyUntil` to execute sequential playouts until a convergence predicate is satisfied:

```csharp
var batch = simulator.PlayoutManyUntil(progress, maxCount: 1000, stopPredicate: b => b.StandardDeviation < 0.5);
```

Predicate receives a snapshot batch each iteration; return true to terminate early.
Common patterns:

```csharp
// Stop when variance stabilizes
var batch1 = simulator.PlayoutManyUntil(progress, 2000, b => b.Variance < 0.25);

// Stop when 95th percentile length is within 2 of median (stability heuristic)
var batch2 = simulator.PlayoutManyUntil(progress, 3000, b => b.Percentile(95) - b.Percentile(50) <= 2);
```

Future enhancement (see backlog): a parallel early-stop variant evaluating convergence without waiting for all tasks.

## Extensibility Roadmap

Planned (not yet implemented) enhancements:

- Policy helpers for enumerating all legal piece moves (movement pattern expansion + legality filtering).
- Composite / stacked policies (e.g., fallback chain: tactical → random).
- Playout statistics (length histograms, branching factor samples) injected via observer hooks.
- Optional state deduplication (transposition hashing) in tree search integrations.

## Summary

The Simulation API layers a minimal deterministic playout loop atop the existing immutable engine. By externalizing move
generation into policies and constraining randomness to encoded state, it enables safe parallel rollouts suitable for
Monte Carlo evaluation, statistical feature extraction, or future reinforcement learning workflows.
