using System.Linq;

using Veggerby.Boards.Othello;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Othello;

public class OthelloMultiMoveTests
{
    [Fact]
    public void Should_allow_multi_move_sequence_like_in_demo()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        // Initial state: d5=Black, e4=Black, d4=White, e5=White
        
        // act - Move 1: Black → d3 (should flip d4 white to black)
        var blackDisc3 = progress.Game.GetPiece("black-disc-3")!;
        var d3Tile = progress.Game.GetTile(OthelloIds.Tiles.D3)!;
        progress = progress.HandleEvent(new PlaceDiscGameEvent(blackDisc3, d3Tile));

        // assert - After move 1: d5=Black, e4=Black, d4=Black (flipped), e5=White, d3=Black
        var d4Tile = progress.Game.GetTile(OthelloIds.Tiles.D4)!;
        var whiteDisc1 = progress.Game.GetPiece("white-disc-1")!;
        var d4ColorAfterMove1 = OthelloHelper.GetCurrentDiscColor(whiteDisc1, progress.State);
        d4ColorAfterMove1.Should().Be(OthelloDiscColor.Black);

        // act - Move 2: White → c4 (should flip d4 black to white)
        var whiteDisc3 = progress.Game.GetPiece("white-disc-3")!;
        var c4Tile = progress.Game.GetTile(OthelloIds.Tiles.C4)!;
        progress = progress.HandleEvent(new PlaceDiscGameEvent(whiteDisc3, c4Tile));

        // assert - after move 2, d4 should be white again
        var d4ColorAfterMove2 = OthelloHelper.GetCurrentDiscColor(whiteDisc1, progress.State);
        d4ColorAfterMove2.Should().Be(OthelloDiscColor.White);
    }
}
