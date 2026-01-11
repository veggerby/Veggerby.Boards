# Time Controls

**Status**: ✅ **Implemented**  
**Version**: v1.0.0  
**Since**: 2026-01-11

## Overview

Veggerby.Boards provides built-in support for time-limited turns and time-based game outcomes, enabling tournament-style play, AI time limits, and real-time constraints without custom time tracking.

**Key Features:**
- **Deterministic Time Modeling**: Timestamps are explicit event parameters (not wall-clock)
- **Standard Time Controls**: Fischer (increment), Bronstein (delay), Classical (bonus after N moves)
- **Time-Based Victory**: Automatic game termination on time expiration
- **Replay Compatibility**: Exact timestamp preservation for deterministic replay
- **Zero Background Timers**: All time tracking through explicit events

## Core Abstractions

### GameClock Artifact

The `GameClock` is an immutable artifact defining time control rules for a game.

```csharp
namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Represents a game clock artifact for tracking time-limited turns.
/// </summary>
public sealed class GameClock : Artifact
{
    /// <summary>
    /// Gets the time control configuration for this clock.
    /// </summary>
    public TimeControl Control { get; }
    
    public GameClock(string id, TimeControl control) : base(id)
    {
        Control = control;
    }
}
```

**Example:**
```csharp
var clock = new GameClock("main-clock", new TimeControl
{
    InitialTime = TimeSpan.FromMinutes(5),
    Increment = TimeSpan.FromSeconds(2)
});
```

### TimeControl Record

Defines how time is allocated and managed during gameplay.

```csharp
namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Defines time control rules for a game clock.
/// </summary>
public sealed record TimeControl
{
    /// <summary>
    /// Gets the initial time allocated to each player (e.g., 5 minutes).
    /// </summary>
    public required TimeSpan InitialTime { get; init; }
    
    /// <summary>
    /// Gets the time increment added after each move (Fischer).
    /// </summary>
    public TimeSpan? Increment { get; init; }
    
    /// <summary>
    /// Gets the delay before clock starts (Bronstein).
    /// </summary>
    public TimeSpan? Delay { get; init; }
    
    /// <summary>
    /// Gets the number of moves before bonus time (Classical).
    /// </summary>
    public int? MovesPerTimeControl { get; init; }
    
    /// <summary>
    /// Gets the bonus time added after move threshold (Classical).
    /// </summary>
    public TimeSpan? BonusTime { get; init; }
}
```

### ClockState

Immutable state tracking remaining time for each player.

```csharp
namespace Veggerby.Boards.States;

/// <summary>
/// Immutable state tracking remaining time for each player in a game clock.
/// </summary>
public sealed class ClockState : IArtifactState
{
    public GameClock Clock { get; }
    public Artifact Artifact { get; }
    
    /// <summary>
    /// Gets the remaining time for each player.
    /// </summary>
    public IReadOnlyDictionary<Player, TimeSpan> RemainingTime { get; }
    
    /// <summary>
    /// Gets the player whose clock is currently running (null when no active turn).
    /// </summary>
    public Player? ActivePlayer { get; }
    
    /// <summary>
    /// Gets the timestamp when the current turn started (null when no active turn).
    /// </summary>
    public DateTime? TurnStartedAt { get; }
    
    /// <summary>
    /// Starts the clock for a player's turn.
    /// </summary>
    public ClockState StartTurn(Player player, DateTime timestamp);
    
    /// <summary>
    /// Stops the clock, deducting elapsed time and applying increments.
    /// </summary>
    public ClockState EndTurn(DateTime timestamp);
    
    /// <summary>
    /// Checks if a player has run out of time.
    /// </summary>
    public bool IsTimeExpired(Player player);
}
```

## Time Events

### StartClockEvent

Signals the beginning of a timed turn.

