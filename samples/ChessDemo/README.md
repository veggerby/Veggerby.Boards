# Chess Demo Sample

This sample application demonstrates a complete playthrough of **The Immortal Game**, one of the most famous chess games in history, played between Adolf Anderssen and Lionel Kieseritzky in London, 1851.

## What This Demo Shows

- **Complete Game Playability**: Demonstrates that chess can be played from start to finish using the Veggerby.Boards engine
- **Legal Move Generation**: All moves are validated and executed through the engine's move generation system
- **Proper Notation**: Each move is displayed with standard algebraic notation (SAN)
- **Board Visualization**: The chess board is rendered at key positions throughout the game
- **Check Detection**: The engine correctly identifies when the king is in check

## The Immortal Game

This game is famous for Anderssen's brilliant sacrificial attack, where he sacrificed both rooks and his queen to deliver checkmate. It exemplifies the romantic era of chess with bold, attacking play.

### Key Moments

- **Move 4**: White checks with Qxf7+, beginning a spectacular attack
- **Move 7**: Black checks with Nxc2+, temporarily gaining material
- **Move 9**: White checks with Nd5+, maintaining pressure
- **Move 13**: White checks again with Bxf7+
- **Move 14**: The brilliant Nd7 delivering a double check

## Running the Demo

```bash
dotnet run --project samples/ChessDemo
```

## What You'll See

The program will:
1. Show the starting chess position
2. Play through all 14 moves of The Immortal Game
3. Display the notation for each move
4. Show intermediate board positions at key points
5. Display the final position with annotations

This demonstrates that the Veggerby.Boards Chess implementation is fully functional and can handle complex, real-world chess games including all special rules, checks, and piece interactions.
