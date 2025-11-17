# Go Demo

Demonstration of the Veggerby.Boards.Go module showing basic stone placement, pass mechanics, and game termination.

## Overview

This demo shows:

- **Stone Placement**: Playing stones on empty intersections
- **Board Rendering**: Simple ASCII representation of the board state
- **Pass Mechanics**: Two consecutive passes end the game
- **Score Calculation**: Area scoring (stones + territory)
- **Rule Enforcement**: Capture detection, suicide rule, ko rule

## Running the Demo

From the repository root:

```bash
dotnet run --project samples/GoDemo
```

## What It Demonstrates

### ✅ Working Features

- Stone placement validation (empty intersections only)
- Turn rotation between black and white
- Pass turn mechanics with consecutive pass tracking
- Game termination after two consecutive passes
- Area scoring calculation
- Basic ASCII board rendering with Unicode stones (● ○)

### Implementation Highlights

- **GroupScanner**: Flood-fill algorithm for liberty counting and group detection
- **PlaceStoneStateMutator**: Handles capture mechanics, suicide rule validation, and ko enforcement
- **PassTurnStateMutator**: Tracks consecutive passes and triggers game end
- **GoScoring.AreaScore**: Deterministic territory and stone counting

### ⚠️ Known Limitations

The demo intentionally shows a simplified game to focus on core mechanics:

- Uses 9×9 board instead of full 19×19 for clarity
- Simplified opening sequence (not a real professional game)
- No visualization of specific captures (though capture logic works)
- Superko (full-board repetition) not enforced
- Dead stone adjudication requires manual play-out

## Sample Output

```
═══════════════════════════════════════════════════════════════
    Veggerby.Boards Go Demo
    Lee Sedol vs. AlphaGo - Match 4, Move 78 (2016)
    Demonstrating the legendary 'God Move'
═══════════════════════════════════════════════════════════════

Starting Position (9×9 board):
   A B C D E F G H J 
 9 · · · · · · · · · 
 8 · · · · · · · · · 
 7 · · + · + · + · · 
 6 · · · · · · · · · 
 5 · · + · + · + · · 
 4 · · · · · · · · · 
 3 · · + · + · + · · 
 2 · · · · · · · · · 
 1 · · · · · · · · · 

Playing demonstration game (simplified opening)...

Move 1: black plays C3
Move 2: white plays G7
Move 3: black plays G3
Move 4: white plays C7
Move 5: black plays E5

   A B C D E F G H J 
 9 · · · · · · · · · 
 8 · · · · · · · · · 
 7 · · ○ · · · ● · · 
 6 · · · · · · · · · 
 5 · · · · ● · · · · 
 4 · · · · · · · · · 
 3 · · ● · · · ● · · 
 2 · · · · · · · · · 
 1 · · · · · · · · · 

...

--- Demonstrating Pass Mechanics ---
Both players pass to end the game...

Black passes.
White passes.

✓ Game ended after two consecutive passes.

Final Score:
  Black: 45 points
  White: 36 points
  Winner: Black
```

## Understanding the Code

### Stone Placement

```csharp
var tile = progress.Game.GetTile($"tile-{x}-{y}");
var stone = GetAvailableStone(progress, colorId);
progress = progress.HandleEvent(new PlaceStoneGameEvent(stone, tile));
```

### Pass Turn

```csharp
progress = progress.HandleEvent(new PassTurnGameEvent());
```

### Check Game Status

```csharp
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome() as GoOutcomeState;
    Console.WriteLine($"Black: {outcome.BlackScore}, White: {outcome.WhiteScore}");
}
```

## Extending the Demo

You can modify `Program.cs` to:

1. **Use 19×19 board**: Change `GoGameBuilder(size: 19)`
2. **Add capture scenarios**: Create positions where captures occur
3. **Test ko rule**: Create a ko situation and attempt immediate recapture
4. **Test suicide rule**: Try placing a stone that would have zero liberties without capturing

## References

- [Go Module README](../../src/Veggerby.Boards.Go/README.md) - Complete module documentation
- [Go Rules](https://senseis.xmp.net/) - Sensei's Library for Go rules reference
- [GameBuilder Guide](../../docs/gamebuilder-guide.md) - How to build game modules

## About Lee Sedol's "God Move"

The real Lee Sedol vs. AlphaGo Match 4, Move 78 (March 13, 2016) was a stunning moment in Go history. Lee Sedol found a brilliant tesuji (tactical move) that AlphaGo had calculated as having only a 1-in-10,000 probability. This move demonstrated the continued creative potential of human play even against superhuman AI.

This demo uses a simplified game on a smaller board for practical demonstration purposes, but the underlying engine supports full 19×19 professional games.
