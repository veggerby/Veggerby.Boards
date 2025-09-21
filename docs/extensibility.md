# Extensibility Guide

This guide walks through adding a new game or extending engine behavior.

## Adding a New Game Module

1. Create a new project `Veggerby.Boards.<GameName>` referencing `Veggerby.Boards`.
2. Add a `<GameName>GameBuilder : GameBuilder` class.
3. Inside `Build()`:
   - Set `BoardId`.
   - Define players (`AddPlayer("p1")`).
   - Define directions (`AddDirection("north")`).
   - Define tiles + relations:

     ```csharp
     var tile = AddTile("tile-a1");
     tile.WithRelationTo("tile-a2").InDirection("north");
     ```

   - Define artifacts (pieces, dice, custom):

     ```csharp
     AddPiece("white-rook-1").WithOwner("white").HasDirection("north").CanRepeat();
     AddDice("d6").HasNoValue();
     ```

   - Set initial placements / dice values:

     ```csharp
     WithPiece("white-rook-1").OnTile("tile-a1");
     WithDice("d6").HasValue(3); // optional
     ```

   - Define phases with rules:

     ```csharp
     AddGamePhase("movement")
       .If<NullGameStateCondition>()
       .Then()
         .ForEvent<MovePieceGameEvent>()
         .Then()
           .Do<MovePieceStateMutator>();
     ```

4. Call `Compile()` to obtain initial `GameProgress`.

## Creating Custom Movement Patterns

If a piece requires movement not representable by current direction or fixed sequence patterns:

- Combine existing direction patterns using `HasPattern("dir1", "dir2", ...)` for fixed sequences.
- For dynamic logic (e.g., teleport, capture-dependent moves), introduce a new `IPattern` implementation + visitor support.

## Adding a New Event Type

1. Define an event class implementing `IGameEvent` with required payload.
2. Add optional pre-processor(s) if the event expands into atomic sub-events.
3. Create a rule: subclass `GameEventRule<YourEvent>` or use a `SimpleGameEventRule` factory.
4. Add state mutators implementing `IStateMutator<YourEvent>`.
5. Wire into a `GamePhase` via builder: `.ForEvent<YourEvent>() ...`.

## Writing a State Mutator

Implement:

```csharp
public class HealPieceStateMutator : IStateMutator<HealPieceGameEvent>
{
    public GameState MutateState(GameEngine engine, GameState gameState, HealPieceGameEvent @event)
    {
        // derive new artifact states (e.g., status flags) and return gameState.Next(newStates)
    }
}
```

Guidelines:

- Do not mutate existing `IArtifactState` instances.
- Return original `gameState` if no changes.
- Validate invariants early; throw explicit exceptions on invalid conditions.

## Adding Conditions

- For state-driven phase activation: implement `IGameStateCondition`.
- For event validation: implement a class deriving `IGameEventCondition` (pattern mirrors existing conditions like `TileBlockedGameEventCondition`).
- Compose with provided composite helpers for logical AND / OR semantics.

## Phases & Hierarchy

Use a root composite to model macro stages (e.g., Setup → Main → Resolution). Each child adds specialized rules.

Example:

```csharp
AddGamePhase("setup")
  .If<InitialGameStateCondition>()
  .Then()
    .ForEvent<RollDiceGameEvent<int>>()
      .Then()
        .Do<DiceStateMutator<int>>();

AddGamePhase("play")
  .If<NullGameStateCondition>()
  .Then()
    .ForEvent<MovePieceGameEvent>()
      .Then()
        .Do<MovePieceStateMutator>();
```

## Custom Artifacts

If you need a new artifact kind (e.g., EnergyNode, TokenStack):

1. Subclass `Artifact`.
2. Provide definition method in builder (`AddArtifact("id").WithFactory(id => new EnergyNode(id))`).
3. Create matching `IArtifactState` types if runtime properties vary (e.g., capacity, counters).
4. Integrate via events + mutators.

## Debugging & Validation Tips

- Use `GameState.CompareTo(previousState)` to inspect changes.
- Add focused tests per rule branch (Valid, Ignore, Invalid).
- Keep mutators small; one concern per class increases composability.

## When to Introduce New Abstractions

Add new types ONLY if:

- Multiple rules share logic that cannot be expressed with composition.
- A new domain invariant emerges (e.g., stacking, capture zones).
- Performance hotspots justify specialized structure.

Avoid premature generalization—prefer clarity and explicitness.

## Checklist Before Shipping a New Game Module

- [ ] All phases defined and reachable.
- [ ] All mutators covered by tests (happy + edge + failure).
- [ ] No orphan directions or unreachable tiles.
- [ ] Dice / random artifacts start in a deterministic state (explicit Null or seed strategy).
<!-- API layer mapping step omitted (HTTP facade currently removed). -->

## Next Steps

See `core-concepts.md` for deeper semantic grounding. (HTTP exposure package will be reconsidered later.)
