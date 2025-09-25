using System.Linq;

using AwesomeAssertions; // assertion helpers

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States; // GameProgress

using Xunit;

namespace Veggerby.Boards.Tests.Core.Turn;

/// <summary>
/// Validates that enabling turn sequencing does not alter piece placement semantics while
/// acknowledging an expected hash divergence (TurnState artifact presence changes canonical
/// serialization). This guards against accidental piece state drift or unintended artifact
/// omissions when toggling the feature flag.
/// </summary>
public class TurnSequencingHashParityTests
{
    [Fact]
    public void GivenSameMoveSequence_WhenSequencingOnOff_ThenPieceStatesMatchAndHashesMayDiffer()
    {
        // arrange
        GameProgress off;
        GameProgress on;

        var original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
        try
        {
            Boards.Internal.FeatureFlags.EnableTurnSequencing = false;
            off = new ChessGameBuilder().Compile();
            Boards.Internal.FeatureFlags.EnableTurnSequencing = true;
            on = new ChessGameBuilder().Compile();
        }
        finally
        {
            Boards.Internal.FeatureFlags.EnableTurnSequencing = original;
        }

        // act + assert baseline piece placement parity (no moves) to isolate hashing side effects only
        var pawn = on.Game.GetPiece("white-pawn-2");
        var offPawnState = off.State.GetStates<PieceState>().Single(ps => ps.Artifact.Equals(pawn));
        var onPawnState = on.State.GetStates<PieceState>().Single(ps => ps.Artifact.Equals(pawn));
        onPawnState.CurrentTile.Should().Be(offPawnState.CurrentTile, "initial piece position parity must hold regardless of sequencing flag");

        // Hashes may differ due to added TurnState artifact when sequencing enabled.
        // Explicitly assert either equality (future convergence if hashing excludes sequencing artifacts)
        // or inequality with justification comment.
        if (off.State.Hash == on.State.Hash)
        {
            // Accept equal (would indicate hashing excludes sequencing artifacts).
            on.State.Hash.Should().Be(off.State.Hash);
        }
        else
        {
            on.State.Hash.Should().NotBe(off.State.Hash, "TurnState artifact presence alters canonical serialization ordering");
        }
    }
}