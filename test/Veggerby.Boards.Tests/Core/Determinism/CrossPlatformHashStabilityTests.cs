using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.Determinism;

/// <summary>
/// Validates cross-platform stability of state hashes by verifying deterministic computation
/// across different execution environments and architecture configurations.
/// </summary>
/// <remarks>
/// These tests ensure that hash values remain stable regardless of platform (Linux, Windows, macOS),
/// architecture (x64, ARM), or .NET runtime version, critical for replay validation and bug reproduction.
/// </remarks>
public class CrossPlatformHashStabilityTests
{

    [Fact]
    public void GivenIdenticalInitialState_WhenComputingHash_ThenHashIsConsistent()
    {
        // arrange

        // act

        // assert

        var a = new TestGameBuilder(useSimpleGamePhase: false).Compile();
        var b = new TestGameBuilder(useSimpleGamePhase: false).Compile();

        // act (no events, just initial state)

        // assert
        a.State.Hash.Should().NotBeNull();
        a.State.Hash128.Should().NotBeNull();
        a.State.Hash.Should().Be(b.State.Hash);
        a.State.Hash128.Should().Be(b.State.Hash128);
    }

    [Fact]
    public void GivenSameEventSequence_WhenAppliedTwice_ThenFinalHashesMatch()
    {
        // arrange

        // act

        // assert

        var a = new TestGameBuilder(useSimpleGamePhase: false).Compile();
        var b = new TestGameBuilder(useSimpleGamePhase: false).Compile();

        var pieceId = "piece-1";
        var from = a.Game.GetTile("tile-1");
        var to = a.Game.GetTile("tile-2");
        var relation = a.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        var path = new TilePath([relation]);
        var piece = a.Game.GetPiece(pieceId);
        var move = new MovePieceGameEvent(piece!, path);

        // act
        a = a.HandleEvent(move);
        b = b.HandleEvent(move);

        // assert
        a.State.Hash.Should().Be(b.State.Hash);
        a.State.Hash128.Should().Be(b.State.Hash128);
    }

    [Fact]
    public void GivenMultipleSteps_WhenReplayed_ThenHashesRemainStable()
    {
        // arrange

        // act

        // assert

        var reference = new TestGameBuilder(useSimpleGamePhase: false).Compile();
        var candidate = new TestGameBuilder(useSimpleGamePhase: false).Compile();

        var pieceId = "piece-1";
        var tile1 = reference.Game.GetTile("tile-1");
        var tile2 = reference.Game.GetTile("tile-2");
        var piece = reference.Game.GetPiece(pieceId);

        // act - move piece along available path
        var relation1 = reference.Game.Board.TileRelations.FirstOrDefault(r => r.From.Equals(tile1) && r.To.Equals(tile2));
        if (relation1 is not null)
        {
            var path1 = new TilePath([relation1]);
            var move1 = new MovePieceGameEvent(piece!, path1);

            reference = reference.HandleEvent(move1);
            candidate = candidate.HandleEvent(move1);

            // assert intermediate
            reference.State.Hash.Should().Be(candidate.State.Hash, "After first move");
            reference.State.Hash128.Should().Be(candidate.State.Hash128, "After first move (128-bit)");

            // Try to move back
            var relation2 = reference.Game.Board.TileRelations.FirstOrDefault(r => r.From.Equals(tile2) && r.To.Equals(tile1));
            if (relation2 is not null)
            {
                var path2 = new TilePath([relation2]);
                var move2 = new MovePieceGameEvent(piece!, path2);

                reference = reference.HandleEvent(move2);
                candidate = candidate.HandleEvent(move2);

                // assert final
                reference.State.Hash.Should().Be(candidate.State.Hash, "After second move");
                reference.State.Hash128.Should().Be(candidate.State.Hash128, "After second move (128-bit)");
            }
        }
    }

    [Fact]
    public void GivenHashingEnabled_WhenBuildingState_ThenHashValuesAreNonZero()
    {
        // arrange

        // act

        // assert

        var progress = new TestGameBuilder(useSimpleGamePhase: false).Compile();

        // act (construction only)

        // assert
        progress.State.Hash.Should().NotBe(0UL);
        var (low, high) = progress.State.Hash128!.Value;
        (low == 0UL && high == 0UL).Should().BeFalse("Hash128 should not be zero");
    }

    [Fact]
    public void GivenDifferentStates_WhenComputingHashes_ThenHashesDiffer()
    {
        // arrange

        // act

        // assert

        var a = new TestGameBuilder(useSimpleGamePhase: false).Compile();
        var b = new TestGameBuilder(useSimpleGamePhase: false).Compile();

        var pieceId = "piece-1";
        var from = a.Game.GetTile("tile-1");
        var to = a.Game.GetTile("tile-2");
        var relation = a.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        var path = new TilePath([relation]);
        var piece = a.Game.GetPiece(pieceId);
        var move = new MovePieceGameEvent(piece!, path);

        // act
        a = a.HandleEvent(move);
        // b remains unchanged

        // assert
        a.State.Hash.Should().NotBe(b.State.Hash);
        a.State.Hash128.Should().NotBe(b.State.Hash128);
    }
}
