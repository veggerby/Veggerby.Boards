using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.TestHelpers;

/// <summary>
/// Convenience extensions for tests to assert required artifact/state retrievals.
/// Keeps production API surface nullable where appropriate while enabling concise test intent.
/// </summary>
public static class RequiredRetrievalExtensions
{
    public static Tile GetRequiredTile(this Board board, string tileId)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(tileId);
        return board.GetTile(tileId).EnsureNotNull();
    }

    public static Piece GetRequiredPiece(this Game game, string pieceId)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(pieceId);
        return game.GetPiece(pieceId).EnsureNotNull();
    }

    public static TExtras GetRequiredExtras<TExtras>(this GameState state)
        where TExtras : class
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.GetExtras<TExtras>().EnsureNotNull();
    }

    public static PieceState GetRequiredPieceState(this GameState state, Piece piece)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(piece);
        return state.GetState<PieceState>(piece).EnsureNotNull();
    }
}