using System;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

// Demo of fixed deterministic mini play sequences for Chess & Backgammon using the core engine.
// This sample intentionally enables simulation + required flags explicitly.
// Simulation feature flag (internal). This demo only needs core event handling; sims are covered by tests.
// FeatureFlags.EnableSimulation = true;

Console.WriteLine("=== Veggerby.Boards Simulation Demo ===\n");
RunChessDemo();
RunBackgammonDemo();

static void RunChessDemo()
{
    Console.WriteLine("-- Chess Demo --");
    var builder = new ChessGameBuilder();
    var progress = builder.Compile();

    // Initial position
    ChessBoardRenderer.Write(progress.Game, progress.State, Console.Out);

    // Opening sequence adapted to current engine (pawns = single step only): 1. e3 e6 2. Nf3 Nf6
    var nomenclature = ResolveNomenclature(progress.Game);
    progress = ApplyMove(progress, nomenclature, "white-pawn-5", "tile-e2", "tile-e3");
    progress = ApplyMove(progress, nomenclature, "black-pawn-5", "tile-e7", "tile-e6");
    progress = ApplyMove(progress, nomenclature, "white-knight-2", "tile-g1", "tile-f3");
    progress = ApplyMove(progress, nomenclature, "black-knight-2", "tile-g8", "tile-f6");

    // Simple capture demonstration: clear e-file then queen captures a pawn
    // Preconditions: white pawn on e3 blocks queen; move it further to e4 to free e3, then advance black pawn to e5 for capture scenario.
    // (Engine currently supports single-step pawn movement only.)
    try
    {
        progress = ApplyMove(progress, nomenclature, "white-pawn-5", "tile-e3", "tile-e4");
        progress = ApplyMove(progress, nomenclature, "black-pawn-5", "tile-e6", "tile-e5");
        // Queen path e1->e5 (multi-step) to capture black pawn
        progress = ApplyMove(progress, nomenclature, "white-queen", "tile-e1", "tile-e5");
        // Show board after capture
        Console.WriteLine("Position after queen captures pawn on e5:");
        ChessBoardRenderer.Write(progress.Game, progress.State, Console.Out);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"(capture demo skipped: {ex.Message})");
    }

    Console.WriteLine();
}

static GameProgress ApplyMove(GameProgress progress, IGameNomenclature nomenclature, string pieceId, string fromTileId, string toTileId)
{
    var piece = progress.Game.GetPiece(pieceId);
    var from = progress.Game.GetTile(fromTileId);
    var to = progress.Game.GetTile(toTileId);

    if (piece == null || from == null || to == null)
    {
        Console.WriteLine($"(warn) Invalid identifiers for move {pieceId} {fromTileId}->{toTileId}");
        return progress;
    }

    // Try resolve via existing patterns (supports multi-step knight paths, sliding, etc.)
    var path = ResolvePath(progress.Game, piece, from, to);
    if (path == null)
    {
        Console.WriteLine($"(info) {nomenclature.GetPieceName(piece)} intent {nomenclature.GetTileName(from)}->{nomenclature.GetTileName(to)} (unreachable with current patterns)");
        return progress;
    }

    var evt = new MovePieceGameEvent(piece, path);
    var next = progress.HandleEvent(evt);
    var notation = nomenclature.Describe(progress.Game, progress.State, evt);
    Console.WriteLine($"Move applied: {notation}");
    return next;
}

static TilePath? ResolvePath(Game game, Piece piece, Tile from, Tile to)
{
    foreach (var pattern in piece.Patterns)
    {
        var visitor = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(game.Board, from, to);
        pattern.Accept(visitor);
        if (visitor.ResultPath is not null)
        {
            return visitor.ResultPath;
        }
    }

    // Fallback: direct relation (single step) if available
    var rel = game.Board.TileRelations.FirstOrDefault(r => r.From == from && r.To == to);
    return rel != null ? new TilePath([rel]) : null;
}

static void RunBackgammonDemo()
{
    Console.WriteLine("-- Backgammon Demo --");
    var builder = new BackgammonGameBuilder();
    var progress = builder.Compile();

    // Initial position snapshot
    BackgammonBoardRenderer.Write(progress.Game, progress.State, Console.Out);

    // For deterministic demo we emulate a dice roll event if rule set present.
    // If dice rules not wired in builder, we just print pieces & exit.
    var dice = progress.Game.GetArtifacts<Dice>().FirstOrDefault();
    if (dice == null)
    {
        Console.WriteLine("(info) Backgammon dice not present in current builder configuration.");
        return;
    }

    // Use provided helper extension for deterministic sequential values (indexes) or construct an ad-hoc event if needed.
    var nomenclature = ResolveNomenclature(progress.Game);
    var rollProgress = progress.RollDice(dice.Id);
    Console.WriteLine("Dice: " + nomenclature.GetDiceName(dice));

    // Attempt a simple piece move: pick first movable piece if direct relation exists.
    var piece = rollProgress.Game.GetArtifacts<Piece>().FirstOrDefault();
    if (piece != null)
    {
        var pieceState = rollProgress.State.GetState<PieceState>(piece);
        var rel = rollProgress.Game.Board.TileRelations.FirstOrDefault(r => r.From == pieceState.CurrentTile);
        if (rel != null)
        {
            var path = new TilePath([rel]);
            var moveEvt = new MovePieceGameEvent(piece, path);
            var next = rollProgress.HandleEvent(moveEvt);
            var newTile = next.State.GetState<PieceState>(piece).CurrentTile.Id;
            var notation = nomenclature.Describe(rollProgress.Game, rollProgress.State, moveEvt);
            Console.WriteLine($"Applied move: {notation}");
            BackgammonBoardRenderer.Write(next.Game, next.State, Console.Out);
        }
        else
        {
            Console.WriteLine($"(info) No immediate relation from starting tile for piece {piece.Id}; skipping move.");
        }
    }
    Console.WriteLine();
}

static IGameNomenclature ResolveNomenclature(Game game)
{
    var id = game.Board.Id.ToLowerInvariant();
    if (id.Contains("chess"))
    {
        return new ChessNomenclature();
    }
    if (id.Contains("backgammon"))
    {
        return new BackgammonNomenclature();
    }
    return new GenericNomenclature();
}