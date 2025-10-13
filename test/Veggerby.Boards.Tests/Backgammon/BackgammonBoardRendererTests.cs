using System.IO;

using Veggerby.Boards.Backgammon;

namespace Veggerby.Boards.Tests.Backgammon;

public class BackgammonBoardRendererTests
{
    [Fact]
    public void GivenInitialBackgammon_WhenWrite_ThenTopBottomAndSummaryPrinted()
    {
        // arrange
        var progress = new BackgammonGameBuilder().Compile();
        var game = progress.Game;
        var state = progress.State;
        using var sw = new StringWriter();

        // act
        BackgammonBoardRenderer.Write(game, state, sw);
        var output = sw.ToString();

        // assert
        output.Should().Contain("+---------------- BACKGAMMON ----------------+");
        output.Should().Contain("Top (24 -> 13)");
        output.Should().Contain("Bottom (12 -> 1)");

        // Known initial points from builder
        // Renderer formats as "{p,2}:{FormatPoint}" (no space after colon)
        output.Should().Contain("24:b2"); // black-1, black-2 at point-24
        output.Should().Contain("13:b5"); // black-3..7 at point-13
        output.Should().Contain(" 8:b3"); // padded left
        output.Should().Contain(" 6:b5"); // padded left

        output.Should().Contain(" 1:w2");   // white-1..2 at point-1
        output.Should().Contain("12:w5");  // white-3..7 at point-12
        output.Should().Contain("17:w3");  // white-8..10 at point-17
        output.Should().Contain("19:w5");  // white-11..15 at point-19

        // Summary line
        output.Should().Contain("Bar: W0 B0 | Home: W0 B0");
    }
}