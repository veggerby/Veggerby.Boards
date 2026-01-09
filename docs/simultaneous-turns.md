# Simultaneous Turns & Secret Commit

## Overview

The simultaneous turns system enables games where players make hidden choices that are revealed and resolved together, unlocking an entire category of games that were previously impossible or required complex workarounds:

- **Simultaneous Selection**: Rock-Paper-Scissors, bidding games, Diplomacy
- **Commit/Reveal**: Secret orders revealed simultaneously
- **Parallel Actions**: Real-time strategy elements, simultaneous turns
- **Auction Mechanics**: Sealed bids, simultaneous bidding
- **Coordination Games**: Simultaneous player decisions

## Core Concepts

### Commitment Phase

A commitment phase is a game phase where players commit actions secretly without seeing other players' choices. Actions are staged but not applied until all required players have committed.

### Reveal Phase

After all players have committed, a reveal phase applies all committed actions simultaneously in deterministic order (player ID ascending) to ensure consistent outcomes.

### Staged Events

Committed actions are stored in a `StagedEventsState` that tracks:
- **Commitments**: Dictionary of player → committed event
- **Pending Players**: Set of players who haven't yet committed
- **Completion Status**: Whether all required players have committed

## Architecture

### Artifacts

**`StagedEventsArtifact`**
- Global immutable artifact representing the staging area for simultaneous commitments
- Created when a game uses commitment phases
- Single instance per game

### State

**`StagedEventsState`**
```csharp
public sealed class StagedEventsState : ArtifactState<StagedEventsArtifact>
{
    // Dictionary of committed player actions
    public IReadOnlyDictionary<Player, IGameEvent> Commitments { get; }
    
    // Set of players who haven't committed
    public IReadOnlySet<Player> PendingPlayers { get; }
    
    // True when all required players have committed
    public bool IsComplete { get; }
    
    // Visibility is Hidden (integrates with player views)
    public Visibility Visibility => Visibility.Hidden;
}
```

**Key Properties**:
- **Immutable**: Each commitment creates a new state instance
- **Hidden Visibility**: Committed actions are hidden from other players (integrates with player view system)
- **Deterministic Equality**: Structural comparison includes all commitments and pending players

### Events

**`CommitActionEvent`**
```csharp
public sealed record CommitActionEvent(Player Player, IGameEvent Action) : IGameEvent;
```
- Represents a player committing an action during a commitment phase
- The action is staged but not applied until reveal
- Validated by `CommitActionCondition`

**`RevealCommitmentsEvent`**
```csharp
public sealed record RevealCommitmentsEvent : IGameEvent;
```
- Triggers simultaneous resolution of all committed actions
- Only valid when all required players have committed
- Applies events in deterministic order (player ID ascending)
- Clears staged events after application

### Mutators

**`CommitActionStateMutator`**
- Records player commitments in `StagedEventsState`
- Removes player from pending set
- No-op if no staged events state exists
- Throws if player has already committed or is not expected to commit

**`RevealCommitmentsStateMutator`**
- Applies all committed events in deterministic order
- Uses player ID (ascending, ordinal comparison) for tie-breaking
- Each committed event is processed through normal rule evaluation
- Removes `StagedEventsState` after successful reveal
- No-op if staged state doesn't exist or commitments incomplete

### Conditions

**Event Conditions**:
- `CommitActionCondition`: Validates player can commit (in pending set)
- `RevealCommitmentsCondition`: Validates all players have committed

**State Conditions** (for phase activation):
- `CommitmentPhaseActiveCondition`: Staged state exists and not yet complete
- `AllPlayersCommittedCondition`: Staged state exists and complete

## Usage Patterns

### Basic Commitment/Reveal Flow

```csharp
// 1. Initialize game with staged events state
var artifact = new StagedEventsArtifact("staged-events");
var pendingPlayers = new HashSet<Player> { player1, player2 };
var stagedState = new StagedEventsState(
    artifact,
    new Dictionary<Player, IGameEvent>(),
    pendingPlayers);
var initialState = GameState.New([stagedState]);

// 2. Each player commits their action
progress = progress.HandleEvent(
    new CommitActionEvent(player1, new SelectChoiceEvent(Choice.Rock)));
progress = progress.HandleEvent(
    new CommitActionEvent(player2, new SelectChoiceEvent(Choice.Scissors)));

// 3. Check if all committed (optional)
var staged = progress.State.GetStates<StagedEventsState>().Single();
if (staged.IsComplete)
{
    // 4. Reveal and resolve simultaneously
    progress = progress.HandleEvent(new RevealCommitmentsEvent());
    
    // Committed events are now applied, staged state is cleared
}
```

