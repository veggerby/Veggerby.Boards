using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Turn;

/// <summary>
/// Validates shadow-mode scaffolding of <see cref="TurnState"/> inserted during <c>GameBuilder.Compile</c>.
/// </summary>
public class TurnStateScaffoldingTests
{
    [Fact]
    public void GivenCompiledGame_WhenInspectingInitialState_ThenTurnStateIsPresent()
    {
        // arrange
        var builder = new ChessGameBuilder(); // any concrete builder suffices

        // act
        var progress = builder.Compile();
        var state = progress.State;

        // assert
        var turnState = state.GetStates<TurnState>().SingleOrDefault();
        turnState.Should().NotBeNull();
        turnState.TurnNumber.Should().Be(1);
        turnState.Segment.Should().Be(TurnSegment.Start);
    }
}