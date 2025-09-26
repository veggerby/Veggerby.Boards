using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Simulation;

/// <summary>
/// Convenience helpers for constructing basic <see cref="IPlayoutPolicy"/> instances.
/// </summary>
/// <remarks>
/// These helpers deliberately avoid game-specific heuristics; they provide generic enumeration of simple piece moves
/// following single relation steps from each piece's current tile. They rely on engine rules to validate legality
/// (conditions may reject or ignore moves). Ordering is deterministic by artifact / tile identifiers.
/// </remarks>
public static class PolicyHelpers
{
    private sealed class SingleStepAllPiecesPolicy : IPlayoutPolicy
    {
        public IEnumerable<IGameEvent> GetCandidateEvents(GameProgress progress)
        {
            var state = progress.State;
            var engine = progress.Engine;
            var game = engine.Game;

            // Order pieces deterministically (Id)
            var pieces = game.Artifacts.OfType<Piece>().OrderBy(p => p.Id, StringComparer.Ordinal);
            foreach (var piece in pieces)
            {
                var pieceState = state.GetState<PieceState>(piece);
                var from = pieceState.CurrentTile;
                // Acquire outgoing relations from board
                var relations = game.Board.TileRelations.Where(r => r.From.Equals(from));
                foreach (var rel in relations.OrderBy(r => r.To.Id, StringComparer.Ordinal))
                {
                    TilePath path;
                    try
                    {
                        path = new TilePath(new List<TileRelation> { rel });
                    }
                    catch
                    {
                        continue; // defensive: skip malformed relation sequences
                    }
                    yield return new MovePieceGameEvent(piece, path);
                }
            }
        }
    }

    /// <summary>
    /// Creates a policy that enumerates single-step move events for every piece along all outgoing relations from its current tile.
    /// </summary>
    public static IPlayoutPolicy SingleStepAllPieces() => new SingleStepAllPiecesPolicy();
}