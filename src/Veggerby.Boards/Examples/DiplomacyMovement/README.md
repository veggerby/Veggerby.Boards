# Diplomacy Movement Example

A simplified Diplomacy-style game demonstrating simultaneous movement orders with conflict resolution.

## Purpose

This example shows the **commit/reveal pattern** for simultaneous turns:

1. **Players commit secret orders** - Each player submits movement orders for their units without seeing other players' choices
2. **All orders revealed together** - Orders are revealed and applied simultaneously in deterministic order (player ID)
3. **Conflicts resolved** - When multiple units move to the same territory, player-order tie-breaking determines the winner

## Simplifications

This is **not** a complete Diplomacy implementation. Real Diplomacy includes:

- **Support orders** - Units helping other units' moves
- **Hold orders** - Defensive positions
- **Convoy orders** - Naval transport
- **Retreat phases** - After failed moves
- **Build/disband phases** - Reinforcement mechanics
- **Complex adjacency rules** - Land/sea territories, coasts
- **Bounce mechanics** - Symmetric conflicts where both moves fail

This example focuses solely on demonstrating the **simultaneous action architecture** from Epic #71.

## Architecture

### Events

**`MoveOrderEvent`**
```csharp
public sealed record MoveOrderEvent(Player Player, Piece Unit, Tile Destination) : IGameEvent
```

### Phases

1. **Commitment Phase** - Players commit `MoveOrderEvent` wrapped in `CommitActionEvent`
2. **Reveal Phase** - `RevealCommitmentsEvent` triggers simultaneous resolution
3. **Movement Phase** - Committed move orders are applied in player ID order

### Conflict Resolution

When multiple units commit to move to the same territory:

- Events applied in **player ID order** (alphabetical, ordinal comparison)
- First player's move succeeds
- Subsequent moves to the same destination fail validation (tile occupied)

**Example**:
```csharp
// England (player-id: "england") commits: london → paris
// Germany (player-id: "germany") commits: berlin → paris

// Reveal applies in order: england, germany
// England's move succeeds (london → paris)
// Germany's move fails (paris occupied by England)
```

## Usage

```csharp
var builder = new DiplomacyMovementGameBuilder();
var progress = builder.Compile();

var england = progress.Game.GetPlayer("england");
var france = progress.Game.GetPlayer("france");

var englandArmy = progress.Game.GetPiece("england-army-1");
var franceArmy = progress.Game.GetPiece("france-army-1");

// Initialize commitment phase
var stagedState = new StagedEventsState(
    artifact,
    new Dictionary<Player, IGameEvent>(),
    new HashSet<Player> { england, france });

progress = progress.NewState([stagedState]);

// Commit orders
progress = progress.HandleEvent(
    new CommitActionEvent(england, new MoveOrderEvent(england, englandArmy, paris)));
progress = progress.HandleEvent(
    new CommitActionEvent(france, new MoveOrderEvent(france, franceArmy, marseilles)));

// Reveal and resolve
progress = progress.HandleEvent(new RevealCommitmentsEvent());

// Units have moved to their destinations (if valid)
```

## Tests

See `DiplomacyMovementIntegrationTests.cs`:

- **Non-conflicting moves** - Both players move to different territories
- **Determinism** - Commitment order doesn't affect outcome
- **Conflict resolution** - Player-order tie-breaking (future enhancement)

## Extending

For a complete Diplomacy implementation:

1. Add support/hold/convoy order types
2. Implement complex adjacency validation
3. Add retreat and build phases
4. Implement bounce mechanics (symmetric conflicts)
5. Add custom conflict resolution logic (see `/docs/simultaneous-turns.md`)

## Related

- **Documentation**: `/docs/simultaneous-turns.md` - Complete guide to simultaneous turns system
- **Examples**: 
  - `RockPaperScissors` - Simplest simultaneous selection
  - `SealedBidAuction` - Hidden bidding with reveal
- **Epic**: veggerby/Veggerby.Boards#71 - Simultaneous Turns / Secret Commit