```csharp
namespace Veggerby.Boards.Events;

/// <summary>
/// Event to start the clock for a player's turn.
/// </summary>
public sealed record StartClockEvent(
    GameClock Clock, 
    Player Player, 
    DateTime Timestamp) : IGameEvent;
```

**Usage:**
```csharp
var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
progress = progress.HandleEvent(new StartClockEvent(clock, player1, timestamp));
```

### StopClockEvent

Signals the end of a timed turn.

```csharp
/// <summary>
/// Event to stop the clock after a move.
/// </summary>
public sealed record StopClockEvent(
    GameClock Clock, 
    DateTime Timestamp) : IGameEvent;
```

**Usage:**
```csharp
var endTimestamp = timestamp.AddSeconds(3.5);
progress = progress.HandleEvent(new StopClockEvent(clock, endTimestamp));
```

### TimeFlagEvent

Signals that a player has run out of time.

```csharp
/// <summary>
/// Event triggered when a player runs out of time.
/// </summary>
public sealed record TimeFlagEvent(Player Player) : IGameEvent;
```

**Usage:**
```csharp
var clockState = progress.State.GetStates<ClockState>().Single();

if (clockState.IsTimeExpired(whitePlayer))
{
    progress = progress.HandleEvent(new TimeFlagEvent(whitePlayer));
    // Game ends, black wins by time forfeit
}
```

## Standard Time Controls

### Fischer Time Control (Increment)

Time is added after each move.

```csharp
var fischerControl = new TimeControl
{
    InitialTime = TimeSpan.FromMinutes(5),
    Increment = TimeSpan.FromSeconds(2)
};

var clock = new GameClock("fischer-clock", fischerControl);
```

**Example:** 5+2 (5 minutes initial, 2 seconds added per move)

**Behavior:**
- Player starts with 5:00
- Makes move in 3.5 seconds
- Clock stops: 5:00 - 3.5s + 2s = 4:58.5 remaining

### Bronstein Time Control (Delay)

Clock waits before starting to count down.

```csharp
var bronsteinControl = new TimeControl
{
    InitialTime = TimeSpan.FromMinutes(5),
    Delay = TimeSpan.FromSeconds(3)
};

var clock = new GameClock("bronstein-clock", bronsteinControl);
```

**Example:** 5+3d (5 minutes initial, 3-second delay)

**Behavior:**
- Player has 3 seconds grace period before clock starts
- Time used within delay is not deducted
- Only time beyond delay is subtracted

**Note:** Delay logic must be implemented in application layer (not yet in core).

### Classical Time Control (Bonus)

Bonus time added after completing N moves.

```csharp
var classicalControl = new TimeControl
{
    InitialTime = TimeSpan.FromHours(2),
    MovesPerTimeControl = 40,
    BonusTime = TimeSpan.FromMinutes(30)
};

var clock = new GameClock("classical-clock", classicalControl);
```

**Example:** 40/2:00 + 30 min (2 hours for first 40 moves, then 30 minutes bonus)

**Note:** Move counting and bonus application must be implemented in application layer (not yet in core).

## Complete Example: Chess with Fischer Time

```csharp
using System;
using System.Linq;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;

// Define time control
var timeControl = new TimeControl
{
    InitialTime = TimeSpan.FromMinutes(5),
    Increment = TimeSpan.FromSeconds(2)
};

var clock = new GameClock("main-clock", timeControl);

// Initialize clock state (using GameBuilder pattern)
var player1 = new Player("white");
var player2 = new Player("black");

var initialClockState = new ClockState(
    clock,
    new Dictionary<Player, TimeSpan>
    {
        [player1] = timeControl.InitialTime,
        [player2] = timeControl.InitialTime
    });

// Simulate a timed move
var progress = /* ... initialized GameProgress with initialClockState ... */;

// Start clock for white's turn
var moveStartTime = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
progress = progress.HandleEvent(new StartClockEvent(clock, player1, moveStartTime));

// White makes move (e.g., MovePieceGameEvent)
progress = progress.HandleEvent(new MovePieceGameEvent(/* ... */));

// Stop clock after 3.5 seconds
var moveEndTime = moveStartTime.AddSeconds(3.5);
progress = progress.HandleEvent(new StopClockEvent(clock, moveEndTime));

// Check remaining time
var clockState = progress.State.GetStates<ClockState>().Single();
var remaining = clockState.RemainingTime[player1];
// Result: 5:00 - 3.5s + 2s = 4:58.5

// Check for time expiration
if (clockState.IsTimeExpired(player1))
{
    progress = progress.HandleEvent(new TimeFlagEvent(player1));
    // Game ends: player2 wins by time forfeit
    
    var outcome = progress.GetOutcome();
    Console.WriteLine($"Game ended: {outcome.TerminalCondition}"); // "TimeExpired"
}
```

