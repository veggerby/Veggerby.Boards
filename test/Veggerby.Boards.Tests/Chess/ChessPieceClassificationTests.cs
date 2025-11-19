using System.Linq;

using Veggerby.Boards.Chess;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Ensures every chess piece has proper role and color metadata attached.
/// Guards against drift when adding/removing pieces or adjusting builder wiring.
/// </summary>
public class ChessPieceClassificationTests
{
    [Fact]
    public void GivenStandardChessGame_WhenBuilt_AllPieceIdsResolveRoleAndColor()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var game = progress.Game;

        // act
        var allPieces = game.Artifacts.OfType<Veggerby.Boards.Artifacts.Piece>().ToArray();

        // assert
        allPieces.Should().HaveCount(32, "Standard chess has 32 pieces");

        foreach (var piece in allPieces)
        {
            piece.Metadata.Should().NotBeNull($"Piece {piece.Id} should have metadata");
            var chessMeta = piece.Metadata as ChessPieceMetadata;
            chessMeta.Should().NotBeNull($"Piece {piece.Id} metadata should be ChessPieceMetadata");
        }

        // Verify we have expected counts
        allPieces.Count(p => ChessPiece.IsPawn(game, p.Id)).Should().Be(16, "Should have 16 pawns");
        allPieces.Count(p => ChessPiece.IsKnight(game, p.Id)).Should().Be(4, "Should have 4 knights");
        allPieces.Count(p => ChessPiece.IsBishop(game, p.Id)).Should().Be(4, "Should have 4 bishops");
        allPieces.Count(p => ChessPiece.IsRook(game, p.Id)).Should().Be(4, "Should have 4 rooks");
        allPieces.Count(p => ChessPiece.IsQueen(game, p.Id)).Should().Be(2, "Should have 2 queens");
        allPieces.Count(p => ChessPiece.IsKing(game, p.Id)).Should().Be(2, "Should have 2 kings");

        allPieces.Count(p => ChessPiece.IsWhite(game, p.Id)).Should().Be(16, "Should have 16 white pieces");
        allPieces.Count(p => ChessPiece.IsBlack(game, p.Id)).Should().Be(16, "Should have 16 black pieces");
    }
}
