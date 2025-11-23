# Ludo/Parcheesi Module

The Ludo module demonstrates a classic race-to-finish board game with dice-driven movement, entry mechanics, capture rules, and safe squares. It showcases deterministic gameplay suitable for AI development and serves as an excellent tutorial example for dice-based race games.

## Game Overview

Ludo (also known as Parcheesi in the US) is a race game where 2-4 players compete to be the first to move all their pieces around the board and into their home stretch.

### Key Features

- **Circular Track**: 52-square main track (13 squares per player)
- **Entry Mechanics**: Roll a 6 to enter pieces from base onto the board
- **Dice-Driven Movement**: Single 6-sided die determines movement distance
- **Capture**: Landing on opponent pieces sends them back to base
- **Safe Squares**: Starting positions where capture is not allowed
- **Home Stretch**: Player-specific 5-square path to victory
- **Exact Finish**: Must roll exact count to reach final home square
- **Bonus Turn**: Rolling a 6 grants an extra turn

## Quick Start

See samples/LudoDemo/Program.cs for a complete deterministic game example.

## Related Documentation

- [Turn Sequencing](../turn-sequencing.md)
- [Core Concepts](../core-concepts.md)
- [Game Builder Guide](../gamebuilder-guide.md)
