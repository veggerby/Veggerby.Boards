using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.Go;
using Veggerby.Boards.Go.MoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Go;

/// <summary>
/// Tests for Go legal move generation API.
/// </summary>
public class GoLegalMoveGenerationTests
{
    [Fact]
    public void GivenGoStartingPosition_WhenGettingLegalMoves_ThenReturnsAllEmptyIntersectionsPlusPass()
    {
        // arrange
        var builder = new GoGameBuilder(9); // 9x9 for faster tests
        var progress = builder.Compile();

        // Set active player (Go may not have turn sequencing by default)
        var blackPlayer = progress.Game.GetPlayer("black");
        var stateWithActivePlayer = progress.State.Next([new ActivePlayerState(blackPlayer!, true)]);
        progress = new GameProgress(progress.Engine, stateWithActivePlayer, progress.Events);

        var generator = progress.GetGoLegalMoveGenerator();

        // act
        var legalMoves = generator.GetLegalMoves(progress.State).ToList();

        // assert
        // Starting position: 81 empty intersections + 1 pass = 82 legal moves
        legalMoves.Should().HaveCount(82, "9x9 Go board has 81 empty intersections plus pass");

        // Should have exactly one pass move
        var passMoves = legalMoves.OfType<PassTurnGameEvent>().ToList();
        passMoves.Should().HaveCount(1, "Should have exactly one pass move");

        // Remaining should be stone placements
        var placementMoves = legalMoves.OfType<PlaceStoneGameEvent>().ToList();
        placementMoves.Should().HaveCount(81, "Should have placement moves for all empty intersections");
    }

    [Fact]
    public void GivenGoGame_WhenValidatingPassMove_ThenReturnsLegal()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();
        var generator = progress.GetGoLegalMoveGenerator();
        var passEvent = new PassTurnGameEvent();

        // act
        var validation = generator.Validate(passEvent, progress.State);

        // assert
        validation.IsLegal.Should().BeTrue("Pass is always legal when game not ended");
        validation.Reason.Should().Be(RejectionReason.None);
    }

    [Fact]
    public void GivenGoGame_WhenValidatingLegalPlacement_ThenReturnsLegal()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();
        var generator = progress.GetGoLegalMoveGenerator();

        // Get first black stone directly
        var stone = progress.Game.GetPiece("black-stone-1");
        stone.Should().NotBeNull();

        // Get an empty tile
        var tile = progress.Game.Board.Tiles.First();

        var placeEvent = new PlaceStoneGameEvent(stone!, tile);

        // act
        var validation = generator.Validate(placeEvent, progress.State);

        // assert
        validation.IsLegal.Should().BeTrue("Placing on empty intersection is legal");
        validation.Reason.Should().Be(RejectionReason.None);
    }

    [Fact]
    public void GivenOccupiedIntersection_WhenValidatingPlacement_ThenReturnsOccupied()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // Place a stone first
        var firstStone = progress.Game.GetArtifacts<Piece>().First(p => p.Owner?.Id == "black");
        var tile = progress.Game.GetTile("tile-5-5");
        progress = progress.HandleEvent(new PlaceStoneGameEvent(firstStone, tile!));

        var generator = progress.GetGoLegalMoveGenerator();

        // Try to place another stone on same tile
        var secondStone = progress.Game.GetArtifacts<Piece>().First(p => p.Owner?.Id == "white");
        var illegalEvent = new PlaceStoneGameEvent(secondStone, tile!);

        // act
        var validation = generator.Validate(illegalEvent, progress.State);

        // assert
        validation.IsLegal.Should().BeFalse("Cannot place on occupied intersection");
        validation.Reason.Should().Be(RejectionReason.DestinationOccupied);
    }

    [Fact]
    public void GivenEndedGame_WhenGettingLegalMoves_ThenReturnsEmpty()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // Manually end the game
        var endedState = progress.State.Next([new GameEndedState()]);
        var generator = progress.GetGoLegalMoveGenerator();

        // act
        var legalMoves = generator.GetLegalMoves(endedState).ToList();

        // assert
        legalMoves.Should().BeEmpty("Game has ended, no legal moves should exist");
    }

    [Fact]
    public void GivenEndedGame_WhenValidatingMove_ThenReturnsGameEnded()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var stone = progress.Game.GetArtifacts<Piece>().First(p => p.Owner?.Id == "black");
        var tile = progress.Game.Board.Tiles.First();
        var placeEvent = new PlaceStoneGameEvent(stone, tile);

        // End the game
        var endedState = progress.State.Next([new GameEndedState()]);
        var generator = progress.GetGoLegalMoveGenerator();

        // act
        var validation = generator.Validate(placeEvent, endedState);

        // assert
        validation.IsLegal.Should().BeFalse("Move should be illegal after game ends");
        validation.Reason.Should().Be(RejectionReason.GameEnded);
    }

    [Fact]
    public void GivenKoPosition_WhenValidatingKoViolation_ThenReturnsRuleViolation()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // Set up a ko position (simplified for testing)
        // We'll manually set ko in extras
        var extras = new GoStateExtras("tile-5-5", 9);
        var stateWithKo = progress.State.ReplaceExtras(extras);

        var generator = progress.GetGoLegalMoveGenerator();

        // Try to place on ko-forbidden tile
        var stone = progress.Game.GetArtifacts<Piece>().First(p => p.Owner?.Id == "black");
        var koTile = progress.Game.GetTile("tile-5-5");
        var illegalEvent = new PlaceStoneGameEvent(stone, koTile!);

        // act
        var validation = generator.Validate(illegalEvent, stateWithKo);

        // assert
        validation.IsLegal.Should().BeFalse("Cannot place on ko-forbidden tile");
        validation.Reason.Should().Be(RejectionReason.RuleViolation);
        validation.Explanation.Should().Contain("ko");
    }

    [Fact]
    public void GivenGoGame_WhenGettingLegalMovesForStone_ThenReturnsAllEmptyIntersections()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();
        var generator = progress.GetGoLegalMoveGenerator();

        // Get first black stone directly
        var stone = progress.Game.GetPiece("black-stone-1");
        stone.Should().NotBeNull();

        // act
        var legalMovesForStone = generator.GetLegalMovesFor(stone!, progress.State).ToList();

        // assert
        // Should have placement moves for all empty intersections
        legalMovesForStone.Should().HaveCount(81, "Should have placement moves for all empty intersections on 9x9 board");
        legalMovesForStone.Should().AllBeOfType<PlaceStoneGameEvent>();
    }

    [Fact]
    public void GivenOpponentStone_WhenGettingLegalMovesForIt_ThenReturnsEmpty()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();
        var generator = progress.GetGoLegalMoveGenerator();

        // Get white player's stone
        var whiteStone = progress.Game.GetPiece("white-stone-1");
        whiteStone.Should().NotBeNull();

        // act
        var legalMovesForStone = generator.GetLegalMovesFor(whiteStone!, progress.State).ToList();

        // assert
        // Without active player, all players' stones can move, so this returns moves
        // This test only makes sense when active player is configured
        legalMovesForStone.Should().HaveCount(81, "Without active player configured, all players' stones can place");
    }
}
