using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal.Layout;

using Xunit;

namespace Veggerby.Boards.Tests.Core.Layout;

public class PieceMapIncrementalTests
{
    [Fact]
    public void GivenMove_WhenHandled_ThenPieceMapTileIndexUpdates()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var pawn = game.GetPiece("white-pawn-2");
        var from = game.GetTile("tile-b2");
        var to = game.GetTile("tile-b3");
        var shape = progress.Engine.Services.TryGet(out BoardShape s) ? s : null;
        Assert.NotNull(shape);
        Assert.NotNull(progress.PieceMapSnapshot);
        var beforeIndex = progress.PieceMapSnapshot.GetTileIndex(pawn);
        Assert.True(beforeIndex >= 0);
        Assert.True(shape!.TryGetTileIndex(to, out var toIdx));

        // act
        var relation = new Veggerby.Boards.Artifacts.Relations.TileRelation(from, to, Veggerby.Boards.Artifacts.Relations.Direction.South); // white pawn moves south
        var path = new TilePath([relation]);
        var after = progress.HandleEvent(new MovePieceGameEvent(pawn, path));
        // assert
        var afterIndex = after.PieceMapSnapshot.GetTileIndex(pawn);
        Assert.Equal((short)toIdx, afterIndex);
        Assert.False(ReferenceEquals(after.PieceMapSnapshot, progress.PieceMapSnapshot));
    }

    [Fact]
    public void GivenMove_WhenExpectedFromMismatch_ThenNoUpdateOccurs()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var pawn = game.GetPiece("white-pawn-2");
        var from = game.GetTile("tile-b2");
        var to = game.GetTile("tile-b3");
        var wrongFrom = game.GetTile("tile-b4");
        var shape = progress.Engine.Services.TryGet(out BoardShape s) ? s : null;
        Assert.NotNull(shape);
        var originalSnapshot = progress.PieceMapSnapshot;

        // simulate mismatch by manual call (internal path); we can't feed wrong from via event because rule will reject, so call snapshot directly
        Assert.True(shape!.TryGetTileIndex(wrongFrom, out var wrongIdx));
        Assert.True(shape.TryGetTileIndex(to, out var toIdx));
        var manual = originalSnapshot.UpdateForMove(pawn, (short)wrongIdx, (short)toIdx);
        Assert.True(ReferenceEquals(manual, originalSnapshot)); // mismatch returns same instance
    }

    [Fact]
    public void GivenMove_OtherPieceIndicesUnchanged()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var pawn = game.GetPiece("white-pawn-2");
        var other = game.GetPiece("white-pawn-3");
        var from = game.GetTile("tile-b2");
        var to = game.GetTile("tile-b3");
        var beforeOtherIndex = progress.PieceMapSnapshot.GetTileIndex(other);

        // act
        var relation2 = new Veggerby.Boards.Artifacts.Relations.TileRelation(from, to, Veggerby.Boards.Artifacts.Relations.Direction.South);
        var path2 = new TilePath([relation2]);
        var after = progress.HandleEvent(new MovePieceGameEvent(pawn, path2));
        // assert
        Assert.Equal(beforeOtherIndex, after.PieceMapSnapshot.GetTileIndex(other));
    }
}