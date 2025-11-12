using System.Linq;

using Veggerby.Boards.Go;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Go;

/// <summary>
/// Tests for Go ko rule: prevents immediate recapture that would repeat the board position.
/// </summary>
public class GoKoTests
{
    [Fact]
    public void GivenSimpleKoSituation_WhenImmediateRecapture_ThenMoveRejected()
    {
        // arrange - Create a simple ko situation
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;

        // Create ko pattern
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-5-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-4-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-5-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-6-4")!));

        // Black captures white stone at 5-4 creating ko
        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-5-5")!));

        var afterFirstCapture = progress.State;
        afterFirstCapture.IsCaptured(whiteStone2).Should().BeTrue("white stone should be captured");

        var extras = afterFirstCapture.GetExtras<GoStateExtras>()!;
        extras.KoTileId.Should().NotBeNullOrEmpty("ko position should be marked");

        var piecesBeforeKoAttempt = afterFirstCapture.GetStates<PieceState>().Count();

        // act - White attempts immediate recapture (ko violation)
        var whiteStone4 = progress.Game.GetPiece("white-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone4, progress.Game.GetTile("tile-5-4")!));

        // assert
        var afterKoAttempt = progress.State;
        var piecesAfterKoAttempt = afterKoAttempt.GetStates<PieceState>().Count();
        
        piecesAfterKoAttempt.Should().Be(piecesBeforeKoAttempt, "ko recapture should be rejected");
        afterKoAttempt.IsCaptured(whiteStone4).Should().BeFalse("white stone should not be placed");
        
        var piecesOnKoTile = afterKoAttempt.GetPiecesOnTile(progress.Game.GetTile("tile-5-4")!);
        piecesOnKoTile.Should().BeEmpty("ko recapture should not place stone");
    }

    [Fact]
    public void GivenKoSituation_WhenPlayElsewhereThenRecapture_ThenRecaptureAllowed()
    {
        // arrange - Create ko, play elsewhere, then recapture
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;

        // Create ko pattern
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-5-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-4-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-5-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-6-4")!));

        // Black captures white stone creating ko
        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-5-5")!));

        // White plays elsewhere (clears ko restriction)
        var whiteStone4 = progress.Game.GetPiece("white-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone4, progress.Game.GetTile("tile-1-1")!));

        var afterPlayElsewhere = progress.State;
        var extrasAfterPlayElsewhere = afterPlayElsewhere.GetExtras<GoStateExtras>()!;
        extrasAfterPlayElsewhere.KoTileId.Should().BeNull("ko restriction should be cleared after playing elsewhere");

        // act - White recaptures (now allowed because ko is cleared)
        var whiteStone5 = progress.Game.GetPiece("white-stone-5")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone5, progress.Game.GetTile("tile-5-4")!));

        // assert
        var afterRecapture = progress.State;
        afterRecapture.IsCaptured(blackStone4).Should().BeTrue("black stone should be recaptured after ko cleared");
        afterRecapture.IsCaptured(whiteStone5).Should().BeFalse("white stone should be placed successfully");
        
        var piecesOnRecaptureTile = afterRecapture.GetPiecesOnTile(progress.Game.GetTile("tile-5-4")!);
        piecesOnRecaptureTile.Should().ContainSingle().Which.Should().Be(whiteStone5, "recapture allowed after ko cleared");
    }

    [Fact]
    public void GivenKoSituation_WhenPassTurn_ThenKoCleared()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;

        // Create ko pattern and capture
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-5-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-4-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-5-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-6-4")!));

        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-5-5")!));

        var afterCapture = progress.State;
        var extrasAfterCapture = afterCapture.GetExtras<GoStateExtras>()!;
        extrasAfterCapture.KoTileId.Should().NotBeNullOrEmpty("ko should be set");

        // act - Pass turn
        progress = progress.HandleEvent(new PassTurnGameEvent());

        // assert
        var afterPass = progress.State;
        var extrasAfterPass = afterPass.GetExtras<GoStateExtras>()!;
        extrasAfterPass.KoTileId.Should().BeNull("ko should be cleared after pass");
    }
}
