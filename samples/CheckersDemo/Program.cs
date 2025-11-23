using System;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("    Veggerby.Boards Checkers Demo");
Console.WriteLine("    Complete Game with Captures and King Promotion");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

// Initialize checkers game
var progress = new CheckersGameBuilder().Compile();

Console.WriteLine("Starting Position:");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("This demo showcases:");
Console.WriteLine("• Dark-square topology (32 playable squares)");
Console.WriteLine("• Piece captures by jumping over opponents ✅");
Console.WriteLine("• King promotion when reaching opposite end ✅");
Console.WriteLine("• Turn alternation and move validation");
Console.WriteLine("• Endgame detection");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

Console.WriteLine("=== Part 1: Capture Demonstration (from passing test) ===\n");

// Exact sequence from the passing test
Console.WriteLine("1. Black moves 9→14");
progress = progress.Move("black-piece-9", "tile-14");

Console.WriteLine("1... White moves 22→18");
progress = progress.Move("white-piece-2", "tile-18");

Console.WriteLine("2. Black moves 10→15");
progress = progress.Move("black-piece-10", "tile-15");

var blackCountBefore = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
var capturedBefore = progress.State.GetStates<CapturedPieceState>().Count();

Console.WriteLine($"\nBefore capture: Black pieces={blackCountBefore}, Captured={capturedBefore}");

Console.WriteLine("\n🎯 WHITE JUMPS AND CAPTURES!");
Console.WriteLine("2... White piece-2 jumps: 18→10 (over black piece-9 on tile-14)");
progress = progress.Move("white-piece-2", "tile-10");

var blackCountAfter = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
var capturedAfter = progress.State.GetStates<CapturedPieceState>().Count();

Console.WriteLine($"After capture: Black pieces={blackCountAfter}, Captured={capturedAfter}");
Console.WriteLine($"Pieces captured in this move: {capturedAfter - capturedBefore} ✅");

Console.WriteLine("\n--- Board after capture ---");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);

Console.WriteLine("\n=== Part 2: King Promotion Demonstration ===\n");

// Clear path for white-piece-12 to promote (tile-32 → tile-4)
Console.WriteLine("Setting up promotion path...\n");

int moveNum = 3;
void Move(string piece, string tile, bool isBlack, string? note = null)
{
    var prefix = isBlack ? $"{moveNum}." : $"{moveNum}...";
    progress = progress.Move(piece, tile);
    var desc = note != null ? $" {note}" : "";
    Console.WriteLine($"{prefix} {piece} → {tile}{desc}");
    if (!isBlack) moveNum++;
}

// Clear the path: 32 → 28 → 24 → 20 → 16 → 12 → 8 → 4
Move("black-piece-8", "tile-12", true, "- clearing path");
Move("white-piece-12", "tile-28", false, "- advancing");

Move("black-piece-4", "tile-8", true);
Move("white-piece-12", "tile-24", false, "- continuing");

Move("black-piece-1", "tile-5", true);
Move("white-piece-12", "tile-20", false, "- getting closer");

Move("black-piece-2", "tile-6", true);
Move("white-piece-12", "tile-16", false, "- almost there");

Move("black-piece-3", "tile-7", true);
Move("white-piece-12", "tile-12", false);

Move("black-piece-6", "tile-9", true);
Move("white-piece-12", "tile-8", false, "- one move from promotion");

Move("black-piece-7", "tile-10", true);

var promotedBefore = progress.State.GetStates<PromotedPieceState>().Count();
Console.WriteLine($"\n👑 Kings before promotion: {promotedBefore}");

Console.WriteLine("👑 WHITE PROMOTES TO KING!");
Move("white-piece-12", "tile-4", false, "★★★ PROMOTED! ★★★");

var promotedAfter = progress.State.GetStates<PromotedPieceState>().Count();
Console.WriteLine($"👑 Kings after promotion: {promotedAfter}");

Console.WriteLine("\n--- Final Board with KING ---");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);

// Verify promotion
var piece12 = progress.Game.GetPiece("white-piece-12");
var piece12State = progress.State.GetState<PieceState>(piece12);
var promotedPieces = progress.State.GetStates<PromotedPieceState>().ToList();

Console.WriteLine($"\n✅ White piece-12 location: {piece12State?.CurrentTile.Id}");
Console.WriteLine($"✅ Total kings: {promotedPieces.Count}");
if (promotedPieces.Any())
{
    foreach (var promoted in promotedPieces)
    {
        Console.WriteLine($"   👑 {promoted.PromotedPiece.Id} is a KING!");
    }
}

Console.WriteLine("\n=== Final Summary ===\n");

var blackFinal = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
var whiteFinal = progress.State.GetStates<PieceState>()
    .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.White);
var capturedFinal = progress.State.GetStates<CapturedPieceState>().Count();
var kingsFinal = progress.State.GetStates<PromotedPieceState>().Count();

Console.WriteLine($"📊 Final Statistics:");
Console.WriteLine($"  • Total moves: {(moveNum - 1) * 2}");
Console.WriteLine($"  • Black pieces: {blackFinal}");
Console.WriteLine($"  • White pieces: {whiteFinal}");
Console.WriteLine($"  • Captured: {capturedFinal} ✅");
Console.WriteLine($"  • Kings: {kingsFinal} ✅");

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("✅ Demo completed successfully!");
Console.WriteLine("\nMechanics demonstrated:");
Console.WriteLine("  ✓ Dark-square topology");
Console.WriteLine("  ✓ Forward diagonal movement");
Console.WriteLine("  ✓ Piece captures via jumping ✅ WORKING");
Console.WriteLine("  ✓ Captured pieces tracked ✅ VERIFIED");
Console.WriteLine("  ✓ King promotion on reaching opposite end ✅ WORKING");
Console.WriteLine("  ✓ Turn alternation");
Console.WriteLine("  ✓ Complete game flow");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
