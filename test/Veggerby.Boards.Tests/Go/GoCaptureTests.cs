using System.Linq;

using Veggerby.Boards.Go;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Go;

/// <summary>
/// Tests for Go capture mechanics: stone removal, group capture, suicide rule enforcement.
/// </summary>
public class GoCaptureTests
{
    [Fact]
    public void GivenSingleStoneWithNoLiberties_WhenSurrounded_ThenStoneIsCaptured()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;
        var whiteStone4 = progress.Game.GetPiece("white-stone-4")!;

        // Place black stone in center
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-5-5")!));

        // Surround it with white stones (all 4 sides)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-5-4")!));

        var beforeState = progress.State;
        beforeState.IsCaptured(blackStone1).Should().BeFalse();

        // act - place final white stone to complete capture
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone4, progress.Game.GetTile("tile-5-6")!));

        // assert
        var afterState = progress.State;
        afterState.IsCaptured(blackStone1).Should().BeTrue("black stone should be captured when surrounded");
        
        var piecesOnCenter = afterState.GetPiecesOnTile(progress.Game.GetTile("tile-5-5")!);
        piecesOnCenter.Should().BeEmpty("captured stone should be removed from board");
    }

    [Fact]
    public void GivenMultiStoneGroup_WhenAllLibertiesRemoved_ThenEntireGroupIsCaptured()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;
        var whiteStone4 = progress.Game.GetPiece("white-stone-4")!;
        var whiteStone5 = progress.Game.GetPiece("white-stone-5")!;
        var whiteStone6 = progress.Game.GetPiece("white-stone-6")!;

        // Create a 2-stone black group
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-5-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-5-6")!));

        // Surround the group
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-6-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-4-6")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone4, progress.Game.GetTile("tile-6-6")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone5, progress.Game.GetTile("tile-5-4")!));

        var beforeState = progress.State;
        beforeState.IsCaptured(blackStone1).Should().BeFalse();
        beforeState.IsCaptured(blackStone2).Should().BeFalse();

        // act - complete the capture
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone6, progress.Game.GetTile("tile-5-7")!));

        // assert
        var afterState = progress.State;
        afterState.IsCaptured(blackStone1).Should().BeTrue("first black stone should be captured");
        afterState.IsCaptured(blackStone2).Should().BeTrue("second black stone should be captured");
        
        var piecesOnTile55 = afterState.GetPiecesOnTile(progress.Game.GetTile("tile-5-5")!);
        var piecesOnTile56 = afterState.GetPiecesOnTile(progress.Game.GetTile("tile-5-6")!);
        piecesOnTile55.Should().BeEmpty("first captured stone should be removed");
        piecesOnTile56.Should().BeEmpty("second captured stone should be removed");
    }

    [Fact]
    public void GivenSuicideMoveWithoutCapture_WhenPlaced_ThenMoveRejected()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;
        var whiteStone3 = progress.Game.GetPiece("white-stone-3")!;

        // Create a situation where black would have no liberties
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-1-2")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-2-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone3, progress.Game.GetTile("tile-2-2")!));

        var beforeState = progress.State;
        var beforePieces = beforeState.GetStates<PieceState>().Count();

        // act - attempt suicide move (black stone at corner with no liberties)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-1-1")!));

        // assert
        var afterState = progress.State;
        var afterPieces = afterState.GetStates<PieceState>().Count();
        
        afterPieces.Should().Be(beforePieces, "suicide move should be rejected, no new stones added");
        afterState.IsCaptured(blackStone1).Should().BeFalse("stone should not be placed or captured");
        
        var piecesOnCorner = afterState.GetPiecesOnTile(progress.Game.GetTile("tile-1-1")!);
        piecesOnCorner.Should().BeEmpty("suicide move should not place stone");
    }

    [Fact]
    public void GivenSuicideMoveWithCapture_WhenPlaced_ThenMoveAllowedAndOpponentCaptured()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;

        // Create situation where white stone is nearly captured
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-1-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-1-2")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-2-2")!));

        // Black's next move at 2-1 would be "suicide" (no liberties) BUT it captures white
        // So it should be allowed

        var beforeState = progress.State;
        beforeState.IsCaptured(whiteStone1).Should().BeFalse();

        // act - place black stone that captures white (suicide with capture is legal)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-2-1")!));

        // assert
        var afterState = progress.State;
        afterState.IsCaptured(whiteStone1).Should().BeTrue("white stone should be captured");
        afterState.IsCaptured(blackStone3).Should().BeFalse("black stone should be placed successfully");
        
        var piecesOnWhiteTile = afterState.GetPiecesOnTile(progress.Game.GetTile("tile-1-1")!);
        piecesOnWhiteTile.Should().BeEmpty("captured white stone should be removed");
        
        var piecesOnBlackTile = afterState.GetPiecesOnTile(progress.Game.GetTile("tile-2-1")!);
        piecesOnBlackTile.Should().ContainSingle().Which.Should().Be(blackStone3, "black stone should be placed");
    }

    [Fact]
    public void GivenSnapbackSituation_WhenRecaptureOccurs_ThenCaptureAllowed()
    {
        // arrange - Snapback: immediate recapture that is NOT ko (different board position)
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;

        // Create snapback setup
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-3-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-4-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-5-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-4-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-4-4")!));

        // White captures black's stone (creating a snapback opportunity)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-4-2")!));

        var beforeSnapback = progress.State;
        
        // Black stone at 4-3 should be captured
        beforeSnapback.IsCaptured(blackStone2).Should().BeTrue();

        // act - Black recaptures immediately (snapback)
        var blackStone5 = progress.Game.GetPiece("black-stone-5")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone5, progress.Game.GetTile("tile-4-3")!));

        // assert - Snapback is allowed (not ko because board position is different)
        var afterState = progress.State;
        afterState.IsCaptured(whiteStone1).Should().BeTrue("white stone should be captured in snapback");
        afterState.IsCaptured(whiteStone2).Should().BeTrue("white stone should be captured in snapback");
        afterState.IsCaptured(blackStone5).Should().BeFalse("recapturing black stone should be placed");
    }
}
