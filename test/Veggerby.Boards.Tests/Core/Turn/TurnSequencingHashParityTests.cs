using System;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Turn;

/// <summary>
/// Validates piece placement parity and documents expected hash divergence caused solely by TurnState presence
/// when the sequencing feature flag is enabled. Also exercises pass-based turn advancement as a stand‑in for a
/// future single-segment (Main-only) profile; actual profile customization is deferred until the engine exposes
/// a supported configuration hook.
/// </summary>
public class TurnSequencingHashParityTests
{
    private sealed class FlagScope : IDisposable
    {
        private readonly bool _original;
        public FlagScope(bool enable)
        {
            _original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
            Boards.Internal.FeatureFlags.EnableTurnSequencing = enable;
        }
        public void Dispose()
        {
            Boards.Internal.FeatureFlags.EnableTurnSequencing = _original;
        }
    }

    private static GameProgress ApplyOpening(GameProgress progress)
    {
        progress = progress.Move("white-pawn-5", "e4");
        progress = progress.Move("black-pawn-5", "e5");
        return progress;
    }

    [Fact]
    public void GivenSameMoves_WhenSequencingOffAndOn_ThenPiecePositionsMatchAndTurnStateExplainsHashDelta()
    {
        GameProgress offProgress;
        GameProgress onProgress;

        using (new FlagScope(false))
        {
            offProgress = ApplyOpening(new ChessGameBuilder().Compile());
        }
        using (new FlagScope(true))
        {
            onProgress = ApplyOpening(new ChessGameBuilder().Compile());
        }

        var offPieces = offProgress.State.GetStates<PieceState>().OrderBy(p => p.Artifact.Id)
            .Select(p => (p.Artifact.Id, p.CurrentTile.Id)).ToArray();
        var onPieces = onProgress.State.GetStates<PieceState>().OrderBy(p => p.Artifact.Id)
            .Select(p => (p.Artifact.Id, p.CurrentTile.Id)).ToArray();

        onPieces.Should().BeEquivalentTo(offPieces, o => o.WithStrictOrdering());

        var offHash = offProgress.State.Hash;
        var onHash = onProgress.State.Hash;
        if (offHash != onHash)
        {
            offProgress.State.GetStates<TurnState>().Should().BeEmpty();
            onProgress.State.GetStates<TurnState>().Should().NotBeEmpty();
        }
    }

    [Fact]
    public void GivenSequencingEnabled_WhenPassingTurn_ThenTurnNumberIncrementsAndSegmentResets()
    {
        using var _ = new FlagScope(true);
        var progress = new ChessGameBuilder().Compile();
        var before = progress.State.GetStates<TurnState>().FirstOrDefault();
        before.Should().NotBeNull();
        var mutator = new TurnPassStateMutator();
        var updatedState = mutator.MutateState(progress.Engine, progress.State, new TurnPassEvent());
        var after = updatedState.GetStates<TurnState>().First();
        after.TurnNumber.Should().Be(before!.TurnNumber + 1);
        after.Segment.Should().Be(TurnSegment.Start);
    }
}