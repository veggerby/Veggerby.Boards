using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Core;

public class GameTimelineTests
{
    private GameState NewState(int marker)
    {
        // Reuse TestGameBuilder minimal state; attach marker via piece count modifications or dummy approach.
        // For simplicity, just build and rely on object identity differences after Next().
        var builder = new TestGameBuilder(useSimpleGamePhase: false);
        var progress = builder.Compile();
        // fabricate a unique state chain by calling Next with empty modifications marker times
        var state = progress.State;
        for (int i = 0; i < marker; i++)
        {
            state = state.Next(System.Linq.Enumerable.Empty<IArtifactState>());
        }
        return state;
    }

    [Fact]
    public void Create_Should_Set_Present_And_Empty_PastFuture()
    {
        var s0 = NewState(0);
        var tl = GameTimeline.Create(s0);
        tl.Past.Should().BeEmpty();
        tl.Present.Should().Be(s0);
        tl.Future.Should().BeEmpty();
    }

    [Fact]
    public void Push_Should_Move_Present_To_Past()
    {
        var s0 = NewState(0);
        var s1 = NewState(1);
        var tl = GameTimeline.Create(s0).Push(s1);
        tl.Past.Should().ContainSingle().Which.Should().Be(s0);
        tl.Present.Should().Be(s1);
        tl.Future.Should().BeEmpty();
    }

    [Fact]
    public void Undo_Should_Move_Present_To_Future()
    {
        var s0 = NewState(0);
        var s1 = NewState(1);
        var tl = GameTimeline.Create(s0).Push(s1);
        var undone = tl.Undo();
        undone.Present.Should().Be(s0);
        undone.Past.Should().BeEmpty();
        undone.Future.Should().ContainSingle().Which.Should().Be(s1);
    }

    [Fact]
    public void Redo_Should_Move_From_Future_To_Present()
    {
        var s0 = NewState(0);
        var s1 = NewState(1);
        var tl = GameTimeline.Create(s0).Push(s1).Undo();
        var redone = tl.Redo();
        redone.Present.Should().Be(s1);
        redone.Past.Should().ContainSingle().Which.Should().Be(s0);
        redone.Future.Should().BeEmpty();
    }

    [Fact]
    public void Push_After_Undo_Should_Clear_Future()
    {
        var s0 = NewState(0);
        var s1 = NewState(1);
        var s2 = NewState(2);
        var tl = GameTimeline.Create(s0).Push(s1).Undo();
        var branched = tl.Push(s2);
        branched.Future.Should().BeEmpty();
        branched.Past.Should().ContainSingle().Which.Should().Be(s0);
        branched.Present.Should().Be(s2);
    }
}