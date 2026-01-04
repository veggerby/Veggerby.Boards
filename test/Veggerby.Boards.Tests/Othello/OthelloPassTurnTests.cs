using System.Linq;

using Veggerby.Boards.Othello;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Othello;

public class OthelloPassTurnTests
{
    [Fact]
    public void Should_allow_pass_turn_event()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        var passEvent = new PassTurnGameEvent();

        // act
        var newProgress = progress.HandleEvent(passEvent);

        // assert - should not throw
        newProgress.Should().NotBeNull();
    }

    [Fact]
    public void Should_switch_player_after_pass()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        progress.State.TryGetActivePlayer(out var initialPlayer).Should().BeTrue();
        initialPlayer!.Id.Should().Be(OthelloIds.Players.Black);

        var passEvent = new PassTurnGameEvent();

        // act
        var newProgress = progress.HandleEvent(passEvent);

        // assert
        newProgress.State.TryGetActivePlayer(out var nextPlayer).Should().BeTrue();
        nextPlayer!.Id.Should().Be(OthelloIds.Players.White);
    }

    [Fact]
    public void Should_allow_consecutive_passes_from_both_players()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        var passEvent = new PassTurnGameEvent();

        // act - Black passes
        progress = progress.HandleEvent(passEvent);
        progress.State.TryGetActivePlayer(out var player1).Should().BeTrue();
        player1!.Id.Should().Be(OthelloIds.Players.White);

        // act - White passes
        progress = progress.HandleEvent(passEvent);

        // assert - Should be back to Black
        progress.State.TryGetActivePlayer(out var player2).Should().BeTrue();
        player2!.Id.Should().Be(OthelloIds.Players.Black);
    }
}
