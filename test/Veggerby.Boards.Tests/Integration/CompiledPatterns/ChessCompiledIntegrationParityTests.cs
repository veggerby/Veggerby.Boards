using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Integration-level parity test ensuring compiled patterns produce identical paths to legacy visitor
/// when feature flag is enabled during game build (white pawn double advance scenario).
/// </summary>
public class ChessCompiledIntegrationParityTests
{
    [Fact]
    public void GivenCompiledPatternsEnabled_WhenResolvingPawnSingleAdvance_ThenPathMatchesLegacy()
    {
        // arrange (single square advance is supported; double is not modeled yet)
        using var _ = new Veggerby.Boards.Tests.Infrastructure.FeatureFlagScope(compiledPatterns: true, adjacencyCache: false);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece("white-pawn-5"); // on e2
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e3");

        // act (legacy)
        var legacyVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
        foreach (var pattern in piece.Patterns) { pattern.Accept(legacyVisitor); }
        var legacy = legacyVisitor.ResultPath;

        // act (compiled first)
        var compiled = progress.ResolvePathCompiledFirst(piece, from, to);

        // assert
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.From.Should().Be(legacy.From);
        compiled.To.Should().Be(legacy.To);
        compiled.Distance.Should().Be(legacy.Distance);
        compiled.Relations.Should().HaveSameCount(legacy.Relations);
    }

    [Fact]
    public void GivenCompiledPatternsEnabled_WhenResolvingPawnDoubleAdvance_ThenBothResolversReturnNull()
    {
        // arrange (double advance not represented by current pattern set)
        using var _ = new Veggerby.Boards.Tests.Infrastructure.FeatureFlagScope(compiledPatterns: true, adjacencyCache: false);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece("white-pawn-5");
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e4");

        // act (legacy)
        var legacyVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
        foreach (var pattern in piece.Patterns) { pattern.Accept(legacyVisitor); }
        var legacy = legacyVisitor.ResultPath;

        // act (compiled)
        var compiled = progress.ResolvePathCompiledFirst(piece, from, to);

        // assert
        legacy.Should().BeNull();
        compiled.Should().BeNull();
    }
}