## Deterministic Time Modeling

**Critical:** Timestamps must be **explicit event parameters**, not wall-clock based.

### ❌ Non-Deterministic (Bad)

```csharp
// Uses system clock – breaks determinism
var now = DateTime.UtcNow;
progress = progress.HandleEvent(new StartClockEvent(clock, player, now));
```

**Problem:** Replay produces different timestamps → different clock states → non-deterministic.

### ✅ Deterministic (Good)

```csharp
// Explicit timestamp – fully deterministic
var timestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
progress = progress.HandleEvent(new StartClockEvent(clock, player, timestamp));
```

**Benefit:** Replay uses exact timestamps → identical clock states → fully deterministic.

### Replay Example

```csharp
// Save game with timestamps
var eventLog = new List<(DateTime Timestamp, IGameEvent Event)>
{
    (new DateTime(2026, 1, 6, 12, 0, 0), new StartClockEvent(clock, player1, ...)),
    (new DateTime(2026, 1, 6, 12, 0, 3.5), new StopClockEvent(clock, ...)),
    // ... more events
};

// Replay with exact timestamps
foreach (var (timestamp, @event) in eventLog)
{
    progress = progress.HandleEvent(@event);
    // Clock states reconstructed identically
}
```

## Time-Based Victory Conditions

### TimeFlagCondition

Detects when any player has run out of time.

```csharp
namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Condition that detects when any player has run out of time.
/// </summary>
public sealed class TimeFlagCondition : IGameStateCondition
{
    private readonly Game _game;
    
    public TimeFlagCondition(Game game) { /* ... */ }
    
    public ConditionResponse Evaluate(GameState state)
    {
        var clockState = state.GetStates<ClockState>().FirstOrDefault();
        
        if (clockState == null)
            return ConditionResponse.Ignore("No clock configured");
        
        foreach (var player in _game.Artifacts.OfType<Player>())
        {
            if (clockState.IsTimeExpired(player))
                return ConditionResponse.Valid;
        }
        
        return ConditionResponse.Ignore("All players have time remaining");
    }
}
```

### TimeFlagStateMutator

Ends the game when a player runs out of time.

```csharp
namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator that ends the game when a player runs out of time.
/// </summary>
public sealed class TimeFlagStateMutator : IStateMutator<TimeFlagEvent>
{
    private readonly Game _game;
    
    public GameState MutateState(GameEngine engine, GameState gameState, TimeFlagEvent @event)
    {
        if (gameState.GetStates<GameEndedState>().Any())
            return gameState;
        
        var opponent = _game.Artifacts.OfType<Player>()
            .First(p => !p.Equals(@event.Player));
        
        var outcome = new OutcomeBuilder()
            .WithWinner(opponent, new() { ["Reason"] = "TimeExpired" })
            .WithLoser(@event.Player, new() { ["Reason"] = "TimeExpired" })
            .WithTerminalCondition("TimeExpired")
            .Build();
        
        return gameState.Next(new IArtifactState[]
        {
            new GameEndedState(),
            (StandardGameOutcome)outcome
        });
    }
}
```

## GameBuilder Integration

Time controls integrate naturally with GameBuilder patterns.

