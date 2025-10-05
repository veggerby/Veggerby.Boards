using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.States;

public class GameStateExtensionsActivePlayerTests
{
    [Fact]
    public void GivenExactlyOneActive_WhenTryGetActivePlayer_ThenReturnsTrueAndPlayer()
    {
        // arrange
        var progress = new TestGameBuilder().Compile();
        var players = progress.Game.Players.ToArray();
        var p1 = players[0];
        var p2 = players[1];
        var initial = progress.State.Next([
            new ActivePlayerState(p1, true),
            new ActivePlayerState(p2, false)
        ]);

        // act
        var ok = initial.TryGetActivePlayer(out var active);

        // assert
        ok.Should().BeTrue();
        active.Should().Be(p1);
    }

    [Fact]
    public void GivenNoActive_WhenTryGetActivePlayer_ThenReturnsFalse()
    {
        // arrange
        var progress = new TestGameBuilder().Compile();
        var players = progress.Game.Players.ToArray();
        var p1 = players[0];
        var p2 = players[1];
        var initial = progress.State.Next([
            new ActivePlayerState(p1, false),
            new ActivePlayerState(p2, false)
        ]);

        // act
        var ok = initial.TryGetActivePlayer(out var active);

        // assert
        ok.Should().BeFalse();
        active.Should().BeNull();
    }

    [Fact]
    public void GivenMultipleActive_WhenTryGetActivePlayer_ThenReturnsFalse()
    {
        // arrange
        var progress = new TestGameBuilder().Compile();
        var players = progress.Game.Players.ToArray();
        var p1 = players[0];
        var p2 = players[1];
        var initial = progress.State.Next([
            new ActivePlayerState(p1, true),
            new ActivePlayerState(p2, true)
        ]);

        // act
        var ok = initial.TryGetActivePlayer(out var active);

        // assert
        ok.Should().BeFalse();
        active.Should().BeNull();
    }
}