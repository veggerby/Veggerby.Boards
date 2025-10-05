using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

#nullable enable

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Helper utility encapsulating movement path validation (currently obstruction detection for sliding moves).
/// </summary>
internal static class MovementPathValidator
{
    /// <summary>
    /// Determines whether any intermediate tile (excluding origin and destination) is occupied for a sliding path.
    /// </summary>
    public static (bool blocked, string? tileId) IsPathObstructed(GameState state, MovePieceGameEvent @event)
    {
        var relations = @event.Path.Relations.ToList();
        var sliding = relations.Select(r => r.Direction).Distinct().Count() == 1;
        if (!sliding)
        {
            return (false, null);
        }
        var tiles = @event.Path.Tiles.ToList();
        if (tiles.Count <= 2)
        {
            return (false, null);
        }
        for (var i = 1; i < tiles.Count - 1; i++)
        {
            if (state.GetPiecesOnTile(tiles[i]).Any())
            {
                return (true, tiles[i].Id);
            }
        }
        return (false, null);
    }
}