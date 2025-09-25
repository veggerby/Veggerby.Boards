using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.TestHelpers;

/// <summary>
/// Provides a small, deterministic and self-contained opening sequence for chess tests.
/// The sequence is intentionally minimal (Ruy Lopez: e4 e5 Nf3 Nc6 Bb5) and avoids
/// any ambiguous rule semantics (no castling, en passant, promotions, or captures).
///
/// Purpose:
/// 1. Reuse across parity / benchmarking / invariant tests to reduce duplication.
/// 2. Provide a stable micro-workload for performance benchmarks (DecisionPlan vs legacy, etc.).
/// 3. Minimize drift risk if piece ids or path semantics evolve (single place to update).
///
/// Style Charter: file-scoped namespace, explicit braces, no LINQ in hot loops, immutable event list.
/// </summary>
internal static class DeterministicChessOpening
{
    /// <summary>
    /// Returns a deterministic list of <see cref="MovePieceGameEvent"/> representing a minimal Ruy Lopez opening:
    /// 1. e2 -> e4
    /// 2. e7 -> e5
    /// 3. g1 -> f3 (white knight)
    /// 4. b8 -> c6 (black knight)
    /// 5. f1 -> b5 (white bishop)
    ///
    /// If any path cannot be resolved (unexpected configuration), the returned list will contain
    /// only the successfully resolved prefix (never null). This defensive stance preserves test
    /// robustness against transient module configuration changes while keeping determinism.
    /// </summary>
    public static IReadOnlyList<MovePieceGameEvent> GetRuyLopezOpeningEvents(Game game)
    {
        var events = new List<MovePieceGameEvent>(5);

        // Ordered tuple: pieceId, from, to
        // Assumed piece id mapping: white-pawn-5 (file e), black-pawn-5 (file e), white-knight-2 (g1), black-knight-1 (b8), white-bishop-2 (f1)
        // NOTE: If numbering changes in future builder, update ids here (single authoritative place).
        var steps = new (string PieceId, string From, string To)[]
        {
            ("white-pawn-5", "e2", "e4"),
            ("black-pawn-5", "e7", "e5"),
            ("white-knight-2", "g1", "f3"),
            ("black-knight-1", "b8", "c6"),
            ("white-bishop-2", "f1", "b5")
        };

        for (int i = 0; i < steps.Length; i++)
        {
            var (pieceId, fromId, toId) = steps[i];
            var piece = game.GetPiece(pieceId);
            var from = game.GetTile(fromId);
            var to = game.GetTile(toId);
            if (piece is null || from is null || to is null)
            {
                break; // stop at first unresolved component
            }

            // Resolve path via pattern visitor (compiled-first resolver under flags will short-circuit internally if enabled)
            TilePath path = null;
            foreach (var pattern in piece.Patterns)
            {
                var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
                pattern.Accept(visitor);
                if (visitor.ResultPath is not null)
                {
                    path = visitor.ResultPath;
                    break;
                }
            }

            if (path is null)
            {
                break; // unresolved path – terminate prefix for deterministic partial sequence
            }

            events.Add(new MovePieceGameEvent(piece, path));
        }

        return events;
    }

    /// <summary>
    /// Applies the deterministic Ruy Lopez opening sequence to the supplied progress instance,
    /// returning the final <see cref="GameProgress"/> after all resolvable events are handled.
    /// </summary>
    public static GameProgress ApplyRuyLopezOpening(GameProgress progress)
    {
        var events = GetRuyLopezOpeningEvents(progress.Game);
        for (int i = 0; i < events.Count; i++)
        {
            progress = progress.HandleEvent(events[i]);
        }

        return progress;
    }
}