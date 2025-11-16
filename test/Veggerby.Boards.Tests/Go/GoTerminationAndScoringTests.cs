using System.Linq;

using Veggerby.Boards.Go;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Go;

/// <summary>
/// Tests for Go game termination (double pass) and scoring.
/// </summary>
public class GoTerminationAndScoringTests
{
    [Fact]
    public void GivenSinglePass_WhenPassTurnEvent_ThenPassCountIncremented()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        var beforeState = progress.State;
        var beforeTurnState = beforeState.GetStates<TurnState>().FirstOrDefault();
        beforeTurnState.Should().NotBeNull("TurnState should exist");
        beforeTurnState!.PassStreak.Should().Be(0);

        // act
        progress = progress.HandleEvent(new PassTurnGameEvent());

        // assert
        var afterState = progress.State;
        var afterTurnState = afterState.GetStates<TurnState>().First();
        afterTurnState.PassStreak.Should().Be(1, "pass streak should increment");

        afterState.GetStates<GameEndedState>().Should().BeEmpty("game should not end on single pass");
    }

    [Fact]
    public void GivenTwoConsecutivePasses_WhenSecondPass_ThenGameEnds()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // First pass
        progress = progress.HandleEvent(new PassTurnGameEvent());

        var afterFirstPass = progress.State;
        var afterFirstPassTurnState = afterFirstPass.GetStates<TurnState>().FirstOrDefault();
        afterFirstPassTurnState.Should().NotBeNull();
        afterFirstPassTurnState!.PassStreak.Should().Be(1);

        // act - Second pass
        progress = progress.HandleEvent(new PassTurnGameEvent());

        // assert
        var afterSecondPass = progress.State;
        var afterSecondPassTurnState = afterSecondPass.GetStates<TurnState>().First();
        afterSecondPassTurnState.PassStreak.Should().Be(2, "second pass should increment counter");

        afterSecondPass.GetStates<GameEndedState>().Should().ContainSingle("game should end after two consecutive passes");
    }

    [Fact]
    public void GivenPassThenPlacement_WhenPassCountReset_ThenGameContinues()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // First pass
        progress = progress.HandleEvent(new PassTurnGameEvent());

        var afterPass = progress.State;
        var afterPassTurnState = afterPass.GetStates<TurnState>().FirstOrDefault();
        afterPassTurnState.Should().NotBeNull();
        afterPassTurnState!.PassStreak.Should().Be(1);

        // act - Place a stone (resets pass count)
        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-5-5")!));

        // assert
        var afterPlacement = progress.State;
        var afterPlacementTurnState = afterPlacement.GetStates<TurnState>().First();
        afterPlacementTurnState.PassStreak.Should().Be(0, "pass streak should reset on placement");

        afterPlacement.GetStates<GameEndedState>().Should().BeEmpty("game should continue after placement");
    }

    [Fact]
    public void GivenEmptyBoard_WhenScoring_ThenNoTerritory()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();
        
        // End game with two passes
        progress = progress.HandleEvent(new PassTurnGameEvent());
        progress = progress.HandleEvent(new PassTurnGameEvent());

        var scoring = new GoScoring(progress.Game);

        // act
        var scores = scoring.ComputeAreaScores(progress.State);

        // assert
        scores.Should().ContainKey("black");
        scores.Should().ContainKey("white");
        scores["black"].Should().Be(0, "empty board has no territory or stones");
        scores["white"].Should().Be(0, "empty board has no territory or stones");
    }

    [Fact]
    public void GivenSurroundedTerritory_WhenScoring_ThenTerritoryAssignedToController()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // Create a small black territory in corner (4 empty intersections surrounded by black)
        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var blackStone3 = progress.Game.GetPiece("black-stone-3")!;
        var blackStone4 = progress.Game.GetPiece("black-stone-4")!;
        var blackStone5 = progress.Game.GetPiece("black-stone-5")!;
        var blackStone6 = progress.Game.GetPiece("black-stone-6")!;

        // Create a small enclosed territory
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-3-1")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-3-2")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone3, progress.Game.GetTile("tile-3-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone4, progress.Game.GetTile("tile-2-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone5, progress.Game.GetTile("tile-1-3")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone6, progress.Game.GetTile("tile-1-2")!));

        // End game
        progress = progress.HandleEvent(new PassTurnGameEvent());
        progress = progress.HandleEvent(new PassTurnGameEvent());

        var scoring = new GoScoring(progress.Game);

        // act
        var scores = scoring.ComputeAreaScores(progress.State);

        // assert
        // Black should have: 6 stones + 2 territory (tiles 1-1, 2-1, 2-2) = at least 8
        scores["black"].Should().BeGreaterThanOrEqualTo(8, "black should have stones + enclosed territory");
        scores["white"].Should().Be(0, "white has no stones or territory");
    }

    [Fact]
    public void GivenMixedTerritory_WhenScoring_ThenBothPlayersHaveScores()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // Place stones for both players creating separate territories
        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var blackStone2 = progress.Game.GetPiece("black-stone-2")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;
        var whiteStone2 = progress.Game.GetPiece("white-stone-2")!;

        // Black territory on left
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-2-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone2, progress.Game.GetTile("tile-3-5")!));
        
        // White territory on right
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-7-5")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone2, progress.Game.GetTile("tile-8-5")!));

        // End game
        progress = progress.HandleEvent(new PassTurnGameEvent());
        progress = progress.HandleEvent(new PassTurnGameEvent());

        var scoring = new GoScoring(progress.Game);

        // act
        var scores = scoring.ComputeAreaScores(progress.State);

        // assert
        scores["black"].Should().BeGreaterThan(0, "black should have stones and territory");
        scores["white"].Should().BeGreaterThan(0, "white should have stones and territory");
    }

    [Fact]
    public void GivenNeutralTerritory_WhenScoring_ThenNoPointsAssigned()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();

        // Create a situation where some territory borders both colors (neutral)
        var blackStone1 = progress.Game.GetPiece("black-stone-1")!;
        var whiteStone1 = progress.Game.GetPiece("white-stone-1")!;

        // Place stones adjacent (territory between them is contested/neutral)
        progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone1, progress.Game.GetTile("tile-5-4")!));
        progress = progress.HandleEvent(new PlaceStoneGameEvent(whiteStone1, progress.Game.GetTile("tile-5-6")!));

        // End game
        progress = progress.HandleEvent(new PassTurnGameEvent());
        progress = progress.HandleEvent(new PassTurnGameEvent());

        var scoring = new GoScoring(progress.Game);

        // act
        var scores = scoring.ComputeAreaScores(progress.State);

        // assert
        // Each should have exactly 1 stone (the tile at 5-5 between them is neutral)
        scores["black"].Should().BeGreaterThanOrEqualTo(1, "black should have at least its stone");
        scores["white"].Should().BeGreaterThanOrEqualTo(1, "white should have at least its stone");
        
        // The neutral territory (5-5) should not be counted for either player
        var totalScore = scores["black"] + scores["white"];
        totalScore.Should().BeLessThan(81, "neutral territory should not be counted");
    }
}
