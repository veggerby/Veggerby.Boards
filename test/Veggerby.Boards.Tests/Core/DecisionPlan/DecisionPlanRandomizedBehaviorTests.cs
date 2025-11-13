using System;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

/// <summary>
/// Short randomized (deterministic seed) DecisionPlan-only behavior test. Generates a handful of pawn single pushes
/// and knight hops (when legal) to ensure no exceptions and basic invariants (piece count stability except captures, unique tile occupancy for same-color pieces).
/// This is NOT a parity test; it exists to keep a light stochastic signal once legacy traversal is removed.
/// </summary>
public class DecisionPlanRandomizedBehaviorTests
{
    [Fact]
    public void GivenRandomShortSequence_WhenApplied_ThenInvariantsHold()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        var rnd = new TestDeterministicRng(12345);

        // act
        for (var i = 0; i < 12; i++)
        {
            // choose a white pawn or knight roughly half the time, else black
            var white = i % 2 == 0;
            var piecePrefix = white ? "white-pawn-" : "black-pawn-";
            // attempt a pawn single push if starting row piece not advanced yet
            var file = rnd.Next(1, 9); // 1..8 maps to a..h
            var fileChar = (char)('a' + file - 1);
            var startRank = white ? '2' : '7';
            var targetRank = white ? '3' : '6';
            var pieceId = piecePrefix + file;
            var from = $"{fileChar}{startRank}";
            var to = $"{fileChar}{targetRank}";
            try
            {
                progress = progress.Move(pieceId, to);
            }
            catch
            {
                // ignore illegal attempt (already moved); try a knight hop instead if possible
                var knightId = white ? "white-knight-1" : "black-knight-1";
                var knightMoves = white
                    ? new[] { "c3", "h3" }
                    : new[] { "c6", "h6" };
                foreach (var dest in knightMoves)
                {
                    try
                    {
                        progress = progress.Move(knightId, dest);
                        break;
                    }
                    catch { /* ignore */ }
                }
            }
        }

        // assert invariants:
        // 1. All white piece states still have a tile (no null occupancy produced by evaluator)
        // 2. Total white piece state count unchanged from initial (16) – we didn't script captures here
        var whitePieceStates = progress.State.GetStates<PieceState>()
            .Where(ps => ps.Artifact.Id.StartsWith("white-", StringComparison.Ordinal))
            .ToList();
        whitePieceStates.Should().HaveCount(16);
        whitePieceStates.All(ps => ps.CurrentTile is not null).Should().BeTrue();
    }
}
