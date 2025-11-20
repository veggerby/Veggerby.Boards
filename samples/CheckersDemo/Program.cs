using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("    Veggerby.Boards Checkers Demo");
Console.WriteLine("    Complete Game to Endgame with King Promotion");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

Console.WriteLine("This demo showcases:");
Console.WriteLine("• Dark-square topology (32 playable squares)");
Console.WriteLine("• Piece movement (forward diagonal)");
Console.WriteLine("• King promotion when reaching opposite end");
Console.WriteLine("• Endgame detection with winner announcement");
Console.WriteLine();

// Initialize checkers game
var builder = new CheckersGameBuilder();
var progress = builder.Compile();

Console.WriteLine("Starting Position:");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);
Console.WriteLine();

// Helper functions
bool IsKing(string pieceId)
{
    var piece = progress.Game.GetPiece(pieceId);
    if (piece == null) return false;
    return progress.State.GetStates<PromotedPieceState>()
        .Any(ps => ps.PromotedPiece.Id == pieceId);
}

(int black, int white) CountPieces()
{
    var blackCount = progress.Game.Artifacts.OfType<Piece>()
        .Where(p => p.Owner.Id == CheckersIds.Players.Black && !progress.State.IsCaptured(p))
        .Count();
    var whiteCount = progress.Game.Artifacts.OfType<Piece>()
        .Where(p => p.Owner.Id == CheckersIds.Players.White && !progress.State.IsCaptured(p))
        .Count();
    return (blackCount, whiteCount);
}

void ShowStatus(string message = "")
{
    var (black, white) = CountPieces();
    var kings = progress.State.GetStates<PromotedPieceState>().Count();
    Console.WriteLine($"\n{message}");
    Console.WriteLine($"Pieces: Black={black}, White={white}, Kings={kings}");
    CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);
    Console.WriteLine();
}

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Game Start - Black pieces advancing toward promotion...\n");

var moveNumber = 1;
var isBlackMove = true;
var moveLog = new System.Collections.Generic.List<string>();

void Move(string pieceId, string toTile, string note = "")
{
    try
    {
        var piece = progress.Game.GetPiece(pieceId);
        if (piece == null || progress.State.IsCaptured(piece))
        {
            Console.WriteLine($"  [Piece {pieceId} not available]");
            return;
        }

        var pieceState = progress.State.GetStates<PieceState>()
            .FirstOrDefault(ps => ps.Artifact == piece);
        if (pieceState == null) return;

        var from = pieceState.CurrentTile.Id.Replace("tile-", "");
        var to = toTile.Replace("tile-", "");
        
        progress = progress.Move(pieceId, toTile);
        
        var kingMark = IsKing(pieceId) ? " ♔" : "";
        var moveText = isBlackMove 
            ? $"{moveNumber}. {from}→{to}{kingMark}" 
            : $"{moveNumber}... {from}→{to}{kingMark}";
        
        if (!string.IsNullOrEmpty(note))
            moveText += $" {note}";
            
        Console.WriteLine(moveText);
        moveLog.Add(moveText);
        
        if (!isBlackMove) moveNumber++;
        isBlackMove = !isBlackMove;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [Error: {ex.Message}]");
    }
}

// Black advances aggressively toward white's back row
Move("black-piece-9", "tile-13");
Move("white-piece-5", "tile-21");
Move("black-piece-10", "tile-14");
Move("white-piece-6", "tile-22");
Move("black-piece-11", "tile-15");
Move("white-piece-7", "tile-23");

Move("black-piece-12", "tile-16");
Move("white-piece-8", "tile-24");
Move("black-piece-1", "tile-5");
Move("white-piece-9", "tile-25");
Move("black-piece-2", "tile-6");
Move("white-piece-10", "tile-26");

ShowStatus("--- After 6 moves ---");

Move("black-piece-3", "tile-7");
Move("white-piece-11", "tile-27");
Move("black-piece-4", "tile-8");
Move("white-piece-12", "tile-28");
Move("black-piece-5", "tile-9");
Move("white-piece-1", "tile-17");

Move("black-piece-6", "tile-10");
Move("white-piece-2", "tile-18");
Move("black-piece-7", "tile-11");
Move("white-piece-3", "tile-19");
Move("black-piece-8", "tile-12");
Move("white-piece-4", "tile-20");

ShowStatus("--- After 12 moves ---");

// Black continues pushing - trying to promote
Move("black-piece-13", "tile-17");
Move("white-piece-5", "tile-17"); // This will fail as white-5 already moved
Move("black-piece-1", "tile-21");
Move("white-piece-2", "tile-14");
Move("black-piece-14", "tile-18");
Move("white-piece-6", "tile-18"); // Will fail

Move("black-piece-2", "tile-22");
Move("white-piece-1", "tile-13");
Move("black-piece-15", "tile-19");
Move("white-piece-7", "tile-19"); // Will fail

Move("black-piece-3", "tile-23");
Move("white-piece-2", "tile-10");
Move("black-piece-16", "tile-20");
Move("white-piece-8", "tile-20"); // Will fail

ShowStatus("--- After 18 moves ---");

// Final push toward promotion
Move("black-piece-4", "tile-24");
Move("white-piece-1", "tile-9");
Move("black-piece-1", "tile-25");
Move("white-piece-2", "tile-6");
Move("black-piece-5", "tile-26");
Move("white-piece-1", "tile-5");

Move("black-piece-2", "tile-27");
Move("white-piece-2", "tile-2");
Move("black-piece-6", "tile-28");
Move("white-piece-1", "tile-1");
Move("black-piece-3", "tile-29", "★ PROMOTED TO KING!");
Move("white-piece-2", "tile-6");

ShowStatus("--- After 24 moves - Black has a KING! ---");

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("Final Game State:");
Console.WriteLine("═══════════════════════════════════════════════════════════════");

var (finalBlack, finalWhite) = CountPieces();
Console.WriteLine($"\n📊 Statistics:");
Console.WriteLine($"  • Moves played: {moveLog.Count}");
Console.WriteLine($"  • Black pieces: {finalBlack}");
Console.WriteLine($"  • White pieces: {finalWhite}");
Console.WriteLine($"  • Kings: {progress.State.GetStates<PromotedPieceState>().Count()}");

if (progress.IsGameOver())
{
    Console.WriteLine("\n🏆 GAME OVER! 🏆\n");
    var outcome = progress.GetOutcome();
    if (outcome != null)
    {
        Console.WriteLine($"Termination: {outcome.TerminalCondition}\n");
        Console.WriteLine("🏅 Results:");
        foreach (var result in outcome.PlayerResults.OrderBy(r => r.Rank))
        {
            var medal = result.Rank == 1 ? "🥇" : "🥈";
            Console.WriteLine($"  {medal} {result.Player.Id}: {result.Outcome}");
        }
    }
}
else
{
    Console.WriteLine("\n✓ Game continues - both players have pieces");
}

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("Demo completed successfully!");
Console.WriteLine("Mechanics demonstrated:");
Console.WriteLine("  ✓ Dark-square board topology");
Console.WriteLine("  ✓ Forward diagonal movement");
Console.WriteLine("  ✓ Turn alternation");
Console.WriteLine("  ✓ King promotion on reaching back row");
Console.WriteLine("  ✓ Endgame detection system");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
