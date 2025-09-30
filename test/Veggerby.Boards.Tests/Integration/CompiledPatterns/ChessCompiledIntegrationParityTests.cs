using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Integration-level parity test ensuring compiled patterns produce identical paths to legacy visitor
/// when feature flag is enabled during game build (white pawn double advance scenario).
/// </summary>
public class ChessCompiledIntegrationParityTests
{
    [Fact(Skip = "diagnostic")]
    public void Diagnostic_PrintWhitePawnPatterns()
    {
        using var _ = new FeatureFlagScope(compiledPatterns: true, adjacencyCache: false);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece("white-pawn-5");
        var from = progress.Game.GetTile("e2");
        var to1 = progress.Game.GetTile("e3");
        var to2 = progress.Game.GetTile("e4");

        var legacyVisitor1 = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to1);
        foreach (var p in piece.Patterns)
        {
            p.Accept(legacyVisitor1); if (legacyVisitor1.ResultPath != null) break;
        }

        var legacyVisitor2 = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to2);
        foreach (var p in piece.Patterns)
        {
            p.Accept(legacyVisitor2); if (legacyVisitor2.ResultPath != null) break;
        }

        legacyVisitor1.ResultPath.Should().BeNull("Expected current legacy to fail for single advance for reproduction");
        legacyVisitor2.ResultPath.Should().BeNull("Expected current legacy to fail for double advance for reproduction");
    }
    [Fact]
    public void GivenCompiledPatternsEnabled_WhenResolvingPawnSingleAdvance_ThenPathMatchesLegacy()
    {
        // arrange (single square advance supported; structural two-step now exists as fixed pattern)
        using var _ = new FeatureFlagScope(compiledPatterns: true, adjacencyCache: false);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece("white-pawn-5"); // on e2
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e3");

        // act (legacy) – expect fixed pattern or direction pattern to yield path
        var legacyVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
        foreach (var pattern in piece.Patterns)
        {
            pattern.Accept(legacyVisitor);
            if (legacyVisitor.ResultPath is not null)
            {
                break;
            }
        }

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
    public void GivenCompiledPatternsEnabled_WhenResolvingPawnDoubleAdvance_ThenStructuralPathExists()
    {
        // arrange (double advance now structurally represented by fixed two-step pattern; legality gated by rules elsewhere)
        using var _ = new FeatureFlagScope(compiledPatterns: true, adjacencyCache: false);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece("white-pawn-5");
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e4");

        // act legacy & compiled
        var legacyVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
        foreach (var pattern in piece.Patterns)
        {
            pattern.Accept(legacyVisitor);
            if (legacyVisitor.ResultPath is not null)
            {
                break;
            }
        }

        var legacy = legacyVisitor.ResultPath;
        var compiled = progress.ResolvePathCompiledFirst(piece, from, to);

        // assert structural presence on both resolvers (two relations)
        legacy.Should().NotBeNull();
        legacy.Distance.Should().Be(2);
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
        compiled.Relations.Should().HaveSameCount(legacy.Relations);
    }
}