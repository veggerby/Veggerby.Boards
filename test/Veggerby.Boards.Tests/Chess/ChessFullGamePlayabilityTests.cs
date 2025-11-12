using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Demonstrates that chess is fully playable from start to finish with legal moves and endgame detection.
/// </summary>
public class ChessFullGamePlayabilityTests
{
    [Fact]
    public void GivenNewChessGame_WhenPlayingScholarsMate_ThenGameEndsInCheckmate()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var detector = new ChessEndgameDetector(game);
        var filter = new ChessLegalityFilter(game);

        // act - Play Scholar's Mate: 1. e4 e5 2. Qh5 Nc6 3. Bc4 Nf6?? 4. Qxf7#

        // Move 1: White e4
        var whiteLegalMoves = filter.GenerateLegalMoves(progress.State);
        whiteLegalMoves.Count.Should().Be(20, "White should have 20 legal opening moves");
        progress = progress.Move(WhitePawn5, "e4");
        detector.IsCheckmate(progress.State).Should().BeFalse("Game should not be over after move 1");

        // Move 1: Black e5
        var blackLegalMoves = filter.GenerateLegalMoves(progress.State);
        blackLegalMoves.Count.Should().BeGreaterThan(0, "Black should have legal moves");
        progress = progress.Move(BlackPawn5, "e5");
        detector.IsCheckmate(progress.State).Should().BeFalse("Game should not be over after move 1");

        // Move 2: White Qh5
        progress = progress.Move(WhiteQueen, "h5");
        detector.IsCheckmate(progress.State).Should().BeFalse("Game should not be over after move 2");

        // Move 2: Black Nc6
        progress = progress.Move(BlackKnight1, "c6");
        detector.IsCheckmate(progress.State).Should().BeFalse("Game should not be over after move 2");

        // Move 3: White Bc4
        progress = progress.Move(WhiteBishop2, "c4");
        detector.IsCheckmate(progress.State).Should().BeFalse("Game should not be over after move 3");

        // Move 3: Black Nf6?? (blunder)
        progress = progress.Move(BlackKnight2, "f6");
        detector.IsCheckmate(progress.State).Should().BeFalse("Game should not be over after move 3");

        // Move 4: White Qxf7# (checkmate!)
        progress = progress.Move(WhiteQueen, "f7");

        // assert
        detector.IsCheckmate(progress.State).Should().BeTrue("Black should be in checkmate");
        detector.IsGameOver(progress.State).Should().BeTrue("Game should be over");
        
        var endgameStatus = detector.GetEndgameStatus(progress.State);
        endgameStatus.Should().Be(EndgameStatus.Checkmate, "Status should be checkmate");

        // Black should have no legal moves
        var blackFinalMoves = filter.GenerateLegalMoves(progress.State);
        blackFinalMoves.Count.Should().Be(0, "Black should have no legal moves in checkmate");
    }

    [Fact]
    public void GivenChessGame_WhenGeneratingLegalMoves_ThenOnlyLegalMovesAreReturned()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var generator = new ChessMoveGenerator(game);
        var filter = new ChessLegalityFilter(game);

        // act
        var pseudoLegalMoves = generator.Generate(progress.State);
        var legalMoves = filter.GenerateLegalMoves(progress.State);

        // assert
        // In starting position, all pseudo-legal moves are legal
        legalMoves.Count.Should().Be(pseudoLegalMoves.Count);
        legalMoves.Count.Should().Be(20, "Starting position has 20 legal moves");

        // All moves should be valid for their piece types
        foreach (var move in legalMoves)
        {
            move.Piece.Should().NotBeNull("Each move should have a piece");
            move.From.Should().NotBeNull("Each move should have a source tile");
            move.To.Should().NotBeNull("Each move should have a destination tile");
        }
    }

    [Fact]
    public void GivenChessGame_WhenCheckingGameComponents_ThenAllComponentsAreFunctional()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var state = progress.State;

        // act & assert

        // 1. Move generation works
        var generator = new ChessMoveGenerator(game);
        var moves = generator.Generate(state);
        moves.Should().NotBeEmpty("Move generator should produce moves");

        // 2. Legality filter works
        var filter = new ChessLegalityFilter(game);
        var legalMoves = filter.GenerateLegalMoves(state);
        legalMoves.Should().NotBeEmpty("Legality filter should produce legal moves");

        // 3. Endgame detection works
        var detector = new ChessEndgameDetector(game);
        var isCheckmate = detector.IsCheckmate(state);
        var isStalemate = detector.IsStalemate(state);
        var status = detector.GetEndgameStatus(state);
        
        isCheckmate.Should().BeFalse("Starting position is not checkmate");
        isStalemate.Should().BeFalse("Starting position is not stalemate");
        status.Should().Be(EndgameStatus.InProgress, "Game should be in progress");

        // 4. Piece identification works
        var whitePawn = game.GetPiece(WhitePawn1).EnsureNotNull();
        ChessPiece.IsPawn(state, whitePawn.Id).Should().BeTrue("White pawn should be identified as pawn");
        ChessPiece.IsWhite(state, whitePawn.Id).Should().BeTrue("White pawn should be identified as white");

        var blackKing = game.GetPiece(BlackKing).EnsureNotNull();
        ChessPiece.IsKing(state, blackKing.Id).Should().BeTrue("Black king should be identified as king");
        ChessPiece.IsBlack(state, blackKing.Id).Should().BeTrue("Black king should be identified as black");

        // 5. Notation works
        var nomenclature = new ChessNomenclature();
        var pawnName = nomenclature.GetPieceName(whitePawn);
        pawnName.Should().Be("P", "White pawn should have correct name");
        
        var tileName = nomenclature.GetTileName(game.GetTile("e2").EnsureNotNull());
        tileName.Should().Be("e2", "Tile should have correct name");
    }

    [Fact]
    public void GivenChessGame_WhenPlayingMultipleMoves_ThenGameStateUpdatesCorrectly()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;

        // act
        progress = progress.Move(WhitePawn5, "e4");
        progress = progress.Move(BlackPawn5, "e5");
        progress = progress.Move(WhiteKnight2, "f3");
        progress = progress.Move(BlackKnight2, "f6");

        // assert
        var whitePawn = game.GetPiece(WhitePawn5).EnsureNotNull();
        var whitePawnState = progress.State.GetState<PieceState>(whitePawn);
        whitePawnState.Should().NotBeNull("White pawn should have a state");
        whitePawnState!.CurrentTile.Id.Should().Be("tile-e4", "White pawn should be on e4");

        var blackKnight = game.GetPiece(BlackKnight2).EnsureNotNull();
        var blackKnightState = progress.State.GetState<PieceState>(blackKnight);
        blackKnightState.Should().NotBeNull("Black knight should have a state");
        blackKnightState!.CurrentTile.Id.Should().Be("tile-f6", "Black knight should be on f6");

        // Game should still be in progress
        var detector = new ChessEndgameDetector(game);
        detector.IsGameOver(progress.State).Should().BeFalse("Game should still be in progress");
    }
}
