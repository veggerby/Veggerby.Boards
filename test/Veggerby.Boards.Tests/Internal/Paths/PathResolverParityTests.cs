using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;
using AwesomeAssertions;

namespace Veggerby.Boards.Tests.Internal.Paths;

public class PathResolverParityTests
{
    [Fact]
    public void GivenRookMove_WhenResolvedThroughPathResolver_ThenGeometricPathReturned()
    {
        // arrange
        using var scope = new FeatureFlagScope(compiledPatterns: true, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        var rook = progress.Game.GetPiece("white-rook-1");
        var from = progress.Game.GetTile(ChessIds.Tiles.A1);
        var to = progress.Game.GetTile(ChessIds.Tiles.A4); // a1->a2->a3->a4 (geometric; occupancy not enforced at path layer)
        var resolver = progress.Engine.Capabilities?.PathResolver;

        // act
        var path = resolver!.Resolve(rook, from, to, progress.State);

        // assert
        resolver.Should().NotBeNull();
        path.Should().NotBeNull();
        path!.To.Should().Be(to);
        path.Distance.Should().Be(3);
    }

    [Fact]
    public void GivenKnightMove_WhenResolvedThroughPathResolver_ThenPathMatchesCompiled()
    {
        // arrange
        using var scope = new FeatureFlagScope(compiledPatterns: true, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        var knight = progress.Game.GetPiece("white-knight-1");
        var from = progress.Game.GetTile(ChessIds.Tiles.B1);
        var to = progress.Game.GetTile(ChessIds.Tiles.C3);
        var resolver = progress.Engine.Capabilities?.PathResolver;

        // act
        var path = resolver!.Resolve(knight, from, to, progress.State);

        // assert
        resolver.Should().NotBeNull();
        path.Should().NotBeNull();
        path!.To.Should().Be(to);
        path.Distance.Should().Be(3); // compiled pattern expands knight L into 3 directional steps
    }
}