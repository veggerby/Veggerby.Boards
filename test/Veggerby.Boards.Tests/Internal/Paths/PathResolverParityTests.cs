using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Internal.Paths;

public class PathResolverParityTests
{
    [Fact]
    public void GivenRookMove_WhenResolvedThroughPathResolver_ThenGeometricPathReturned()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        var rook = progress.Game.GetPiece("white-rook-1");
        var from = progress.Game.GetTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A1);
        var to = progress.Game.GetTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A4); // a1->a2->a3->a4 (geometric; occupancy not enforced at path layer)
        var resolver = progress.Engine.Capabilities?.PathResolver;

        // assert (setup)
        rook.Should().NotBeNull();
        from.Should().NotBeNull();
        to.Should().NotBeNull();
        resolver.Should().NotBeNull();

        // act
        var path = resolver!.Resolve(rook!, from!, to!, progress.State);

        // assert
        path.Should().NotBeNull();
        path!.To.Should().Be(to);
        path.Distance.Should().Be(3);
    }

    [Fact]
    public void GivenKnightMove_WhenResolvedThroughPathResolver_ThenPathMatchesCompiled()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        var knight = progress.Game.GetPiece("white-knight-1");
        var from = progress.Game.GetTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.B1);
        var to = progress.Game.GetTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.C3);
        var resolver = progress.Engine.Capabilities?.PathResolver;

        // assert (setup)
        knight.Should().NotBeNull();
        from.Should().NotBeNull();
        to.Should().NotBeNull();
        resolver.Should().NotBeNull();

        // act
        var path = resolver!.Resolve(knight!, from!, to!, progress.State);

        // assert
        path.Should().NotBeNull();
        path!.To.Should().Be(to);
        path.Distance.Should().Be(3); // compiled pattern expands knight L into 3 directional steps
    }
}
