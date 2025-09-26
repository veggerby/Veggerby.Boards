using System;
using System.Collections.Generic;
using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.Serialization;

/// <summary>
/// Tests for the internal <see cref="CanonicalStateSerializer"/> exercised indirectly via <see cref="GameState"/> hashing.
/// We validate deterministic ordering, stability across runs, and independence from artifact insertion order.
/// </summary>
public class CanonicalStateSerializerTests
{
    private sealed record DummyState(Artifact Artifact, int Value) : IArtifactState;

    private static (GameState state, Piece piece, Dice dice, TurnArtifact turn) BuildState(
        int pieceTileIndex,
        int diceValue,
        int turnNumber,
        TurnSegment segment,
        int dummyValue,
        IEnumerable<string> artifactInsertionOrder = null)
    {
        // Direct artifact construction (no builder required) – hashing only depends on Artifact.Id + state properties.
        var player = new Player("p1");
        var piece = new Piece("piece-1", player, Enumerable.Empty<IPattern>());
        var dice = new Dice("dice-1");
        var turnArtifact = new TurnArtifact("turn");
        var tileA = new Tile("tile-a");
        var tileB = new Tile("tile-b");
        var tile = pieceTileIndex == 0 ? tileA : tileB;

        var states = new List<IArtifactState>
        {
            new PieceState(piece, tile),
            new DiceState<int>(dice, diceValue),
            new TurnState(turnArtifact, turnNumber, segment),
            new DummyState(new ArtifactIdArtifact("dummy-artifact"), dummyValue)
        };

        if (artifactInsertionOrder is not null)
        {
            var map = states.ToDictionary(s => s.Artifact.Id, s => s, StringComparer.Ordinal);
            states = artifactInsertionOrder.Select(id => map[id]).ToList();
        }

        return (GameState.New(states), piece, dice, turnArtifact);
    }

    private sealed class ArtifactIdArtifact : Artifact
    {
        public ArtifactIdArtifact(string id) : base(id) { }
    }

    [Fact]
    public void GivenIdenticalLogicalStates_WhenHashesComputedTwice_ThenHashesAreEqual()
    {
        // arrange
        ulong? firstHash;
        ulong? secondHash;
        using (new FeatureFlagScope(hashing: true))
        {
            var s1 = BuildState(0, 3, 1, TurnSegment.Main, 42).state;
            var s2 = BuildState(0, 3, 1, TurnSegment.Main, 42).state; // rebuilt fresh
            firstHash = s1.Hash;
            secondHash = s2.Hash;
        }

        // act // (already computed inside GameState construction when hashing enabled)

        // assert
        firstHash.HasValue.Should().BeTrue();
        secondHash.HasValue.Should().BeTrue();
        firstHash.Should().Be(secondHash);
    }

    [Fact]
    public void GivenDifferentInsertionOrder_WhenHashesComputed_ThenHashesAreEqual()
    {
        // arrange
        ulong? orderedHash;
        ulong? shuffledHash;
        using (new FeatureFlagScope(hashing: true))
        {
            var ordered = BuildState(1, 5, 2, TurnSegment.Start, 7).state;
            orderedHash = ordered.Hash;

            // Build with same logical artifact states but reversed insertion order
            var reversedOrder = new[] { "dummy-artifact", "turn", "dice-1", "piece-1" }; // reverse of natural build order
            var shuffled = BuildState(1, 5, 2, TurnSegment.Start, 7, reversedOrder).state;
            shuffledHash = shuffled.Hash;
        }

        // act // hashing done during construction

        // assert
        orderedHash.HasValue.Should().BeTrue();
        shuffledHash.HasValue.Should().BeTrue();
        orderedHash.Should().Be(shuffledHash);
    }

    [Fact]
    public void GivenSinglePropertyChange_WhenHashesComputed_ThenHashDiffers()
    {
        // arrange
        ulong? baseHash;
        ulong? changedHash;
        using (new FeatureFlagScope(hashing: true))
        {
            var baseline = BuildState(0, 4, 3, TurnSegment.End, 10).state;
            baseHash = baseline.Hash;
            var changed = BuildState(0, 5, 3, TurnSegment.End, 10).state; // dice value changed
            changedHash = changed.Hash;
        }

        // act

        // assert
        baseHash.HasValue.Should().BeTrue();
        changedHash.HasValue.Should().BeTrue();
        changedHash.Should().NotBe(baseHash);
    }

    [Fact]
    public void GivenHashingDisabled_WhenStateCreated_ThenHashIsNull()
    {
        // arrange
        ulong? hash;
        using (new FeatureFlagScope(hashing: false))
        {
            var state = BuildState(0, 1, 1, TurnSegment.Main, 1).state;
            hash = state.Hash;
        }

        // act

        // assert
        hash.Should().BeNull();
    }
}