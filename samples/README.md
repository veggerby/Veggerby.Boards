# Veggerby.Boards Samples

Demonstration projects showing how to use different game modules.

## Available Demos

### ChessDemo

Plays the famous "Immortal Game" (Anderssen vs. Kieseritzky, 1851) demonstrating:
- Complete 23-move game from opening to checkmate
- SAN notation parsing
- Piece sacrifices and complex tactics
- Checkmate detection

**Run:** `dotnet run --project ChessDemo`

See [ChessDemo/README.md](ChessDemo/README.md) for details.

### GoDemo

Demonstrates Go (Weiqi/Baduk) gameplay with:
- Stone placement on 9×9 board
- Pass mechanics and game termination
- Basic ASCII board rendering
- Area scoring

**Run:** `dotnet run --project GoDemo`

See [GoDemo/README.md](GoDemo/README.md) for details.

### CardGameDemo

Shows basic card operations using the Cards module:
- Deck initialization and deterministic shuffling
- Drawing and discarding cards
- Peeking at cards without state change
- Revealing cards to players
- Automatic reshuffle when draw pile empties
- Deterministic RNG for reproducible gameplay

**Run:** `dotnet run --project CardGameDemo`

See [CardGameDemo/README.md](CardGameDemo/README.md) for details.

### SimulationDemo

Demonstrates Monte Carlo simulation capabilities:
- Random game playouts
- Statistical analysis
- Performance metrics
- Parallel execution

**Run:** `dotnet run --project SimulationDemo`

See [SimulationDemo/README.md](SimulationDemo/README.md) for details.

### RiskDemo

Demonstrates Risk-style territory conquest gameplay with:
- Territory graph with 24 territories across 4 continents
- Reinforcement calculation (territories/3 + continent bonuses)
- Multi-dice combat resolution (attacker 1-3 dice vs defender 1-2 dice)
- Conquest mechanics with army movement
- Fortification between connected territories
- Phase-based turn structure (Reinforce → Attack → Fortify)

**Run:** `dotnet run --project RiskDemo`

## Running All Demos

From the repository root:

```bash
# Run all demos sequentially
dotnet run --project samples/ChessDemo
dotnet run --project samples/GoDemo
dotnet run --project samples/CardGameDemo
dotnet run --project samples/SimulationDemo
dotnet run --project samples/RiskDemo
```

## Creating Your Own Demo

1. Create a new console project in `samples/`:
   ```bash
   dotnet new console -n MyGameDemo
   ```

2. Add reference to your game module:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\..\src\Veggerby.Boards\Veggerby.Boards.csproj" />
     <ProjectReference Include="..\..\src\Veggerby.Boards.MyGame\Veggerby.Boards.MyGame.csproj" />
   </ItemGroup>
   ```

3. Implement `Program.cs` following patterns from existing demos

4. Add to solution:
   ```bash
   dotnet sln add samples/MyGameDemo/MyGameDemo.csproj
   ```

## Demo Patterns

All demos follow these conventions:

- **Build Target**: `net9.0`
- **Output**: Console with clear section headers
- **Error Handling**: Try-catch blocks with descriptive messages
- **Documentation**: README.md with overview and sample output
- **Rendering**: ASCII art for board visualization (when applicable)
- **Determinism**: Seeded RNG for reproducible results

## References

- [GameBuilder Guide](../docs/gamebuilder-guide.md) - How to create game modules
- [Core Concepts](../docs/core-concepts.md) - Understanding the engine
- [Module Documentation](../docs/README.md) - All available modules
