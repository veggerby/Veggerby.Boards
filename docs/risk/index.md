# Risk Module

The Risk module (`Veggerby.Boards.Risk`) implements a simplified Risk-style territory conquest game demonstrating multi-dice combat resolution patterns applicable to wargame-style modules.

## Overview

This module provides:

- **Territory Graph**: 24 territories across 4 continents with adjacency connections
- **Reinforcement Mechanics**: Territory-based army calculation with continent bonuses
- **Multi-Dice Combat**: Deterministic dice resolution using `IRandomSource`
- **Conquest & Ownership**: Territory capture and army movement mechanics
- **Fortification**: End-of-turn army movement between connected territories
- **Win Condition**: World domination (single player controls all territories)

## Territory Graph Structure

The simplified world map contains 24 territories grouped into 4 continents:

### Continents

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

## Game Flow

### Phase Structure

Each player's turn follows three phases:

1. **Reinforce**: Place new armies on owned territories
2. **Attack**: Attack adjacent enemy territories (optional, repeatable)
3. **Fortify**: Move armies between connected owned territories (optional, once per turn)

### Reinforcement Calculation

```
Total Armies = max(3, territories_owned / 3) + continent_bonuses
```

- **Base**: Territories owned divided by 3 (minimum 3)
- **Continent Bonus**: Full control of a continent adds its bonus armies

**Example**: Player owns 12 territories including all of South America:
- Base: 12 / 3 = 4 armies
- Continent bonus: +2 (South America)
- Total: 6 armies

## Combat Algorithm

Risk combat uses multi-dice comparison with defender advantage on ties.

### Combat Rules

1. **Attacker rolls 1-3 dice** (must leave at least 1 army behind)
2. **Defender rolls 1-2 dice** (based on defending armies)
3. **Sort dice descending** for both sides
4. **Compare pairs**: highest vs highest, second vs second
5. **Defender wins ties**

### Combat Resolution Example

```
Attacker rolls: [6, 3, 1] → sorted: [6, 3, 1]
Defender rolls: [5, 4]    → sorted: [5, 4]

Comparison:
  6 > 5 → Defender loses 1 army
  3 < 4 → Attacker loses 1 army

Result: 1 attacker loss, 1 defender loss
```

### Determinism

Combat resolution uses `IRandomSource` for deterministic replay:

```csharp
var random = XorShiftRandomSource.Create(seed);
var result = CombatResolver.Resolve(attackerDice: 3, defenderArmies: 2, random);
```

Same seed produces identical combat outcomes across runs.

## Events

| Event | Phase | Description |
|-------|-------|-------------|
| `PlaceArmiesGameEvent` | Reinforce | Place armies on owned territory |
| `AttackGameEvent` | Attack | Initiate attack on adjacent enemy territory |
| `ConquerTerritoryGameEvent` | Attack | Move armies into conquered territory |
| `EndAttackPhaseGameEvent` | Attack | Transition to Fortify phase |
| `FortifyGameEvent` | Fortify | Move armies between connected territories |
| `EndFortifyPhaseGameEvent` | Fortify | End turn, rotate to next player |
| `SkipFortifyPhaseGameEvent` | Fortify | Skip fortification, end turn |

## Conditions

| Condition | Description |
|-----------|-------------|
| `PlaceArmiesCondition` | Validates reinforcement placement |
| `AttackCondition` | Validates attack (adjacency, ownership, dice count) |
| `ConquerCondition` | Validates conquest (minimum army movement) |
| `FortifyCondition` | Validates fortification (connectivity, ownership) |
| `WorldDominationCondition` | Checks if single player controls all territories |
| `GameNotEndedCondition` | Guards against actions after game termination |

## State Types

### TerritoryState

Tracks ownership and army count for each territory:

```csharp
public sealed class TerritoryState : IArtifactState
{
    public Tile Territory { get; }
    public Player Owner { get; }
    public int ArmyCount { get; }
    
    public TerritoryState WithArmyDelta(int delta);
    public TerritoryState WithNewOwner(Player newOwner, int armyCount);
}
```

### RiskStateExtras

Tracks game-wide state:

```csharp
public sealed record RiskStateExtras(
    IReadOnlyList<Continent> Continents,
    RiskPhase CurrentPhase,
    int ReinforcementsRemaining,
    bool ConqueredThisTurn,
    int? MinimumConquestArmies);
```

### RiskOutcomeState

Game termination outcome implementing `IGameOutcome`:

```csharp
public sealed class RiskOutcomeState : IArtifactState, IGameOutcome
{
    public Player Winner { get; }
    public IReadOnlyList<Player> EliminationOrder { get; }
    public string TerminalCondition => "WorldDomination";
}
```

## Builder Configuration

```csharp
var progress = new RiskGameBuilder().Compile();

// Access game and state
var game = progress.Game;
var state = progress.State;

// Handle events
progress = progress.HandleEvent(new PlaceArmiesGameEvent(player, territory, armies));
```

### Custom Maps

Extend `RiskGameBuilder` to create custom territory configurations:

```csharp
public class CustomRiskGameBuilder : RiskGameBuilder
{
    protected override void Build()
    {
        base.Build();
        
        // Add additional territories
        AddTile("custom-territory-1");
        
        // Add adjacency
        WithTile("custom-territory-1")
            .WithRelationTo("territory-alaska")
            .InDirection(AdjacentDirection);
    }
}
```

## Testing

### Combat Resolution Tests

```csharp
[Fact]
public void Resolve_DefenderWinsTies()
{
    var attackerRolls = new[] { 4, 3 };
    var defenderRolls = new[] { 4, 3 };
    
    var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 2);
    
    result.AttackerLosses.Should().Be(2);  // Ties go to defender
    result.DefenderLosses.Should().Be(0);
}
```

### Reinforcement Calculation Tests

```csharp
[Fact]
public void Calculate_WithContinentControl_IncludesBonus()
{
    // Player owns all of South America (4 territories)
    var result = ReinforcementCalculator.Calculate(player, state, continents);
    
    result.Should().Be(5); // max(3, 4/3) + 2 bonus = 3 + 2 = 5
}
```

## Performance Considerations

- **Combat resolution**: O(n log n) for dice sorting where n ≤ 3
- **Fortification connectivity**: BFS graph traversal, O(V + E) where V = territories, E = adjacencies
- **Reinforcement calculation**: O(T + C) where T = territories, C = continents

Target: < 1ms per combat resolution.

## Related Documentation

- [Turn Sequencing](../turn-sequencing.md) - Phase-based turn structure
- [Game Termination](../game-termination.md) - Win condition patterns
- [Phase-Based Design](../phase-based-design.md) - Event gating by phase
