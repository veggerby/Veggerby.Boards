using System;
using System.Linq;
using System.Text.RegularExpressions;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Validates that every intermediate tile (excluding origin and destination) on a movement path is empty.
/// </summary>
/// <remarks>
/// Applies to sliding style movement (rook, bishop, queen in chess). Knights and single-step non-repeating moves
/// naturally have no intermediate tiles and therefore always pass. Destination tile occupancy (capture rules) is
/// intentionally ignored here and should be handled by a separate condition or mutator logic when capture semantics
/// are introduced.
/// </remarks>
public sealed class PathNotObstructedGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var relations = @event.Path.Relations.ToList();

        // If path encoded as a single relation with distance > 1 (sliding fast-path) we still need to
        // verify intermediate occupancy by reconstructing hop tiles.
        if (relations.Count == 1)
        {
            var single = relations[0];
            if (single.Distance <= 1)
            {
                return ConditionResponse.Valid; // single step
            }

            // Reconstruct intermediate tiles by walking board relations stepwise in same direction.
            var current = single.From;
            var stepsValidated = 0;
            while (stepsValidated < single.Distance - 1) // exclude destination
            {
                var relation = engine.Game.Board.GetTileRelation(current, single.Direction);
                if (relation is null)
                {
                    break; // fall back to coordinate approach
                }
                current = relation.To;
                stepsValidated++;
                if (state.GetPiecesOnTile(current).Any())
                {
                    return ConditionResponse.Ignore($"Path blocked at {current.Id}");
                }
            }

            if (stepsValidated < single.Distance - 1)
            {
                // Fallback: attempt coordinate-based interpolation for chess-style tile ids (tile-[a-h][1-8]).
                var pattern = new Regex("^tile-([a-h])(\\d)$");
                var fromMatch = pattern.Match(single.From.Id);
                var toMatch = pattern.Match(single.To.Id);
                if (fromMatch.Success && toMatch.Success)
                {
                    int fx = fromMatch.Groups[1].Value[0] - 'a';
                    int fy = int.Parse(fromMatch.Groups[2].Value) - 1;
                    int tx = toMatch.Groups[1].Value[0] - 'a';
                    int ty = int.Parse(toMatch.Groups[2].Value) - 1;

                    int dx = Math.Sign(tx - fx);
                    int dy = Math.Sign(ty - fy);
                    int max = Math.Max(Math.Abs(tx - fx), Math.Abs(ty - fy));
                    for (int step = 1; step < max; step++)
                    {
                        var ix = fx + dx * step;
                        var iy = fy + dy * step;
                        var tileId = $"tile-{(char)('a' + ix)}{iy + 1}";
                        var tile = engine.Game.Board.Tiles.SingleOrDefault(t => t.Id == tileId);
                        if (tile is not null && state.GetPiecesOnTile(tile).Any())
                        {
                            return ConditionResponse.Ignore($"Path blocked at {tile.Id}");
                        }
                    }
                }
            }
            return ConditionResponse.Valid;
        }

        // Determine if movement is sliding (all relations share identical direction).
        var sliding = relations.Select(r => r.Direction).Distinct().Count() == 1;
        if (!sliding)
        {
            // Treat non-sliding multi-relation paths (knight style leaps / compound patterns) as inherently unobstructed.
            return ConditionResponse.Valid;
        }

        var tiles = @event.Path.Tiles.ToList();
        if (tiles.Count <= 2)
        {
            return ConditionResponse.Valid;
        }

        for (var i = 1; i < tiles.Count - 1; i++)
        {
            if (state.GetPiecesOnTile(tiles[i]).Any())
            {
                // Ignore (do not invalidate) so the move is simply not applied; engine treats as no-op without raising.
                return ConditionResponse.Ignore($"Path blocked at {tiles[i].Id}");
            }
        }

        return ConditionResponse.Valid;
    }
}