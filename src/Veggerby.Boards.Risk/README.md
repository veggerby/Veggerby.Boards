# Veggerby.Boards.Risk

Risk-style territory conquest module for Veggerby.Boards providing multi-dice combat resolution, reinforcement mechanics, territory control, fortification, and world domination win conditions built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards`. Use when you want a ready Risk-style ruleset or a foundation for territory control/wargame-style board games.

## Install

```bash
dotnet add package Veggerby.Boards.Risk
```

## Overview

This module provides a deterministic implementation of Risk-style gameplay with:

- Simplified world map with 24 territories across 4 continents
- Territory graph topology with border adjacency connections
- Cross-continent connections (Alaska-Siberia, Greenland-Iceland, etc.)
- Reinforcement mechanics: territories/3 (minimum 3) + continent bonuses
- Multi-dice combat resolution with deterministic `IRandomSource` support
- Combat algorithm: attacker (1-3 dice) vs defender (1-2 dice), defender wins ties
- Conquest mechanics with minimum army movement requirement
- Fortification: end-of-turn army movement between connected owned territories
- BFS-based connectivity verification for fortification paths
- Phase-based turn structure: Reinforce → Attack → Fortify
- Win condition: World domination (single player controls all territories)
- Player elimination tracking
- Support for 2+ players (configurable)

This package does **not** implement AI, UI, or network play.

## Quick Start

```csharp
using Veggerby.Boards.Risk;

// Create a standard 2-player Risk game
var builder = new RiskGameBuilder();
var progress = builder.Compile();

// Get players and territories
var redPlayer = progress.Game.GetPlayer(RiskIds.Players.Red);
var bluePlayer = progress.Game.GetPlayer(RiskIds.Players.Blue);
var alaska = progress.Game.GetTile(RiskIds.Territories.Alaska);

// Reinforcement phase: place armies on owned territory
progress = progress.HandleEvent(new PlaceArmiesGameEvent(
    redPlayer, 
    alaska, 
    armies: 3));

// Attack phase: attack adjacent enemy territory
var siberia = progress.Game.GetTile(RiskIds.Territories.Siberia);
var random = XorShiftRandomSource.Create(seed: 12345);
progress = progress.HandleEvent(new AttackGameEvent(
    redPlayer,
    from: alaska,
    to: siberia,
    attackerDiceCount: 3,
    random));

// If defender eliminated, conquer the territory
progress = progress.HandleEvent(new ConquerTerritoryGameEvent(
    redPlayer,
    alaska,
    siberia,
    armiesToMove: 2));

// End attack phase to move to fortification
progress = progress.HandleEvent(new EndAttackPhaseGameEvent());

// Fortify: move armies between connected territories
progress = progress.HandleEvent(new FortifyGameEvent(
    redPlayer,
    from: alaska,
    to: siberia,
    armies: 1));

// End turn
progress = progress.HandleEvent(new EndFortifyPhaseGameEvent());

// Check for world domination
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome() as RiskOutcomeState;
    Console.WriteLine($"Winner: {outcome.Winner.Id}");
}
```

## Key Concepts

### Territory Graph Structure

The simplified world map contains 24 territories grouped into 4 continents:

| Continent | Territories | Bonus Armies |
|-----------|-------------|--------------|
| North America | 6 | 3 |
| South America | 4 | 2 |
| Europe | 7 | 5 |
| Asia | 7 | 7 |

### Cross-Continent Connections

- **Alaska ↔ Siberia**: North America to Asia (sea route)
- **Greenland ↔ Iceland**: North America to Europe
- **Quebec ↔ Venezuela**: North America to South America
- **Brazil ↔ Western Europe**: South America to Europe
- **Ukraine ↔ Ural**: Europe to Asia
- **Southern Europe ↔ Middle East**: Europe to Asia

### Reinforcement Calculation

```
Total Armies = max(3, territories_owned / 3) + continent_bonuses
```

**Example**: Player owns 12 territories including all of South America:
- Base: 12 / 3 = 4 armies
- Continent bonus: +2 (South America)
- Total: 6 armies

### Combat Algorithm

Risk combat uses multi-dice comparison with defender advantage on ties:

1. **Attacker rolls 1-3 dice** (must leave at least 1 army behind)
2. **Defender rolls 1-2 dice** (based on defending armies)
3. **Sort dice descending** for both sides
4. **Compare pairs**: highest vs highest, second vs second
5. **Defender wins ties**

```
Attacker rolls: [6, 3, 1] → sorted: [6, 3, 1]
Defender rolls: [5, 4]    → sorted: [5, 4]

Comparison:
  6 > 5 → Defender loses 1 army
  3 < 4 → Attacker loses 1 army

