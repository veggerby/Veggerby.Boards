using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Validates that enabling the compiled patterns adjacency cache does not change
/// resolved paths compared to compiled patterns without the cache.
/// Uses a representative subset of chess archetypes (rook slider, bishop slider,
/// knight fixed jump, queen multi-direction, pawn single step + unreachable double step).
/// </summary>
public class CompiledPatternAdjacencyCacheParityTests
{
    private static TilePath? ResolveCompiled(GameProgress progress, Piece piece, Tile from, Tile to)
    {
        return progress.ResolvePathCompiledFirst(piece, from, to);
    }

    [Theory]
    [InlineData("white-rook-1", "a1", "a4")] // vertical slider (north chain)
    [InlineData("white-bishop-1", "c1", "f4")] // diagonal
    [InlineData("white-queen", "e1", "h4")] // multi-direction (east+north-east sequence)
    [InlineData("white-knight-1", "b1", "c3")] // L-jump
    [InlineData("white-pawn-5", "e2", "e3")] // single step pawn
    public void GivenAdjacencyCacheToggle_WhenResolvingReachablePaths_ThenResultsMatch(string pieceId, string fromId, string toId)
    {
        // arrange
        var builder = new ChessGameBuilder();
        TilePath? without;
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: false))
        {
            var progress = builder.Compile();
            var piece = progress.Game.GetPiece(pieceId);
            var from = progress.Game.GetTile($"tile-{fromId}");
            var to = progress.Game.GetTile($"tile-{toId}");
            piece.Should().NotBeNull();
            from.Should().NotBeNull();
            to.Should().NotBeNull();
            without = ResolveCompiled(progress, piece!, from!, to!);
        }

        TilePath? with;
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var progress = builder.Compile();
            var piece = progress.Game.GetPiece(pieceId);
            var from = progress.Game.GetTile($"tile-{fromId}");
            var to = progress.Game.GetTile($"tile-{toId}");
            piece.Should().NotBeNull();
            from.Should().NotBeNull();
            to.Should().NotBeNull();
            with = ResolveCompiled(progress, piece!, from!, to!);
        }

        // assert
        if (without is null)
        {
            with.Should().BeNull();
            return;
        }
        with.Should().NotBeNull();
        with!.Distance.Should().Be(without!.Distance);
        with.To.Should().Be(without.To);
        with.From.Should().Be(without.From);
        with.Relations.Should().HaveSameCount(without.Relations);
    }

    [Fact]
    public void GivenAdjacencyCacheToggle_WhenResolvingDoublePawnAdvance_ThenStructuralPathMatches()
    {
        // arrange

        // act

        // assert

        var builder = new ChessGameBuilder();
        TilePath? without;
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: false))
        {
            var progress = builder.Compile();
            var piece = progress.Game.GetPiece("white-pawn-5");
            var from = progress.Game.GetTile(ChessIds.Tiles.E2);
            var to = progress.Game.GetTile(ChessIds.Tiles.E4);
            piece.Should().NotBeNull();
            from.Should().NotBeNull();
            to.Should().NotBeNull();
            without = ResolveCompiled(progress, piece!, from!, to!);
        }
        TilePath? with;
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var progress = builder.Compile();
            var piece = progress.Game.GetPiece("white-pawn-5");
            var from = progress.Game.GetTile(ChessIds.Tiles.E2);
            var to = progress.Game.GetTile(ChessIds.Tiles.E4);
            piece.Should().NotBeNull();
            from.Should().NotBeNull();
            to.Should().NotBeNull();
            with = ResolveCompiled(progress, piece!, from!, to!);
        }

        // assert structural path present and consistent both with and without adjacency cache
        without.Should().NotBeNull();
        with.Should().NotBeNull();
        with!.Distance.Should().Be(without!.Distance);
        with.Relations.Should().HaveSameCount(without.Relations);
    }
}
