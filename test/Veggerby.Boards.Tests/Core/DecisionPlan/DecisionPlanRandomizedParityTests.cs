using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

/// <summary>
/// Randomized parity harness. Generates short pseudo-random sequences of legal-ish moves (pawn single pushes only) and dice rolls
/// then asserts resulting state parity between legacy evaluator and DecisionPlan path.
/// Scope intentionally narrow (no captures, promotions) as a scaffold for future expansion.
/// </summary>
public class DecisionPlanRandomizedParityTests
{
    private static IEnumerable<IGameEvent> GenerateSequence(GameProgress startingProgress, int length, int seed)
    {
        var rnd = new System.Random(seed);
        var localProgress = startingProgress;
        for (int i = 0; i < length; i++)
        {
            // Alternate between move attempt and (synthetic) dice roll (dice roll only if a dice exists in game - in chess it doesn't, so we skip)
            if (i % 2 == 0)
            {
                // Attempt a pawn single forward push for a random white or black pawn in starting rows
                var white = rnd.Next(2) == 0;
                var pawnId = white ? $"white-pawn-{rnd.Next(1, 9)}" : $"black-pawn-{rnd.Next(1, 9)}";
                var piece = localProgress.Game.GetPiece(pawnId);
                if (piece is null)
                {
                    continue;
                }
                var pieceState = localProgress.State.GetState<PieceState>(piece);
                if (pieceState is null)
                {
                    continue;
                }
                // Determine forward direction: white moves north (rank increasing), black moves south (rank decreasing) in builder coordinate system.
                // Tiles encoded as tile-[file][rank]; file a-h, rank 1-8.
                var currentId = pieceState.CurrentTile.Id; // e.g., tile-b2
                if (currentId.Length != 7)
                {
                    continue; // unexpected format
                }
                var file = currentId[5];
                var rankChar = currentId[6];
                if (!char.IsDigit(rankChar))
                {
                    continue;
                }
                int rank = rankChar - '0';
                int targetRank = white ? rank + 1 : rank - 1;
                if (targetRank < 1 || targetRank > 8)
                {
                    continue;
                }
                var targetTileId = $"{file}{targetRank}"; // rely on GameExtensions auto tile- prefix normalization
                // Use Move extension (no-op if illegal). We'll emit the event form for parity deterministic reproduction.
                // Resolve a path (like Move does) selecting first shortest candidate.
                // Use extension method to perform move; if illegal it will not change state so we instead manually resolve path for deterministic event sequences.
                var targetTile = localProgress.Game.GetTile(targetTileId);
                if (targetTile is null) { continue; }
                var relation = localProgress.Game.Board.TileRelations.FirstOrDefault(r => r.From.Equals(pieceState.CurrentTile) && r.To.Equals(targetTile));
                if (relation is null) { continue; }
                var path = new TilePath([relation]);
                var evt = new MovePieceGameEvent(piece, path);
                // apply to local progress so subsequent events use updated positions
                localProgress = localProgress.HandleEvent(evt);
                yield return evt;
            }
            else
            {
                // No dice in chess module; placeholder for future modules.
                continue;
            }
        }
    }

    [Fact]
    public void GivenRandomShortSequences_WhenEvaluatedWithAndWithoutDecisionPlan_ThenParityMaintained()
    {
        // arrange
        const int sequences = 25; // lightweight
        const int length = 6;
        for (int seed = 1; seed <= sequences; seed++)
        {
            GameProgress legacy;
            GameProgress plan;
            using (new FeatureFlagScope(decisionPlan: false))
            {
                legacy = new ChessGameBuilder().Compile();
            }
            using (new FeatureFlagScope(decisionPlan: true))
            {
                plan = new ChessGameBuilder().Compile();
            }
            var events = GenerateSequence(legacy, length, seed).ToList();

            // act
            var legacyProgress = legacy;
            var planProgress = plan;
            foreach (var e in events)
            {
                legacyProgress = legacyProgress.HandleEvent(e);
                planProgress = planProgress.HandleEvent(e);
            }

            // assert basic parity: piece positions for all pawns
            foreach (var pawnId in Enumerable.Range(1, 8).Select(i => $"white-pawn-{i}").Concat(Enumerable.Range(1, 8).Select(i => $"black-pawn-{i}")))
            {
                var pawn = plan.Game.GetPiece(pawnId);
                if (pawn is null) { continue; }
                var legacyState = legacyProgress.State.GetState<PieceState>(pawn);
                var planState = planProgress.State.GetState<PieceState>(pawn);
                if (legacyState is null && planState is null) { continue; }
                legacyState.Should().NotBeNull();
                planState.Should().NotBeNull();
                planState.CurrentTile.Should().Be(legacyState.CurrentTile, $"Pawn {pawnId} diverged (seed {seed})");
            }
            if (legacyProgress.State.Hash.HasValue && planProgress.State.Hash.HasValue)
            {
                planProgress.State.Hash.Should().Be(legacyProgress.State.Hash, $"State hash diverged (seed {seed})");
            }
        }
    }
}