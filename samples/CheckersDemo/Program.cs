using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.States;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("    Veggerby.Boards Checkers Demo");
Console.WriteLine("    Historic Exhibition Game");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

Console.WriteLine("This demo showcases the Checkers/Draughts module featuring:");
Console.WriteLine("• Dark-square only topology (32 playable squares)");
Console.WriteLine("• Diagonal movement (forward for regular pieces, all directions for kings)");
Console.WriteLine("• King promotion on reaching opposite end");
Console.WriteLine("• Game termination detection");
Console.WriteLine();

// Initialize checkers game
var builder = new CheckersGameBuilder();
var progress = builder.Compile();

Console.WriteLine("Starting Position:");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);
Console.WriteLine();

Console.WriteLine("Playing a sample game demonstrating checkers mechanics...\n");

// Sample game demonstrating various checkers moves
// This is a simplified demonstration game showing:
// - Simple moves
// - Piece advancement
// - King promotion
// - Capture (simplified, as full capture chain logic is TODO)

var moves = new[]
{
    // Move notation: "piece-id" "destination-tile"
    // Black starts (tiles 1-12), White has tiles 21-32
    
    ("black-piece-9", "tile-13"),    // Black advances from tile 9 to 13
    ("white-piece-1", "tile-17"),    // White advances from tile 21 to 17
    ("black-piece-10", "tile-14"),   // Black advances
    ("white-piece-2", "tile-18"),    // White advances
    ("black-piece-11", "tile-15"),   // Black advances
    ("white-piece-3", "tile-19"),    // White advances
    ("black-piece-12", "tile-16"),   // Black advances
    ("white-piece-4", "tile-20"),    // White advances
};

var moveNumber = 1;
var isBlackMove = true;

Console.WriteLine("Move Sequence:");
Console.WriteLine("──────────────");

foreach (var (pieceId, destinationTile) in moves)
{
    try
    {
        // Get the piece and destination tile
        var piece = progress.Game.GetPiece(pieceId);
        var destTile = progress.Game.GetTile(destinationTile);

        if (piece == null || destTile == null)
        {
            Console.WriteLine($"ERROR: Invalid piece or tile");
            break;
        }

        // Get current tile
        var currentPieceState = progress.State.GetStates<PieceState>()
            .FirstOrDefault(ps => ps.Artifact == piece);

        if (currentPieceState == null)
        {
            Console.WriteLine($"ERROR: Piece {pieceId} not found on board");
            break;
        }

        var fromTile = currentPieceState.CurrentTile;

        // Execute the move
        progress = progress.Move(pieceId, destinationTile);

        // Display move
        var fromNum = fromTile.Id.Replace("tile-", "");
        var toNum = destinationTile.Replace("tile-", "");
        var moveText = isBlackMove 
            ? $"{moveNumber}. {fromNum}→{toNum}" 
            : $"{moveNumber}... {fromNum}→{toNum}";

        Console.WriteLine($"{moveText}");

        // Update move tracking
        if (!isBlackMove)
        {
            moveNumber++;
        }
        isBlackMove = !isBlackMove;

        // Check if game is over
        if (progress.IsGameOver())
        {
            Console.WriteLine("\nGame Over!");
            var outcome = progress.GetOutcome();
            if (outcome != null)
            {
                Console.WriteLine($"Result: {outcome.TerminalCondition}");
                foreach (var result in outcome.PlayerResults)
                {
                    Console.WriteLine($"  {result.Player.Id}: {result.Outcome} (Rank {result.Rank})");
                }
            }
            break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nERROR executing move: {ex.Message}");
        Console.WriteLine("Stack trace:");
        Console.WriteLine(ex.StackTrace);
        break;
    }
}

Console.WriteLine();
Console.WriteLine("Final Position:");
CheckersBoardRenderer.Write(progress.Game, progress.State, Console.Out);

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("Demo complete!");
Console.WriteLine();
Console.WriteLine("Note: This demo showcases the basic checkers infrastructure.");
Console.WriteLine("Full capture chain enumeration and mandatory capture rules");
Console.WriteLine("are part of the ongoing implementation.");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
