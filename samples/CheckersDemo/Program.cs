using System;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("    Veggerby.Boards Checkers Demo");
Console.WriteLine("    Complete Working Game Demonstration");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

// Initialize checkers game
var progress = new CheckersGameBuilder().Compile();

Console.WriteLine("Starting Position:");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Playing a series of moves demonstrating:");
Console.WriteLine("• Basic piece movement (forward diagonals)");
Console.WriteLine("• Turn alternation between black and white");
Console.WriteLine("• Board state updates");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

int moveNumber = 1;

void PlayMove(string piece, string toTile, bool isBlack)
{
    var prefix = isBlack ? $"{moveNumber}." : $"{moveNumber}...";
    progress = progress.Move(piece, toTile);
    Console.WriteLine($"{prefix} {piece} → {toTile}");
    if (!isBlack) moveNumber++;
}

// Play a simple game sequence with VALID moves
Console.WriteLine("Game Sequence:");
PlayMove("black-piece-9", "tile-13", true);   // 9→13 (SW)
PlayMove("white-piece-1", "tile-17", false);  // 21→17 (NE)

PlayMove("black-piece-10", "tile-14", true);  // 10→14 (SW)
PlayMove("white-piece-2", "tile-18", false);  // 22→18 (NW)

PlayMove("black-piece-11", "tile-15", true);  // 11→15 (SW)
PlayMove("white-piece-3", "tile-19", false);  // 23→19 (NW)

PlayMove("black-piece-12", "tile-16", true);  // 12→16 (SW)
PlayMove("white-piece-4", "tile-20", false);  // 24→20 (NW)

Console.WriteLine();
Console.WriteLine("--- Board after 8 moves ---");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);

// Count pieces
var blackPieces = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
var whitePieces = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.White);
var kings = progress.State.GetStates<PromotedPieceState>().Count();

Console.WriteLine($"\n📊 Game Statistics:");
Console.WriteLine($"  • Total moves played: {(moveNumber - 1) * 2}");
Console.WriteLine($"  • Black pieces: {blackPieces}");
Console.WriteLine($"  • White pieces: {whitePieces}");
Console.WriteLine($"  • Kings promoted: {kings}");

progress.State.TryGetActivePlayer(out var activePlayer);
Console.WriteLine($"  • Active player: {activePlayer?.Id}");

var gameEnded = progress.State.GetStates<GameEndedState>().Any();
Console.WriteLine($"  • Game ended: {gameEnded}");

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("✅ Demo completed successfully!");
Console.WriteLine("Mechanics demonstrated:");
Console.WriteLine("  ✓ Dark-square board topology (32 playable tiles)");
Console.WriteLine("  ✓ Forward diagonal movement for regular pieces");
Console.WriteLine("  ✓ Turn alternation (black/white)");
Console.WriteLine("  ✓ Board state updates after each move");
Console.WriteLine("  ✓ Valid move sequences respecting tile connections");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
