# Veggerby.Boards.Othello

Othello/Reversi implementation for the Veggerby.Boards game engine.

## Overview

Othello (also known as Reversi) is a classic two-player strategy board game. Players place discs on an 8×8 board, flipping opponent discs when they sandwich them between two of their own discs.

## Rules

- Two players: Black and White
- Black moves first
- Players alternate placing discs on empty squares
- A move must flip at least one opponent disc
- Flipping occurs when a newly placed disc creates a straight line (horizontal, vertical, or diagonal) of opponent discs bounded by the new disc and another disc of the same color
- All sandwiched opponent discs are flipped to the player's color
- If a player has no valid move, they must pass
- Game ends when the board is full or neither player can move
- Player with the most discs wins

## Starting Position

```
   a b c d e f g h
8  · · · · · · · ·
7  · · · · · · · ·
6  · · · · · · · ·
5  · · · ○ ● · · ·
4  · · · ● ○ · · ·
3  · · · · · · · ·
2  · · · · · · · ·
1  · · · · · · · ·
```

## Usage

```csharp
using Veggerby.Boards.Othello;

var builder = new OthelloGameBuilder();
var progress = builder.Compile();

// Black places first disc at c4
var blackDisc = progress.Game.GetPiece("black-disc-3");
var targetTile = progress.Game.GetTile("c4");
progress = progress.HandleEvent(new PlacePieceGameEvent(blackDisc, targetTile));

// White responds at c5
var whiteDisc = progress.Game.GetPiece("white-disc-3");
targetTile = progress.Game.GetTile("c5");
progress = progress.HandleEvent(new PlacePieceGameEvent(whiteDisc, targetTile));

// Display the board
OthelloBoardRenderer.Write(progress.Game, progress.State, Console.Out);
```

## Key Components

### OthelloGameBuilder
Main builder class that configures the 8×8 board, creates discs, and sets up game rules.

### Conditions
- **ValidPlacementCondition** - Ensures a disc placement flips at least one opponent disc
- **OthelloEndgameCondition** - Detects when the game has ended
- **GameNotEndedCondition** - Prevents moves after game termination

### Mutators
- **FlipDiscsStateMutator** - Flips opponent discs when a disc is placed
- **OthelloEndGameMutator** - Counts discs and determines the winner

### State
- **FlippedDiscState** - Tracks disc flips without modifying piece artifacts
- **OthelloOutcomeState** - Records final scores and winner

### Helper
- **OthelloHelper** - Utility methods for determining current disc colors

## Implementation Details

Disc flipping is implemented using `FlippedDiscState` which tracks color changes without mutating the original piece artifacts. The current color of a disc is determined by counting flip states: an odd number of flips means the disc is the opposite of its original color.

This approach maintains the engine's immutability principles while efficiently representing the dynamic nature of Othello discs.

## See Also

- [Core Concepts](../../docs/core-concepts.md)
- [Game Builder Guide](../../docs/gamebuilder-guide.md)
- [Phase-Based Design](../../docs/phase-based-design.md)