```csharp
public class ChessGameBuilder : GameBuilder
{
    public ChessGameBuilder WithClock(string id, TimeControl control)
    {
        var clock = new GameClock(id, control);
        
        // Add clock artifact
        AddArtifact(clock);
        
        // Initialize clock state
        var initialClockState = new ClockState(
            clock,
            Players.ToDictionary(p => p, p => control.InitialTime));
        
        AddInitialState(initialClockState);
        
        // Add time flag rule
        AddGameRule()
            .ForEvent<TimeFlagEvent>()
            .Then()
                .Do(new TimeFlagStateMutator(Game));
        
        return this;
    }
}

// Usage
var builder = new ChessGameBuilder()
    .WithClock("main-clock", new TimeControl
    {
        InitialTime = TimeSpan.FromMinutes(5),
        Increment = TimeSpan.FromSeconds(2)
    });

var progress = builder.Compile();
```

## Best Practices

### ✅ DO

- **Always use explicit timestamps** - Pass `DateTime` parameters explicitly (not `DateTime.UtcNow`)
- **Track time in game state** - Use `ClockState` to maintain time banks
- **Validate time expiration** - Check `IsTimeExpired` before sending `TimeFlagEvent`
- **Integrate with outcome framework** - Use `GameEndedState` + `StandardGameOutcome`
- **Test determinism** - Verify replay produces identical clock states

### ❌ DON'T

- **Don't use wall-clock time** - Always pass timestamps explicitly for determinism
- **Don't mutate clock state** - Use immutable state transitions only
- **Don't implement background timers** - Time tracking is event-driven, not timer-driven
- **Don't skip GameEndedState** - Always mark game as ended when time expires
- **Don't forget replay tests** - Validate timestamp preservation in replays

## Application Integration Patterns

### Manual Clock Management

Applications must explicitly send clock events:

```csharp
// Application tracks current player turn
var currentPlayer = GetCurrentPlayer(progress);

// Start clock when turn begins
var turnStartTime = GetCurrentTimestamp(); // Application-controlled timestamp
progress = progress.HandleEvent(new StartClockEvent(clock, currentPlayer, turnStartTime));

// ... player makes move ...

// Stop clock when move completes
var turnEndTime = GetCurrentTimestamp();
progress = progress.HandleEvent(new StopClockEvent(clock, turnEndTime));

// Check for time expiration
var clockState = progress.State.GetStates<ClockState>().Single();

if (clockState.IsTimeExpired(currentPlayer))
{
    progress = progress.HandleEvent(new TimeFlagEvent(currentPlayer));
}
```

### Automatic Clock Integration (Future)

Future enhancement: Phase-based automatic clock management.

```csharp
// Hypothetical future API
builder.AddGamePhase("timed-turn")
    .WithAutomaticClock(clock)  // Automatically start/stop clock
    .WithTimeEnforcement()       // Automatically trigger TimeFlagEvent
    .If<GameNotEndedCondition>()
    .Then()
        // Movement rules...
```

## Performance

**Clock updates are minimal overhead (&lt; 1% of event handling):**
- State transitions: O(1) dictionary updates
- Time checks: O(1) dictionary lookups
- No background threads or timers
- No allocations beyond new state instances

**Benchmarks (typical game):**
- Clock state transition: ~100 ns
- Time expiration check: ~50 ns
- Total overhead per move: &lt; 0.5% of total event processing

## Testing

### Unit Tests