Result: 1 attacker loss, 1 defender loss
```

### Phase Structure

Each player's turn follows three phases:

1. **Reinforce**: Place new armies on owned territories
2. **Attack**: Attack adjacent enemy territories (optional, repeatable)
3. **Fortify**: Move armies between connected owned territories (optional, once per turn)

### Events & Mutators

| Event | Phase | Mutator | Description |
|-------|-------|---------|-------------|
| `PlaceArmiesGameEvent` | Reinforce | `PlaceArmiesStateMutator` | Place armies on owned territory |
| `AttackGameEvent` | Attack | `CombatResolutionStateMutator` | Initiate attack with dice resolution |
| `ConquerTerritoryGameEvent` | Attack | `ConquerTerritoryStateMutator` | Move armies into conquered territory |
| `EndAttackPhaseGameEvent` | Attack | `EndAttackPhaseStateMutator` | Transition to Fortify phase |
| `FortifyGameEvent` | Fortify | `FortifyStateMutator` | Move armies between connected territories |
| `EndFortifyPhaseGameEvent` | Fortify | `EndFortifyPhaseStateMutator` | End turn, rotate to next player |
| `SkipFortifyPhaseGameEvent` | Fortify | `SkipFortifyPhaseStateMutator` | Skip fortification, end turn |

### Conditions

| Condition | Description |
|-----------|-------------|
| `PlaceArmiesCondition` | Validates reinforcement placement (ownership, available reinforcements) |
| `AttackCondition` | Validates attack (adjacency, ownership, sufficient armies, dice count) |
| `ConquerCondition` | Validates conquest (defender eliminated, minimum army movement) |
| `FortifyCondition` | Validates fortification (connectivity, ownership, sufficient armies) |
| `WorldDominationCondition` | Checks if single player controls all territories |
| `GameNotEndedCondition` | Guards against actions after game termination |

### State Types

**TerritoryState**: Tracks ownership and army count for each territory
```csharp
public sealed class TerritoryState : IArtifactState
{
    public Tile Territory { get; }
    public Player Owner { get; }
    public int ArmyCount { get; }
}
```

**RiskStateExtras**: Tracks game-wide phase and reinforcement state
```csharp
public sealed record RiskStateExtras(
    IReadOnlyList<Continent> Continents,
    RiskPhase CurrentPhase,
    int ReinforcementsRemaining,
    bool ConqueredThisTurn,
    int? MinimumConquestArmies);
```

**RiskOutcomeState**: Game termination outcome with winner and elimination order

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Risk"
```

**Test Coverage**:
- Reinforcement calculation (base + continent bonuses)
- Combat resolution (all dice combinations, tie handling)
- Territory conquest mechanics
- Fortification connectivity validation
- Phase transitions
- World domination detection
- Game builder functionality

## Known Limitations

- **No Cards**: Territory cards and set bonuses not implemented
- **No Capital Risk**: Capital elimination variant not implemented
- **Fixed Map**: Only simplified 24-territory map included
- **2-Player Default**: Multi-player requires custom configuration
- **No Mission Risk**: Mission-based victory conditions not implemented
- **Elimination Order**: Player elimination order is not tracked during gameplay

## Extending This Module

Common extension scenarios:

### Adding Territory Cards

```csharp
public sealed record RiskCardState(
    IReadOnlyDictionary<Player, IReadOnlyList<TerritoryCard>> PlayerHands,
    IReadOnlyList<TerritoryCard> Deck);

public sealed record TerritoryCard(string TerritoryId, CardType Type);

// Add card draw on conquest, set trading for bonus armies
```

### Custom Map Configurations

```csharp
public class ClassicRiskGameBuilder : RiskGameBuilder
{
    protected override void Build()
    {
        base.Build();
        
        // Add all 42 territories
        // Add all 6 continents
        // Configure full adjacency graph
    }
}
```

### Implementing Mission Risk

```csharp
public sealed record MissionState(
    IReadOnlyDictionary<Player, Mission> PlayerMissions);

public interface Mission
{
    bool IsCompleted(GameState state);
}

public sealed class CaptureTerritoriesCondition : IGameStateCondition
{
    // Check if player's mission is completed
}
```

## Performance Considerations

- **Combat resolution**: O(n log n) for dice sorting where n ≤ 3
- **Fortification connectivity**: BFS graph traversal, O(V + E) where V = territories, E = adjacencies
- **Reinforcement calculation**: O(T + C) where T = territories, C = continents

Target: < 1ms per combat resolution.

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines
- **Game Termination**: See [/docs/game-termination.md](../../docs/game-termination.md) for outcome patterns
- **Module Documentation**: See [/docs/risk/index.md](../../docs/risk/index.md) for detailed game rules
- **Turn Sequencing**: See [/docs/turn-sequencing.md](../../docs/turn-sequencing.md) for phase-based turn structure

## Versioning

Semantic versioning aligned with repository releases. Breaking rule or API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