### Game Builder Integration

```csharp
protected override void Build()
{
    // ... board, players, pieces setup ...
    
    // Commitment phase
    AddGamePhase("commitment")
        .If<CommitmentPhaseActiveCondition>()
        .Then()
            .ForEvent<CommitActionEvent>()
                .If<CommitActionCondition>()
            .Then()
                .Do<CommitActionStateMutator>();
    
    // Reveal phase
    AddGamePhase("reveal")
        .If<AllPlayersCommittedCondition>()
        .Then()
            .ForEvent<RevealCommitmentsEvent>()
                .If<RevealCommitmentsCondition>()
            .Then()
                .Do<RevealCommitmentsStateMutator>();
}
```

## Determinism Guarantees

### Ordering

Committed events are always applied in **player ID order (ascending, ordinal comparison)**. This ensures:

1. **Reproducibility**: Same commitments → same result (always)
2. **Tie-Breaking**: Conflicts resolved consistently by player order
3. **No Race Conditions**: Order is deterministic regardless of commitment timing

### Example

```csharp
// Given players: player-a, player-b, player-c
// Commitment order: b → c → a
// Application order: a → b → c (deterministic by ID)

var orderedCommitments = stagedState.Commitments
    .OrderBy(kvp => kvp.Key.Id, StringComparer.Ordinal)
    .ToList();
// Produces: [(player-a, action-a), (player-b, action-b), (player-c, action-c)]
```

### State Hashing

`StagedEventsState` participates in state hashing for determinism verification:
- Includes all commitments (ordered by player ID)
- Includes all pending players (ordered)
- Hash changes when any commitment added or player status changes

## Integration with Player Views

`StagedEventsState` has **`Visibility.Hidden`** by default, which integrates with the player view system:

- **Committed Actions**: Hidden from all players (including the committing player)
- **Pending Status**: Can be exposed through custom projections if needed
- **Observer Views**: May show pending counts but not contents

### Custom Visibility (Future Enhancement)

**Note**: This is a potential future enhancement and not currently implemented. The current implementation always uses `Visibility.Hidden` for all staged events.

For games that might need to show a player their own commitment (future pattern):

```csharp
// Future pattern (not yet implemented)
public sealed class PlayerVisibleCommitmentState : StagedEventsState
{
    public override Visibility GetVisibility(Player? viewer)
    {
        // Players can see their own commitment, hidden from others
        return Commitments.ContainsKey(viewer) 
            ? Visibility.Private 
            : Visibility.Hidden;
    }
}
```

## Conflict Resolution

When simultaneous events conflict (e.g., two pieces moving to the same tile), resolution strategies:

### Default: Player Order Tie-Breaking

Events are applied in player ID order. First event succeeds, subsequent conflicting events may fail validation.

**Example**:
```csharp
// Player-A commits: Move piece to tile-5
// Player-B commits: Move piece to tile-5
// Resolution: Player-A's move applied first
//            Player-B's move fails (tile occupied)
```

### Extension Pattern: Custom Conflict Resolution

Games can implement custom conflict logic by creating game-specific event processors or custom mutators. The commit/reveal system provides a foundation, and games extend it with their domain rules.

**Pattern 1: Custom Mutator**
```csharp
internal sealed class CustomRevealMutator : IStateMutator<RevealCommitmentsEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, RevealCommitmentsEvent @event)
    {
        var staged = state.GetStates<StagedEventsState>().Single();
        
        // Group conflicting events
        var conflicts = DetectConflicts(staged.Commitments);
        
        // Resolve using game-specific strategy
        var resolvedEvents = ResolveConflicts(conflicts);
        
        // Apply resolved events
        var resultState = state;
        foreach (var evt in resolvedEvents)
        {
            resultState = engine.HandleEvent(evt).State;
        }
        
        // Clear staged state
        return ClearStagedState(resultState);
    }
    
    private IEnumerable<(Player, IGameEvent)> DetectConflicts(
        IReadOnlyDictionary<Player, IGameEvent> commitments)
    {
        // Game-specific conflict detection
        // Example: moves to same destination, resource contention, etc.
        return commitments.Select(kvp => (kvp.Key, kvp.Value));
    }
    
    private IEnumerable<IGameEvent> ResolveConflicts(
        IEnumerable<(Player, IGameEvent)> conflicts)
    {
        // Game-specific resolution strategy
        // Examples:
        // - Highest priority wins
        // - Cancel both conflicting moves
        // - Combine/merge effects
        // - Player order (default)
        return conflicts.Select(c => c.Item2);
    }
}
```

