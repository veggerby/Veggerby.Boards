using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Random;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.Determinism;

/// <summary>
/// Randomized parity tests for deterministic replay validation using deterministic RNG seeds.
/// </summary>
/// <remarks>
/// These tests verify that identical initial states and event sequences produce identical state hashes,
/// regardless of execution timing or environmental differences. This is critical for replay validation
/// and cross-platform determinism enforcement.
/// </remarks>
public class RandomizedReplayDeterminismTests : HashParityTestFixture
{

    [Fact]
    public void GivenDeterministicSeed_WhenReplayingSequence_ThenHashesMatch()
    {
        // arrange

        // act

        // assert

        using var scope = new FeatureFlagScope(hashing: true);
        const ulong seed = 12345UL;

        var builder1 = new TestGameBuilder(useSimpleGamePhase: false);
        var builder2 = new TestGameBuilder(useSimpleGamePhase: false);

        // Build with same RNG seed
        var progress1 = builder1.WithSeed(seed).Compile();
        var progress2 = builder2.WithSeed(seed).Compile();

        // act - apply same event sequence
        var piece = progress1.Game.GetPiece("piece-1");
        var tile1 = progress1.Game.GetTile("tile-1");
        var tile2 = progress1.Game.GetTile("tile-2");
        var relation = progress1.Game.Board.TileRelations.Single(r => r.From.Equals(tile1) && r.To.Equals(tile2));
        var path = new TilePath([relation]);
        var move = new MovePieceGameEvent(piece!, path);

        progress1 = progress1.HandleEvent(move);
        progress2 = progress2.HandleEvent(move);

        // assert
        AssertHashParity(progress1, progress2, "Deterministic replay with RNG");
    }

    [Theory]
    [InlineData(42UL)]
    [InlineData(999UL)]
    [InlineData(7777UL)]
    public void GivenSameSeed_WhenApplyingMoves_ThenFinalHashesMatch(ulong seed)
    {
        // arrange

        // act

        // assert

        using var scope = new FeatureFlagScope(hashing: true);

        var builder1 = new TestGameBuilder(useSimpleGamePhase: false);
        var builder2 = new TestGameBuilder(useSimpleGamePhase: false);

        var progress1 = builder1.WithSeed(seed).Compile();
        var progress2 = builder2.WithSeed(seed).Compile();

        var piece = progress1.Game.GetPiece("piece-1");
        var tile1 = progress1.Game.GetTile("tile-1");
        var tile2 = progress1.Game.GetTile("tile-2");
        var tile3 = progress1.Game.GetTile("tile-3");

        // act - apply moves
        var relation1 = progress1.Game.Board.TileRelations.FirstOrDefault(r => r.From.Equals(tile1) && r.To.Equals(tile2));
        if (relation1 is not null)
        {
            var path1 = new TilePath([relation1]);
            var move1 = new MovePieceGameEvent(piece!, path1);
            progress1 = progress1.HandleEvent(move1);
            progress2 = progress2.HandleEvent(move1);
        }

        var relation2 = progress1.Game.Board.TileRelations.FirstOrDefault(r => r.From.Equals(tile2) && r.To.Equals(tile3));
        if (relation2 is not null)
        {
            var path2 = new TilePath([relation2]);
            var move2 = new MovePieceGameEvent(piece!, path2);
            progress1 = progress1.HandleEvent(move2);
            progress2 = progress2.HandleEvent(move2);
        }

        // assert
        AssertHashParity(progress1, progress2, $"Randomized replay with seed {seed}");
    }

    [Fact]
    public void GivenDifferentSeeds_WhenBuildingWithRNG_ThenHashesDiffer()
    {
        // arrange

        // act

        // assert

        using var scope = new FeatureFlagScope(hashing: true);

        var builder1 = new TestGameBuilder(useSimpleGamePhase: false);
        var builder2 = new TestGameBuilder(useSimpleGamePhase: false);

        // act - attach different RNG sources
        var progress1 = builder1.WithSeed(123UL).Compile();
        var progress2 = builder2.WithSeed(456UL).Compile();

        // assert
        progress1.State.Hash.Should().NotBe(progress2.State.Hash, "Different RNG seeds should produce different hashes");
        progress1.State.Hash128.Should().NotBe(progress2.State.Hash128, "Different RNG seeds should produce different hashes (128-bit)");
    }

    [Fact]
    public void GivenRNGProgression_WhenCloningState_ThenHashesReflectState()
    {
        // arrange

        // act

        // assert

        using var scope = new FeatureFlagScope(hashing: true);
        var builder = new TestGameBuilder(useSimpleGamePhase: false);
        var progress = builder.WithSeed(777UL).Compile();

        var initialHash = progress.State.Hash;
        var initialHash128 = progress.State.Hash128;

        // act - apply a move (state transition)
        var piece = progress.Game.GetPiece("piece-1");
        var tile1 = progress.Game.GetTile("tile-1");
        var tile2 = progress.Game.GetTile("tile-2");
        var relation = progress.Game.Board.TileRelations.First(r => r.From.Equals(tile1) && r.To.Equals(tile2));
        var path = new TilePath([relation]);
        var move = new MovePieceGameEvent(piece!, path);
        progress = progress.HandleEvent(move);

        // assert - hash should change after state transition
        progress.State.Hash.Should().NotBe(initialHash, "Hash should change after move");
        progress.State.Hash128.Should().NotBe(initialHash128, "Hash128 should change after move");
    }
}
