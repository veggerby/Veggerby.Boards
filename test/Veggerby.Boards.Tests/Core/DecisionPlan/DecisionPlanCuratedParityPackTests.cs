using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

/// <summary>
/// Curated dual-run parity pack comparing legacy traversal vs DecisionPlan evaluator across representative scenarios.
/// Ensures enabling DecisionPlan does not alter externally observable game outcomes.
/// </summary>
public class DecisionPlanCuratedParityPackTests
{
    // Contract:
    // For each scenario we run with decisionPlan: false then true.
    // We collect final piece tile mapping, history depth, and (when hashing on) state hash values.
    // External observable equivalence required.

    private record ScenarioResult(Dictionary<string, string> PieceToTile, ulong? Hash, (ulong hi, ulong lo)? Hash128);

    private static ScenarioResult Run(bool decisionPlan, System.Action<Veggerby.Boards.States.GameProgress> sequence, bool hashing)
    {
        using var _ = new FeatureFlagScope(decisionPlan: decisionPlan, hashing: hashing, compiledPatterns: true);
        var builder = new TestGameBuilder(useSimpleGamePhase: false);
        var progress = builder.Compile();
        sequence(progress);
        var last = progress.State;
        // derive piece positions from known test piece ids (builder creates piece-1, piece-2, piece-n, piece-x, piece-y)
        var ids = new[] { "piece-1", "piece-2", "piece-n", "piece-x", "piece-y" };
        var pieceMap = new Dictionary<string, string>(ids.Length);
        foreach (var id in ids)
        {
            var piece = progress.Game.GetPiece(id);
            if (piece != null)
            {
                var ps = last.GetState<Veggerby.Boards.States.PieceState>(piece);
                if (ps != null)
                {
                    pieceMap[id] = ps.CurrentTile.Id;
                }
            }
        }
        return new ScenarioResult(pieceMap, last.Hash, last.Hash128);
    }

    private static IEnumerable<(string Name, System.Action<Veggerby.Boards.States.GameProgress> Sequence)> Scenarios()
    {
        yield return ("single-move", gp =>
        {
            // move piece-1 from tile-1 -> tile-2 if path exists
            var piece = gp.Game.GetPiece("piece-1");
            var from = gp.Game.GetTile("tile-1");
            var to = gp.Game.GetTile("tile-2");
            var relation = gp.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
            var path = new TilePath([relation]);
            gp.HandleEvent(new MovePieceGameEvent(piece, path));
        }
        );

        yield return ("branching-two-moves", gp =>
        {
            // piece-1: tile-1 -> tile-2 (clockwise)
            var piece1 = gp.Game.GetPiece("piece-1");
            var t1 = gp.Game.GetTile("tile-1");
            var t2 = gp.Game.GetTile("tile-2");
            var rel12 = gp.Game.Board.TileRelations.Single(r => r.From.Equals(t1) && r.To.Equals(t2));
            gp.HandleEvent(new MovePieceGameEvent(piece1, new TilePath([rel12])));

            // piece-2: tile-2 -> tile-1 (counterclockwise)
            var piece2 = gp.Game.GetPiece("piece-2");
            var rel21 = gp.Game.Board.TileRelations.Single(r => r.From.Equals(t2) && r.To.Equals(t1));
            gp.HandleEvent(new MovePieceGameEvent(piece2, new TilePath([rel21])));
        }
        );

        yield return ("illegal-event-ignored", gp =>
        {
            // piece-1 does not have the 'up' direction so moving along tile-1 -> tile-3 (up) should be rejected/ignored consistently.
            var piece = gp.Game.GetPiece("piece-1");
            var t1 = gp.Game.GetTile("tile-1");
            var t3 = gp.Game.GetTile("tile-3");
            var rel13 = gp.Game.Board.TileRelations.Single(r => r.From.Equals(t1) && r.To.Equals(t3));
            gp.HandleEvent(new MovePieceGameEvent(piece, new TilePath([rel13])));
        }
        );

        yield return ("hash-stability-sequence", gp =>
        {
            // For each movable piece, attempt a legal single-step move from its current tile if a relation exists.
            var movable = new[] { "piece-1", "piece-2" };
            foreach (var id in movable)
            {
                var piece = gp.Game.GetPiece(id);
                var state = gp.State.GetState<Veggerby.Boards.States.PieceState>(piece);
                if (state == null)
                {
                    continue;
                }
                var current = state.CurrentTile;
                var outgoing = gp.Game.Board.TileRelations.FirstOrDefault(r => r.From.Equals(current));
                if (outgoing != null)
                {
                    gp.HandleEvent(new MovePieceGameEvent(piece, new TilePath([outgoing])));
                }
            }
        }
        );
    }

    public static IEnumerable<object[]> ScenarioMatrix()
    {
        foreach (var scenario in Scenarios())
        {
            foreach (var hashing in new[] { false, true })
            {
                yield return new object[] { scenario.Name, scenario.Sequence, hashing };
            }
        }
    }

    [Theory]
    [MemberData(nameof(ScenarioMatrix))]
    public void GivenScenario_WhenDecisionPlanToggled_ThenExternalOutcomesMatch(string name, System.Action<Veggerby.Boards.States.GameProgress> sequence, bool hashing)
    {
        // arrange
        var legacy = Run(false, sequence, hashing);
        var plan = Run(true, sequence, hashing);

        // assert
        plan.PieceToTile.Should().BeEquivalentTo(legacy.PieceToTile, options => options.WithStrictOrdering());
        if (hashing)
        {
            plan.Hash.Should().Be(legacy.Hash, $"hash mismatch scenario={name}");
            plan.Hash128.Should().Be(legacy.Hash128, $"hash128 mismatch scenario={name}");
        }
    }
}
