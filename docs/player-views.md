# Player Views & Hidden Information Support

## Overview

The player view system provides first-class support for **imperfect-information games** by enabling deterministic projection of game state based on visibility policies. This allows each player (or observer) to see the same underlying `GameState` with appropriate masking or redaction of hidden information.

## Why Player Views?

### Enables New Game Categories

Without player views, the engine can only properly support **perfect-information games** where all state is visible to all players (Chess, Go, Checkers). Player views enable:

- **Card Games**: Hidden hands, face-down cards (Poker, Blackjack, Bridge)
- **Strategy Games**: Fog of war, hidden units (Stratego, Battleship)
- **Cooperative Games**: Asymmetric information (Pandemic, Hanabi)
- **Analysis Tools**: Observer mode for tournaments, replays
- **AI Development**: Training agents without leaking hidden information

### Solves Key Problems

1. **Information Leakage**: UI accidentally showing opponent's cards or hidden state
2. **Duplicated Logic**: Each module implementing custom visibility logic inconsistently
3. **No Observer Support**: Cannot provide spectator views with limited visibility
4. **AI Challenges**: Reinforcement learning agents seeing information they shouldn't have
5. **Replay Analysis**: Cannot show "what player X knew at time T" for analysis

## Core Concepts

### Visibility Levels

The `Visibility` enum defines three levels of information visibility:

```csharp
public enum Visibility
{
    /// <summary>
    /// Visible to all players and observers (e.g., board position, public scores).
    /// </summary>
    Public,

    /// <summary>
    /// Visible only to the owning player (e.g., hand cards, hidden units).
    /// </summary>
    Private,

    /// <summary>
    /// Visible to no players until explicitly revealed (e.g., face-down cards, deck contents).
    /// </summary>
    Hidden
}
```

### IArtifactState Extension

All artifact states now have a `Visibility` property (default: `Public` for backward compatibility):

```csharp
public interface IArtifactState
{
    Artifact Artifact { get; }
    
    /// <summary>
    /// Gets the visibility constraint for this artifact state in player-projected views.
    /// </summary>
    Visibility Visibility => Visibility.Public;
}
```

Custom states can override this property:

```csharp
public sealed class CardState : IArtifactState
{
    public Card Card => (Card)Artifact;
    public bool IsFaceUp { get; }
    public Player? Owner { get; }
    
    // Card visibility depends on face-up status and ownership
    // Public: face-up (everyone sees)
    // Private: face-down with owner (only owner sees)
    // Hidden: no owner, e.g., in deck (no one sees)
    public Visibility Visibility => IsFaceUp 
        ? Visibility.Public 
        : (Owner != null ? Visibility.Private : Visibility.Hidden);
}
```

### Visibility Policies

`IVisibilityPolicy` implementations determine which states a player or observer can see:

```csharp
public interface IVisibilityPolicy
{
    /// <summary>
    /// Determines whether the specified viewer can see the given artifact state.
    /// </summary>
    bool CanSee(Player? viewer, IArtifactState state);

    /// <summary>
    /// Creates a redacted version of the artifact state for viewers who cannot see the full state.
    /// </summary>
    IArtifactState? Redact(Player? viewer, IArtifactState state);
}
```

#### Built-in Policies

**FullVisibilityPolicy** (default, for perfect-information games):
```csharp
var policy = FullVisibilityPolicy.Instance;
// All players see all state - backward compatible with existing games
```

### GameStateView

A `GameStateView` is a filtered, read-only projection of a `GameState`:

```csharp
public sealed class GameStateView
{
    /// <summary>
    /// Gets the collection of artifact states visible to this viewer.
    /// </summary>
    public IEnumerable<IArtifactState> VisibleStates { get; }

    /// <summary>
    /// Retrieves the visible state of a specific artifact.
    /// </summary>
    public T? GetState<T>(Artifact artifact) where T : class, IArtifactState;

    /// <summary>
    /// Gets all visible artifact states of the specified type.
    /// </summary>
    public IEnumerable<T> GetStates<T>() where T : IArtifactState;
}
```

### Observer Roles

The `ObserverRole` enum defines different access levels for spectators:

```csharp
public enum ObserverRole
{
    /// <summary>
    /// Full visibility including all hidden state (e.g., admin, arbiter, post-game analysis).
    /// </summary>
    Full,

    /// <summary>
    /// Public state only, no private or hidden information (e.g., live tournament spectator).
    /// </summary>
    Limited,

    /// <summary>
    /// View from a specific player's perspective (e.g., training replay, coaching view).
    /// </summary>
    PlayerPerspective
}
```

## Usage Examples

### Perfect-Information Games (Chess, Go)

For existing perfect-information games, **no changes are required**. The default `FullVisibilityPolicy` ensures all state is visible:

```csharp
var progress = new ChessGameBuilder().Compile();

// All players see the same state
var player1View = progress.GetViewFor(player1); // Full board
var player2View = progress.GetViewFor(player2); // Full board

// Views are identical for perfect-information games
player1View.VisibleStates.Should().BeEquivalentTo(player2View.VisibleStates);
```

### Card Games (Poker, Blackjack)