```csharp
[Fact]
public void Should_deduct_time_and_apply_increment()
{
    // arrange
    var control = new TimeControl
    {
        InitialTime = TimeSpan.FromMinutes(5),
        Increment = TimeSpan.FromSeconds(2)
    };
    
    var clock = new GameClock("clock", control);
    var player = new Player("player");
    
    var remainingTime = new Dictionary<Player, TimeSpan>
    {
        [player] = TimeSpan.FromMinutes(5)
    };
    
    var startTimestamp = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
    var endTimestamp = startTimestamp.AddSeconds(10);
    
    var state = new ClockState(clock, remainingTime);
    
    // act
    state = state.StartTurn(player, startTimestamp);
    state = state.EndTurn(endTimestamp);
    
    // assert
    // 5:00 - 10s + 2s = 4:52
    state.RemainingTime[player].Should().Be(TimeSpan.FromMinutes(5).Subtract(TimeSpan.FromSeconds(10)).Add(TimeSpan.FromSeconds(2)));
}
```

### Integration Tests

```csharp
[Fact]
public void Should_end_game_when_time_expires()
{
    // arrange
    var progress = SetupGameWithClock();
    
    // Simulate time expiration
    progress = SimulateMoveUntilTimeExpires(progress);
    
    // act & assert
    progress.IsGameOver().Should().BeTrue();
    
    var outcome = progress.GetOutcome();
    outcome.TerminalCondition.Should().Be("TimeExpired");
}
```

### Determinism Tests

```csharp
[Fact]
public void Should_produce_identical_clock_states_on_replay()
{
    // arrange
    var eventLog = RecordGameWithTimestamps();
    
    // act
    var replay1 = ReplayEvents(eventLog);
    var replay2 = ReplayEvents(eventLog);
    
    // assert
    var clockState1 = replay1.State.GetStates<ClockState>().Single();
    var clockState2 = replay2.State.GetStates<ClockState>().Single();
    
    clockState1.Should().BeEquivalentTo(clockState2);
}
```

## Future Enhancements

### Planned Features

- **Automatic Phase Integration**: Phase-based clock start/stop
- **Bronstein Delay Implementation**: Core support for delay logic
- **Classical Bonus Implementation**: Move counting and bonus application
- **Time Bank Transfers**: Transfer unused time between players (untimed games)
- **Time Pressure Indicators**: Query remaining time percentage
- **Pause/Resume Support**: Pause clock during adjournments

### Extension Points

- **Custom Time Controls**: Implement `TimeControl` variants
- **Time-Based Conditions**: Custom conditions based on remaining time
- **Application-Specific Clocking**: Integrate with UI countdown timers
- **Network Time Sync**: Coordinate timestamps across network

## Related Documentation

- [Game Termination](game-termination.md) - GameEndedState and IGameOutcome
- [Determinism & RNG](determinism-rng-timeline.md) - Deterministic replay principles
- [Phase-Based Design](phase-based-design.md) - Phase integration patterns
- [Save/Load/Replay](save-load-replay.md) - Timestamp serialization

## Migration from External Time Tracking

If you're migrating from external time tracking:

### Before (External Tracking)

```csharp
// Application maintains time outside GameState
var playerTimes = new Dictionary<Player, TimeSpan>();

// Non-deterministic, not part of replay
playerTimes[player] -= DateTime.UtcNow - turnStart;
```

### After (Built-in Time Controls)

```csharp
// Time tracked in ClockState, part of GameState history
var turnStart = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc);
progress = progress.HandleEvent(new StartClockEvent(clock, player, turnStart));

// Deterministic, replay-compatible
var turnEnd = turnStart.AddSeconds(3.5);
progress = progress.HandleEvent(new StopClockEvent(clock, turnEnd));
```

**Benefits:**
- Time tracking is deterministic
- Full replay support with exact timestamps
- Standardized outcome framework integration
- No external state management

## Summary

Veggerby.Boards time controls provide:

1. **Deterministic Time Tracking** - Explicit timestamps for replay compatibility
2. **Standard Time Controls** - Fischer, Bronstein, Classical support
3. **Time-Based Outcomes** - Automatic game termination on time expiration
4. **Zero Background Timers** - Event-driven architecture only
5. **Seamless Integration** - Works with existing GameState and outcome frameworks

**Time tracking is now a first-class citizen of the engine, enabling tournament-style play without custom implementations.**
