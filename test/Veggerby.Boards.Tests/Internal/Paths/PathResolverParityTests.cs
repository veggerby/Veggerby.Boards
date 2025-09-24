using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal.Paths;
using Veggerby.Boards.Tests.Utils;

using Xunit;

namespace Veggerby.Boards.Tests.Internal.Paths;

public class PathResolverParityTests
{
    [Fact]
    public void GivenRookMove_WhenResolvedThroughPathResolver_ThenGeometricPathReturned()
    {
        using var scope = new FeatureFlagScope(compiledPatterns: true, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        var rook = progress.Game.GetPiece("white-rook-1");
        var from = progress.Game.GetTile("tile-a1");
        var to = progress.Game.GetTile("tile-a4"); // a1->a2->a3->a4 (geometric; occupancy not enforced at path layer)
        Assert.True(progress.Engine.Services.TryGet<IPathResolver>(out var resolver));
        var path = resolver.Resolve(rook, from, to, progress.State);
        Assert.NotNull(path);
        Assert.Equal(to, path.To);
        Assert.Equal(3, path.Distance);
    }

    [Fact]
    public void GivenKnightMove_WhenResolvedThroughPathResolver_ThenPathMatchesCompiled()
    {
        using var scope = new FeatureFlagScope(compiledPatterns: true, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        var knight = progress.Game.GetPiece("white-knight-1");
        var from = progress.Game.GetTile("tile-b1");
        var to = progress.Game.GetTile("tile-c3");
        Assert.True(progress.Engine.Services.TryGet<IPathResolver>(out var resolver));
        var path = resolver.Resolve(knight, from, to, progress.State);
        Assert.NotNull(path);
        Assert.Equal(to, path.To);
        Assert.Equal(3, path.Distance); // compiled pattern expands knight L into 3 directional steps
    }
}