Define custom card state with visibility based on face-up status:

```csharp
public sealed class CardState : IArtifactState
{
    public Card Card => (Card)Artifact;
    public bool IsFaceUp { get; }
    public Player? Owner { get; }
    
    public Visibility Visibility => IsFaceUp 
        ? Visibility.Public 
        : (Owner != null ? Visibility.Private : Visibility.Hidden);
}
```

Use player views to filter hidden information:

```csharp
var progress = new PokerGameBuilder().Compile();

// Each player sees only their own hand
var player1View = progress.GetViewFor(player1);
var player1Cards = player1View.GetStates<CardState>(); // Only player1's cards + public cards

var player2View = progress.GetViewFor(player2);
var player2Cards = player2View.GetStates<CardState>(); // Only player2's cards + public cards

// No information leakage
player1Cards.Should().NotContain(c => c.Owner == player2 && !c.IsFaceUp);
```

### Observer Mode (Tournament Spectator)

Provide limited visibility for live spectators:

```csharp
var progress = new PokerGameBuilder().Compile();

// Limited observer sees only public state (face-up cards, board)
var spectatorView = progress.GetObserverView(ObserverRole.Limited);
var visibleCards = spectatorView.GetStates<CardState>(); // Only face-up cards

// Arbiter/admin sees everything
var arbiterView = progress.GetObserverView(ObserverRole.Full);
var allCards = arbiterView.GetStates<CardState>(); // All cards including hidden
```

### AI Training (Blackjack)

Train agents with realistic information constraints:

```csharp
var progress = new BlackjackGameBuilder().Compile();

// Agent sees dealer's face-up card, not hole card
var agentView = progress.GetViewFor(playerAgent);
var dealerCards = agentView.GetStates<CardState>()
    .Where(c => c.Card.Owner == dealer);

dealerCards.Count(c => c.IsFaceUp).Should().Be(1); // Only one visible
```

## API Reference

### GameProgress Extensions

```csharp
public static class GameProgressExtensions
{
    /// <summary>
    /// Creates a player-specific view of the current game state.
    /// </summary>
    public static GameStateView GetViewFor(
        this GameProgress progress, 
        Player player, 
        IVisibilityPolicy? policy = null);

    /// <summary>
    /// Creates an observer view of the current game state.
    /// </summary>
    public static GameStateView GetObserverView(
        this GameProgress progress, 
        ObserverRole role, 
        IVisibilityPolicy? policy = null);
}
```

### Default Projection

```csharp
public sealed class DefaultGameStateProjection : IGameStateProjection
{
    public DefaultGameStateProjection(GameState state, IVisibilityPolicy? policy = null);
    
    public GameStateView ProjectFor(Player player);
    public GameStateView ProjectForObserver(ObserverRole role);
}
```

## Implementing Custom Visibility Policies

### Example: Player-Owned Visibility

For games where players own pieces/cards and should only see their own:

```csharp
public class PlayerOwnedVisibilityPolicy : IVisibilityPolicy
{
    public bool CanSee(Player? viewer, IArtifactState state)
    {
        if (state.Visibility == Visibility.Public)
            return true;
            
        if (state.Visibility == Visibility.Hidden)
            return false;
            
        // Private: check ownership
        if (state is IPieceState ps && ps.Artifact.Owner == viewer)
            return true;
            
        return false;
    }

    public IArtifactState? Redact(Player? viewer, IArtifactState state)
    {
        if (!CanSee(viewer, state))
        {
            // Return placeholder or null
            return new RedactedPieceState(state.Artifact);
        }
        return state;
    }
}
```

### Example: Fog of War

For strategy games where visibility depends on unit positions:

```csharp
public class FogOfWarVisibilityPolicy : IVisibilityPolicy
{
    private readonly Func<Player, IEnumerable<Tile>> _getVisibleTiles;

    public FogOfWarVisibilityPolicy(Func<Player, IEnumerable<Tile>> getVisibleTiles)
    {
        _getVisibleTiles = getVisibleTiles;
    }

    public bool CanSee(Player? viewer, IArtifactState state)
    {
        if (viewer == null) return false;
        
        if (state.Visibility == Visibility.Public)
            return true;
            
        if (state is PieceState ps && ps.Artifact.Owner == viewer)
            return true; // Always see own units
            
        if (state is PieceState piece)
        {
            var visibleTiles = _getVisibleTiles(viewer);
            return visibleTiles.Contains(piece.CurrentTile);
        }
        
        return false;
    }

    public IArtifactState? Redact(Player? viewer, IArtifactState state)
    {
        return CanSee(viewer, state) ? state : null; // Omit invisible units
    }
}
```

## Design Principles

### 1. Determinism

Projections are pure functions: same input → same output.

```csharp
var view1 = progress.GetViewFor(player);
var view2 = progress.GetViewFor(player);
// view1 and view2 contain identical visible states
```

### 2. Immutability

Views are read-only; cannot mutate underlying state through a view.

```csharp
var view = progress.GetViewFor(player);
// No mutating methods on GameStateView
```

### 3. Performance

Views are lightweight wrappers with lazy evaluation:

```csharp
// Visible states computed once and cached
var view = progress.GetViewFor(player);
var states1 = view.VisibleStates; // Computed
var states2 = view.VisibleStates; // Cached
states1.Should().BeSameAs(states2);
```

### 4. Backward Compatibility

Existing games work without modification:

```csharp
// IArtifactState.Visibility defaults to Public
public interface IArtifactState
{
    Visibility Visibility => Visibility.Public;
}

// FullVisibilityPolicy is default
progress.GetViewFor(player); // Uses FullVisibilityPolicy
```

## Testing Strategies

### Unit Tests: Visibility Policies

```csharp
[Fact]
public void FullVisibilityPolicy_Should_Show_All_States()
{
    var policy = FullVisibilityPolicy.Instance;
    var state = new PieceState(piece, tile);
    
    policy.CanSee(player1, state).Should().BeTrue();
    policy.CanSee(player2, state).Should().BeTrue();
    policy.Redact(player1, state).Should().BeSameAs(state);
}
```

### Integration Tests: Full Game Flows

```csharp
[Fact]
public void Poker_Should_Hide_Opponent_Cards()
{
    var progress = new PokerGameBuilder().Compile();
    var player1View = progress.GetViewFor(player1);
    var player2Cards = player1View.GetStates<CardState>()
        .Where(c => c.Owner == player2 && !c.IsFaceUp);
        
    player2Cards.Should().BeEmpty(); // No information leak
}
```

### Parity Tests: No Information Leaks

```csharp
[Fact]
public void PlayerView_Should_Not_Leak_Hidden_Information()
{
    var progress = new CardGameBuilder().Compile();
    var view = progress.GetViewFor(player1);
    
    // Try to access full state via reflection (should fail)
    var fullState = view.GetType()
        .GetField("_underlyingState", BindingFlags.NonPublic | BindingFlags.Instance)
        ?.GetValue(view);
        
    fullState.Should().BeNull(); // Internal field not accessible
}
```

### Performance Tests

```csharp
[Fact]
public void Projection_Overhead_Should_Be_Minimal()
{
    var progress = new LargeGameBuilder().Compile();
    var stopwatch = Stopwatch.StartNew();
    
    for (int i = 0; i < 1000; i++)
    {
        var view = progress.GetViewFor(player);
        var _ = view.VisibleStates.Count();
    }
    
    stopwatch.Stop();
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // < 5% overhead target
}
```

## Migration Guide

### Existing Perfect-Information Games

**No changes required.** The default `Visibility.Public` and `FullVisibilityPolicy` ensure existing games work unchanged.

### Adding Hidden Information to Existing Games

1. **Override Visibility on States**:
```csharp
public class MyGamePieceState : IArtifactState
{
    public Visibility Visibility => isRevealed ? Visibility.Public : Visibility.Hidden;
}
```

2. **Use Player Views in UI/AI**:
```csharp
// Before:
var pieces = progress.State.GetStates<PieceState>();

// After:
var view = progress.GetViewFor(currentPlayer);
var pieces = view.GetStates<PieceState>();
```

3. **Test Information Hiding**:
```csharp
[Fact]
public void Should_Hide_Unrevealed_Pieces()
{
    var opponentView = progress.GetViewFor(opponent);
    var hiddenPieces = opponentView.GetStates<MyGamePieceState>()
        .Where(p => !p.IsRevealed);
        
    hiddenPieces.Should().BeEmpty();
}
```

## Future Enhancements

### Phase 2: Advanced Visibility Policies

- `PlayerOwnedVisibilityPolicy`: Ownership-based filtering
- `ObserverVisibilityPolicy`: Observer-specific rules
- `RedactedPieceState`: Placeholder for hidden state

### Phase 3: Dynamic Visibility

- Fog of war based on unit positions
- Timed reveals (face-down cards revealed after action)
- Partial redaction (show card suit but not rank)

### Phase 4: Integration

- Card game modules (Poker, Blackjack, Bridge)
- Strategy games with hidden units (Stratego, Battleship)
- Replay/save format enhancements for observer views

## Related Documentation

- [Core Concepts](core-concepts.md) - Foundation concepts
- [Architecture](architecture.md) - System architecture
- [Game Termination](game-termination.md) - Terminal states and outcomes

## FAQ

**Q: Do I need to change existing games?**  
A: No. Default visibility is `Public` and default policy is `FullVisibilityPolicy`, ensuring backward compatibility.

**Q: Can I have different policies for different players?**  
A: Yes. Pass a custom policy to `GetViewFor(player, customPolicy)` on a per-player basis.

**Q: How do I hide information from all players?**  
A: Set `Visibility.Hidden` on the state. Neither player can see it until explicitly revealed.

**Q: What's the performance overhead?**  
A: Minimal. Views are lazy and cached. Target is < 5% overhead vs. no projection.

**Q: Can observers see hidden state?**  
A: Depends on `ObserverRole`. `Full` sees everything, `Limited` sees public only, `PlayerPerspective` sees what a player sees.

**Q: How do I implement fog of war?**  
A: Create a custom `IVisibilityPolicy` that checks unit visibility ranges. See examples above.
