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
        // arrange - Create a simple ko situation using corner pattern
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;

        // Create ko pattern in corner:
        // Pattern:  B W
        //           W ?
        // Black at 1-1, White at 2-1, 1-2
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-1-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-2-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-2-2")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-1-2")!));

        // Black captures white at 1-2 by playing at 1-3, creating ko
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-1-3")!));

        var afterFirstCapture = progress.State;
        afterFirstCapture.IsCaptured(whiteStone2).Should().BeTrue("white stone at 1-2 should be captured");

        var extras = afterFirstCapture.GetExtras<GoStateExtras>()!;
        extras.KoTileId.Should().Be("tile-1-2", "ko position should be marked at captured stone location");

        var piecesBeforeKoAttempt = afterFirstCapture.GetStates<PieceState>().Count();

        // act - White attempts immediate recapture at 1-2 (ko violation)
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-1-2")!));

        // assert - Ko rule should reject the move
        var afterKoAttempt = progress.State;
        var piecesAfterKoAttempt = afterKoAttempt.GetStates<PieceState>().Count();
        
        piecesAfterKoAttempt.Should().Be(piecesBeforeKoAttempt, "ko recapture should be rejected");
        afterKoAttempt.IsCaptured(whiteStone3).Should().BeFalse("white stone should not be placed");
        
        var piecesOnKoTile = afterKoAttempt.GetPiecesOnTile(progress.Game.GetTile("tile-1-2")!);
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
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;

        // Create ko pattern in corner (same as first test)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-1-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-2-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-2-2")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-1-2")!));

        // Black captures white, creating ko
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-1-3")!));

        // White plays elsewhere (clears ko restriction)
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-9-9")!));

        var afterPlayElsewhere = progress.State;
        var extrasAfterPlayElsewhere = afterPlayElsewhere.GetExtras<GoStateExtras>()!;
        extrasAfterPlayElsewhere.KoTileId.Should().BeNull("ko restriction should be cleared after playing elsewhere");

        // act - White recaptures (now allowed because ko is cleared)
        var whiteStone4 = progress.Game.GetPiece("white-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone4, progress.Game.GetTile("tile-1-2")!));

        // assert
        var afterRecapture = progress.State;
        afterRecapture.IsCaptured(blackStone3).Should().BeTrue("black stone should be recaptured after ko cleared");
        afterRecapture.IsCaptured(whiteStone4).Should().BeFalse("white stone should be placed successfully");
        
        var piecesOnRecaptureTile = afterRecapture.GetPiecesOnTile(progress.Game.GetTile("tile-1-2")!);
        piecesOnRecaptureTile.Should().ContainSingle().Which.Should().Be(whiteStone4, "recapture allowed after ko cleared");
    }

    [Fact]
    public void GivenKoSituation_WhenPassTurn_ThenKoCleared()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;

        // Create ko pattern and capture (same setup)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-1-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-2-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-2-2")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-1-2")!));

        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-1-3")!));

        var afterCapture = progress.State;
        var extrasAfterCapture = afterCapture.GetExtras<GoStateExtras>()!;
        extrasAfterCapture.KoTileId.Should().Be("tile-1-2", "ko should be set after capture");

        // act - Pass turn
        progress = progress.HandleEvent(new PassTurnGameEvent());

        // assert
        var afterPass = progress.State;
        var extrasAfterPass = afterPass.GetExtras<GoStateExtras>()!;
        extrasAfterPass.KoTileId.Should().BeNull("ko should be cleared after pass");
    }
}