**Pattern 2: Pre-Resolution Validation**
```csharp
internal sealed class ConflictAwareCondition : IGameEventCondition<MoveOrderEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MoveOrderEvent @event)
    {
        // Check if this move would conflict with already-applied simultaneous moves
        var existingMoves = state.GetStates<RecentMoveState>();
        
        if (existingMoves.Any(m => m.Destination.Equals(@event.Destination)))
        {
            // Conflict detected - reject this move
            return ConditionResponse.Invalid;
        }
        
        return ConditionResponse.Valid;
    }
}
```

**Pattern 3: Post-Resolution Adjustment**
```csharp
// After reveal, apply game-specific conflict resolution
progress = progress.HandleEvent(new RevealCommitmentsEvent());

// Custom post-processing for conflicts
var conflicts = DetectPostRevealConflicts(progress.State);
foreach (var conflict in conflicts)
{
    progress = progress.HandleEvent(new ResolveConflictEvent(conflict));
}
```

The current implementation uses the default player-order strategy, which is deterministic and works for many games. For games requiring more complex conflict resolution (like Diplomacy's support/hold mechanics), implement a custom reveal mutator or post-processing logic as shown above.

## Performance Considerations

### Overhead

Commitment/reveal adds minimal overhead:
- **Staging**: O(1) per commitment (dictionary insert)
- **Reveal**: O(n log n) for sorting + O(n × rule evaluation) for application
- **State Hashing**: O(n) for commitments in hash calculation

### Optimization Opportunities

For high-frequency simultaneous events:
1. **Batch Application**: Apply all events in single state transition
2. **Pre-computed Conflicts**: Detect conflicts before application
3. **Parallel Validation**: Validate all commitments concurrently (future)

## Testing Strategy

### Unit Tests

- State construction and immutability (`StagedEventsStateTests`)
- Commitment recording (`CommitmentMechanicsTests`)
- Reveal application (`RevealCommitmentsTests`)
- Condition validation

### Integration Tests

- Full commitment/reveal flow with game builder
- Determinism verification (same commitments → same outcome)
- Conflict scenarios (overlapping actions)

### Property Tests

- Commitment order invariance: commit order doesn't affect result
- Replay determinism: replaying same commitments produces same state
- Hash stability: same staged state → same hash

## Examples

### Rock-Paper-Scissors

```csharp
// Player 1 commits Rock
progress = progress.HandleEvent(
    new CommitActionEvent(player1, new SelectChoiceEvent(Choice.Rock)));

// Player 2 commits Scissors
progress = progress.HandleEvent(
    new CommitActionEvent(player2, new SelectChoiceEvent(Choice.Scissors)));

// Reveal and determine winner
progress = progress.HandleEvent(new RevealCommitmentsEvent());
var outcome = DetermineOutcome(progress.State); // Player 1 wins
```

### Sealed-Bid Auction

```csharp
// Each player commits a bid
foreach (var player in players)
{
    var bidAmount = GetBidFromPlayer(player);
    progress = progress.HandleEvent(
        new CommitActionEvent(player, new PlaceBidEvent(bidAmount)));
}

// Reveal all bids simultaneously
progress = progress.HandleEvent(new RevealCommitmentsEvent());

// Highest bid wins (deterministic by player order if tie)
var highestBidder = DetermineWinner(progress.State);
```

### Diplomacy-Style Orders

```csharp
// Each player commits their orders for all their units
foreach (var player in players)
{
    var orders = CollectOrders(player);
    progress = progress.HandleEvent(
        new CommitActionEvent(player, new SubmitOrdersEvent(orders)));
}

// Resolve all orders simultaneously
progress = progress.HandleEvent(new RevealCommitmentsEvent());

// Conflicts (bounces, support) resolved by custom logic
ResolveConflicts(progress.State);
```

## Future Enhancements

### Partial Reveals

Support for revealing commitments in stages:
```csharp
// Reveal only specific players' commitments
progress = progress.HandleEvent(
    new PartialRevealEvent(playersToReveal));
```

### Conditional Commitments

Support for commitments that depend on other players' choices:
```csharp
// "If player A chooses X, I choose Y, otherwise Z"
progress = progress.HandleEvent(
    new ConditionalCommitEvent(player, condition, thenAction, elseAction));
```

### Timeout Support

Integration with turn clocks for automatic reveal after timeout:
```csharp
// Auto-reveal when timer expires
AddGamePhase("commitment")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .OnTimeout(() => new RevealCommitmentsEvent());
```

## Related Documentation

- **[Player Views & Hidden Information](/docs/player-views.md)** - Visibility system integration
- **[Phase-Based Design](/docs/phase-based-design.md)** - Phase architecture patterns
- **[Turn Sequencing](/docs/turn-sequencing.md)** - Turn progression and segments
- **[Determinism](/docs/determinism-rng-timeline.md)** - Deterministic execution guarantees

## API Reference

### Core Types

| Type | Namespace | Purpose |
|------|-----------|---------|
| `StagedEventsArtifact` | `Veggerby.Boards.Artifacts` | Staging area artifact |
| `StagedEventsState` | `Veggerby.Boards.States` | Immutable commitment state |
| `CommitActionEvent` | `Veggerby.Boards.Events` | Commitment event |
| `RevealCommitmentsEvent` | `Veggerby.Boards.Events` | Reveal trigger event |
| `CommitActionStateMutator` | `Veggerby.Boards.Flows.Mutators` | Commitment mutator |
| `RevealCommitmentsStateMutator` | `Veggerby.Boards.Flows.Mutators` | Reveal mutator |
| `CommitActionCondition` | `Veggerby.Boards.Flows.Rules.Conditions.Commitment` | Commit validation |
| `RevealCommitmentsCondition` | `Veggerby.Boards.Flows.Rules.Conditions.Commitment` | Reveal validation |
| `CommitmentPhaseActiveCondition` | `Veggerby.Boards.States.Conditions` | Phase activation condition |
| `AllPlayersCommittedCondition` | `Veggerby.Boards.States.Conditions` | Reveal readiness condition |

### Extension Points

Games can extend the commitment system by:
1. **Custom Commit Events**: Define domain-specific commitment events
2. **Custom Reveal Mutators**: Implement game-specific conflict resolution
3. **Custom Visibility**: Override visibility logic for specific needs
4. **Custom Conditions**: Add game-specific validation for commits/reveals

## Migration Guide

For games currently using ad-hoc synchronization:

### Before (Manual Synchronization)
```csharp
// Error-prone, non-deterministic
var actions = new Dictionary<Player, IGameEvent>();
foreach (var player in players)
{
    actions[player] = WaitForPlayerAction(player); // How to synchronize?
}
// No guarantee of order, hard to test
ApplyActionsInSomeOrder(actions);
```

### After (Commitment System)
```csharp
// Deterministic, testable, integrated
// 1. Initialize staged state
var stagedState = new StagedEventsState(artifact, [], pendingPlayers);
progress = progress.NewState([stagedState]);

// 2. Commit actions (order doesn't matter)
foreach (var player in players)
{
    var action = GetPlayerAction(player);
    progress = progress.HandleEvent(new CommitActionEvent(player, action));
}

// 3. Reveal (deterministic application)
progress = progress.HandleEvent(new RevealCommitmentsEvent());
```

## FAQ

**Q: Can I have multiple commitment phases in a single game?**  
A: Yes, you can enter/exit commitment phases multiple times. Each time requires initializing a new `StagedEventsState` with the required pending players.

**Q: What happens if a committed event becomes invalid by the time it's revealed?**  
A: The event goes through normal rule validation during reveal. Invalid events are rejected through the standard `InvalidGameEventException` mechanism.

**Q: Can I cancel a commitment before reveal?**  
A: Not in the current implementation. Once committed, the action is staged until reveal. Future enhancements may add commitment withdrawal.

**Q: How does this interact with turn sequencing?**  
A: Commitment phases can exist within turn segments (e.g., commitment during Main segment, reveal during End segment) or span multiple turns.

**Q: What if a player disconnects during commitment?**  
A: The engine doesn't handle timeouts or disconnects directly (transport layer concern). Games should implement timeout logic using turn clocks or custom conditions to handle missing commitments.

**Q: Can observers see pending commitments?**  
A: By default, no (`Visibility.Hidden`). Games can create custom projections to show pending counts or other metadata without revealing actual commitments.
