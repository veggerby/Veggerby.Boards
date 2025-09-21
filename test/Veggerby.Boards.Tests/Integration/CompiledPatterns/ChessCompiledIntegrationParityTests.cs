using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Integration-level parity test ensuring that when compiled patterns are enabled at build time the resolved
/// movement path for a representative chess move (white pawn double advance) matches the legacy visitor result.
/// </summary>
public class ChessCompiledIntegrationParityTests
{
    [Fact]
    public void GivenPawnDoubleAdvance_WhenCompiledEnabled_ThenPathMatchesLegacy()
    {
        // arrange
        var original = FeatureFlags.EnableCompiledPatterns;
        FeatureFlags.EnableCompiledPatterns = true;
        try
        {
            var builder = new ChessGameBuilder();
            var progress = builder.Compile();
            var game = progress.Game;
            var pawn = game.GetPiece("white-pawn-2");
            var from = game.GetTile("e2");
            var to = game.GetTile("e4");

            // act
            var legacyVisitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            pawn.Patterns.First().Accept(legacyVisitor);
            var legacy = legacyVisitor.ResultPath;
            var compiled = progress.ResolvePathCompiledFirst(pawn, from, to);

            // assert
            legacy.Should().NotBeNull();
            compiled.Should().NotBeNull();
            compiled.From.Should().Be(legacy.From);
            compiled.To.Should().Be(legacy.To);
            compiled.Distance.Should().Be(legacy.Distance);
        }
        finally
        {
            FeatureFlags.EnableCompiledPatterns = original;
        }
    }
}