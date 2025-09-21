using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Random;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Core.States;

public class GameStateRandomTests
{
    private sealed class DummyArtifact : Artifact
    {
        public DummyArtifact(string id) : base(id) { }
    }

    private sealed record DummyArtifactState(DummyArtifact ArtifactRef) : IArtifactState
    {
        public Artifact Artifact => ArtifactRef;
    }

    [Fact]
    public void GivenStateWithRandom_WhenNextCalled_ThenRandomCloned()
    {
        // Arrange
        var rng = XorShiftRandomSource.Create(123UL);
        var artifact = new DummyArtifact("a1");
        var state = GameState.New([new DummyArtifactState(artifact)], rng);

        // Act
        var next = state.Next([]);

        // Assert
        next.Random.Should().NotBeNull();
        next.Random.Should().NotBeSameAs(state.Random);
        next.Random.Seed.Should().Be(state.Random.Seed);
    }

    [Fact]
    public void GivenState_WhenWithRandomCalled_ThenRandomReplaced()
    {
        // Arrange
        var artifact = new DummyArtifact("a1");
        var state = GameState.New([new DummyArtifactState(artifact)]);
        var rng = XorShiftRandomSource.Create(999UL);

        // Act
        var updated = state.WithRandom(rng);

        // Assert
        updated.Random.Should().NotBeNull();
        updated.Random.Seed.Should().Be(999UL);
    }
}