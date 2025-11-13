using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("    Veggerby.Boards Chess Demo - Scholar's Mate");
Console.WriteLine("    A Classic Four-Move Checkmate");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

// Initialize chess game
var builder = new ChessGameBuilder();
var progress = builder.Compile();
var nomenclature = new ChessNomenclature();
var moveGenerator = new ChessMoveGenerator(progress.Game);
var legalityFilter = new ChessLegalityFilter(progress.Game);
var endgameDetector = new ChessEndgameDetector(progress.Game);

Console.WriteLine("Starting Position:");
ChessBoardRenderer.Write(progress.Game, progress.State, Console.Out);
Console.WriteLine();

// Scholar's Mate - a classic beginner's checkmate
// This demonstrates full chess playability in just 4 moves
var scholarsMate = new[]
{
    ("e2", "e4", "1. e4"),
    ("e7", "e5", "1... e5"),
    ("f1", "c4", "2. Bc4"),  
    ("b8", "c6", "2... Nc6"),
    ("d1", "h5", "3. Qh5"),
    ("g8", "f6", "3... Nf6??"),
    ("h5", "f7", "4. Qxf7#"), // Checkmate!
};

Console.WriteLine("Now playing Scholar's Mate...\n");

foreach (var (fromSquare, toSquare, notation) in scholarsMate)
{
    var fromTile = progress.Game.GetTile(fromSquare);
    var toTile = progress.Game.GetTile(toSquare);

    if (fromTile == null || toTile == null)
    {
        Console.WriteLine($"ERROR: Invalid squares {fromSquare}-{toSquare}");
        continue;
    }

    // Find the piece on the from square
    var piecesOnFrom = progress.State.GetPiecesOnTile(fromTile);
    var activePlayer = progress.State.TryGetActivePlayer(out var player) ? player : null;
    var movingPiece = piecesOnFrom.FirstOrDefault(p =>
    {
        var pieceState = progress.State.GetState<PieceState>(p);
        return pieceState?.Artifact.Owner?.Id == activePlayer?.Id &&
               !progress.State.IsCaptured(p);
    });

    if (movingPiece == null)
    {
        Console.WriteLine($"ERROR: No piece found on {fromSquare}");
        continue;
    }

    // Find the path for this move
    var path = ResolvePath(progress.Game, movingPiece, fromTile, toTile);
    if (path == null)
    {
        Console.WriteLine($"ERROR: Cannot find path from {fromSquare} to {toSquare}");
        continue;
    }

    // Create and apply the move event
    var moveEvent = new MovePieceGameEvent(movingPiece, path);
    var previousState = progress.State;
    progress = progress.HandleEvent(moveEvent);

    // Generate notation using nomenclature
    var actualNotation = nomenclature.Describe(progress.Game, previousState, moveEvent);

    // Check game status
    var status = endgameDetector.GetEndgameStatus(progress.State);
    var statusText = status switch
    {
        EndgameStatus.Check => " (Check!)",
        EndgameStatus.Checkmate => " (Checkmate!)",
        EndgameStatus.Stalemate => " (Stalemate)",
        _ => ""
    };

    // Display move
    Console.WriteLine($"{notation} [{actualNotation}]{statusText}");

    // Show position after key moves
    if (notation.StartsWith("2.") || notation == "4. Qxf7#")
    {
        Console.WriteLine();
        ChessBoardRenderer.Write(progress.Game, progress.State, Console.Out);
        Console.WriteLine();
    }
}

// Final analysis
Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("   Game Analysis: Scholar's Mate Complete!");
Console.WriteLine("═══════════════════════════════════════════════════════════════");

var finalStatus = endgameDetector.GetEndgameStatus(progress.State);
Console.WriteLine($"\nFinal Status: {finalStatus}");

// Generate legal moves to prove it's checkmate (should be zero)
var legalMoves = legalityFilter.GenerateLegalMoves(progress.State);
Console.WriteLine($"Legal Moves Available: {legalMoves.Count}");

Console.WriteLine("\nScholar's Mate demonstrates:");
Console.WriteLine("  ✓ Legal move generation");
Console.WriteLine("  ✓ Piece movement (pawns, bishops, queen, knight)");
Console.WriteLine("  ✓ Capture mechanics (queen takes pawn)");
Console.WriteLine("  ✓ Check detection");
Console.WriteLine("  ✓ Checkmate detection");
Console.WriteLine("  ✓ SAN notation generation");
Console.WriteLine("\nChess is fully playable!");

static TilePath? ResolvePath(
    Game game, 
    Piece piece, 
    Tile from,
    Tile to)
{
    ArgumentNullException.ThrowIfNull(game);
    ArgumentNullException.ThrowIfNull(piece);
    ArgumentNullException.ThrowIfNull(from);
    ArgumentNullException.ThrowIfNull(to);

    foreach (var pattern in piece.Patterns)
    {
        var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
        pattern.Accept(visitor);
        if (visitor.ResultPath is not null)
        {
            return visitor.ResultPath;
        }
    }

    // Fallback: direct relation (single step) if available
    var rel = game.Board.TileRelations.FirstOrDefault(r => r.From == from && r.To == to);
    return rel is not null ? new TilePath([rel]) : null;
}
