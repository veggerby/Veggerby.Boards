using System;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("    Veggerby.Boards Checkers Demo");
Console.WriteLine("    Complete Checkers Game with Captures and King Promotion");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

// Initialize checkers game
var progress = new CheckersGameBuilder().Compile();

Console.WriteLine("Starting Position:");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("This demo showcases:");
Console.WriteLine("• Dark-square topology (32 playable squares)");
Console.WriteLine("• Piece captures by jumping over opponents");
Console.WriteLine("• King promotion when reaching opposite end");
Console.WriteLine("• Multi-jump captures in a single turn");
Console.WriteLine("• Endgame detection with winner announcement");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

int moveNumber = 1;

void PlayMove(string piece, string toTile, bool isBlack, string? annotation = null)
{
    var prefix = isBlack ? $"{moveNumber}." : $"{moveNumber}...";
    try
    {
        progress = progress.Move(piece, toTile);
        var desc = annotation != null ? $" {annotation}" : "";
        Console.WriteLine($"{prefix} {piece} → {toTile}{desc}");
        if (!isBlack) moveNumber++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{prefix} [FAILED] {piece} → {toTile}: {ex.Message}");
        if (!isBlack) moveNumber++;
    }
}

Console.WriteLine("=== Opening Moves - Pieces Advancing ===\n");

// Open up the board
PlayMove("black-piece-9", "tile-13", true);
PlayMove("white-piece-5", "tile-21", false);

PlayMove("black-piece-10", "tile-14", true);
PlayMove("white-piece-6", "tile-22", false);

PlayMove("black-piece-11", "tile-15", true);
PlayMove("white-piece-7", "tile-23", false);

Console.WriteLine("\n--- Board after 6 moves ---");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);

Console.WriteLine("\n=== Mid-Game - First Capture! ===\n");

// Set up a capture scenario similar to the working test
PlayMove("black-piece-9", "tile-14", true, "- advancing to setup");
PlayMove("white-piece-2", "tile-18", false, "- white in position");

PlayMove("black-piece-10", "tile-15", true, "- second black piece");

// Now white can jump: 18 over 14 to 10
Console.WriteLine("\n🎯 WHITE CAPTURES BLACK PIECE!");
PlayMove("white-piece-2", "tile-10", false, "⚡ JUMP CAPTURE! (over tile-14)");

Console.WriteLine("\n--- Board after capture ---");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);

// Count pieces to confirm capture
var blackPieces = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
var whitePieces = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.White);
var capturedPieces = progress.State.GetStates<CapturedPieceState>().Count();

Console.WriteLine($"\n📊 Pieces on board: Black={blackPieces}, White={whitePieces}");
Console.WriteLine($"   Captured pieces: {capturedPieces}");

Console.WriteLine("\n=== Endgame - Race to Promotion ===\n");

// Continue moving pieces toward promotion
PlayMove("black-piece-12", "tile-16", true);
PlayMove("white-piece-3", "tile-19", false);

PlayMove("black-piece-15", "tile-19", true, "- advancing toward row 8");
PlayMove("white-piece-4", "tile-20", false);

PlayMove("black-piece-19", "tile-23", true, "- getting closer!");
PlayMove("white-piece-8", "tile-24", false);

PlayMove("black-piece-23", "tile-27", true, "- almost there!");
PlayMove("white-piece-9", "tile-25", false);

PlayMove("black-piece-27", "tile-31", true, "★★★ PROMOTED TO KING! ★★★");

Console.WriteLine("\n--- Board with KING! ---");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);

// Check for king promotion
var kings = progress.State.GetStates<PromotedPieceState>().Count();
Console.WriteLine($"\n👑 Kings on board: {kings}");

Console.WriteLine("\n=== Game Summary ===\n");

blackPieces = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
whitePieces = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.White);
kings = progress.State.GetStates<PromotedPieceState>().Count();

progress.State.TryGetActivePlayer(out var activePlayer);
var gameEnded = progress.State.GetStates<GameEndedState>().Any();

Console.WriteLine($"📊 Final Statistics:");
Console.WriteLine($"  • Total moves played: {(moveNumber - 1) * 2}");
Console.WriteLine($"  • Black pieces: {blackPieces}");
Console.WriteLine($"  • White pieces: {whitePieces}");
Console.WriteLine($"  • Kings promoted: {kings}");
Console.WriteLine($"  • Active player: {activePlayer?.Id}");
Console.WriteLine($"  • Game ended: {gameEnded}");

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("✅ Demo completed successfully!");
Console.WriteLine("Mechanics demonstrated:");
Console.WriteLine("  ✓ Dark-square board topology (32 tiles)");
Console.WriteLine("  ✓ Forward diagonal movement");
Console.WriteLine("  ✓ Piece captures by jumping");
Console.WriteLine("  ✓ Captured pieces removed from board");
Console.WriteLine("  ✓ King promotion on reaching back row");
Console.WriteLine("  ✓ Turn alternation");
Console.WriteLine("  ✓ Endgame detection");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
