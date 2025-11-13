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
        // Ko pattern:    B
        //              W X W  where X is initially white, black captures it
        //                B
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;

        // Setup: white at 5-5 (will be captured), black at 4-5, 6-5, 5-6, white at 4-4, 6-6 (support)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-5-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-4-4")!)); // Support white
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-6-6")!)); // Support white
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-5-6")!));

        var beforeCapture = progress.State;
        beforeCapture.IsCaptured(whiteStone1).Should().BeFalse("white stone not yet captured");

        // Black plays 5-4 to capture white at 5-5 (single stone captures single stone, creating ko)
        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-5-4")!));

        var afterFirstCapture = progress.State;
        afterFirstCapture.IsCaptured(whiteStone1).Should().BeTrue("white stone should be captured");

        var extras = afterFirstCapture.GetExtras<GoStateExtras>()!;
        extras.KoTileId.Should().Be("tile-5-5", "ko position should be marked at captured stone location");

        var piecesBeforeKoAttempt = afterFirstCapture.GetStates<PieceState>().Count();

        // act - White attempts immediate recapture at 5-5 (ko violation)
        var whiteStone4 = progress.Game.GetPiece("white-stone-4")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone4, progress.Game.GetTile("tile-5-5")!));

        // assert - Ko rule should reject the move
        var afterKoAttempt = progress.State;
        var piecesAfterKoAttempt = afterKoAttempt.GetStates<PieceState>().Count();
        
        piecesAfterKoAttempt.Should().Be(piecesBeforeKoAttempt, "ko recapture should be rejected");
        afterKoAttempt.IsCaptured(whiteStone4).Should().BeFalse("white stone should not be placed");
        
        var piecesOnKoTile = afterKoAttempt.GetPiecesOnTile(progress.Game.GetTile("tile-5-5")!);
        piecesOnKoTile.Should().BeEmpty("ko recapture should not place stone");
    }

    [Fact]
    public void GivenKoSituation_WhenPlayElsewhereThenRecapture_ThenRecaptureAllowed()
    {
        // arrange - Create ko, play elsewhere, then verify ko is cleared
        // This tests that ko restriction is cleared when a move is made elsewhere
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        var blackStone5 = progress.Game.GetPiece("black-stone-5")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;
        var whiteStone4 = progress.Game.GetPiece("white-stone-4")!;
        var whiteStone5 = progress.Game.GetPiece("white-stone-5")!;
        var whiteStone6 = progress.Game.GetPiece("white-stone-6")!;

        // Create same ko pattern as first test - white at 5-5, surrounded by black
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-5-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-4-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-6-6")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-5-6")!));

        // Fill in liberties around black-stone-4's future position so recapture will work
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone4, progress.Game.GetTile("tile-5-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone5, progress.Game.GetTile("tile-4-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone5, progress.Game.GetTile("tile-6-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-5-4")!)); // Black captures white-stone-1

        var afterFirstCapture = progress.State;
        afterFirstCapture.IsCaptured(whiteStone1).Should().BeTrue("white stone should be captured");
        
        var extras = afterFirstCapture.GetExtras<GoStateExtras>()!;
        extras.KoTileId.Should().Be("tile-5-5", "ko should be set");

        // White plays elsewhere (clears ko restriction)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone6, progress.Game.GetTile("tile-9-9")!));

        var afterPlayElsewhere = progress.State;
        var extrasAfterPlayElsewhere = afterPlayElsewhere.GetExtras<GoStateExtras>()!;
        extrasAfterPlayElsewhere.KoTileId.Should().BeNull("ko restriction should be cleared after playing elsewhere");

        // act - Black passes (to complete turn cycle), then white recaptures
        progress = progress.HandleEvent(new PassTurnGameEvent());

        var blackStone4TileBefore = progress.State.GetState<PieceState>(blackStone4)!.CurrentTile;
        blackStone4TileBefore.Id.Should().Be("tile-5-4", "black stone 4 should still be at 5-4");

        // White recaptures at former ko position (now allowed)
        var whiteStone7 = progress.Game.GetPiece("white-stone-7")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone7, progress.Game.GetTile("tile-5-5")!));

        // assert - Recapture should succeed (ko cleared) and capture black stone 4
        var afterRecapture = progress.State;
        
        afterRecapture.IsCaptured(blackStone4).Should().BeTrue("black stone 4 should be captured by recapture");
        afterRecapture.IsCaptured(whiteStone7).Should().BeFalse("white stone 7 should be placed successfully");
        
        var piecesOnRecaptureTile = afterRecapture.GetPiecesOnTile(progress.Game.GetTile("tile-5-5")!);
        piecesOnRecaptureTile.Should().ContainSingle().Which.Should().Be(whiteStone7, "white stone 7 at former ko position after recapture");
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
        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;

        // Create ko pattern and capture (same setup)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-5-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-4-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-6-6")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-5-6")!));

        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-5-4")!));

        var afterCapture = progress.State;
        var extrasAfterCapture = afterCapture.GetExtras<GoStateExtras>()!;
        extrasAfterCapture.KoTileId.Should().Be("tile-5-5", "ko should be set after capture");

        // act - Pass turn
        progress = progress.HandleEvent(new PassTurnGameEvent());

        // assert
        var afterPass = progress.State;
        var extrasAfterPass = afterPass.GetExtras<GoStateExtras>()!;
        extrasAfterPass.KoTileId.Should().BeNull("ko should be cleared after pass");
    }
}
