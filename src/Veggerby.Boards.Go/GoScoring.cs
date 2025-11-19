using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go;

/// <summary>
/// Provides scoring utilities for Go games using area scoring rules.
/// Area scoring: each player's score = stones on board + empty intersections controlled.
/// </summary>
public sealed class GoScoring
{
    private readonly Game _game;

    /// <summary>
    /// Creates a new <see cref="GoScoring"/> instance for the specified game.
    /// </summary>
    /// <param name="game">The Go game instance.</param>
    public GoScoring(Game game)
    {
        _game = game ?? throw new System.ArgumentNullException(nameof(game));
    }

    /// <summary>
    /// Computes area scores for both players.
    /// Area score = stones on board + controlled empty territory.
    /// </summary>
    /// <param name="state">Terminal game state (after two passes).</param>
    /// <returns>Dictionary mapping player ID to score.</returns>
    public Dictionary<string, int> ComputeAreaScores(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var pieceStates = state.GetStates<PieceState>().ToList();
        var allTiles = _game.Board.Tiles.ToList();
        var scores = new Dictionary<string, int>();

        // Initialize scores for all players
        foreach (var player in _game.Players)
        {
            scores[player.Id] = 0;
        }

        // Count stones on board
        foreach (var pieceState in pieceStates)
        {
            var owner = pieceState.Artifact.Owner;
            if (owner != null && scores.ContainsKey(owner.Id))
            {
                scores[owner.Id]++;
            }
        }

        // Find empty intersections and assign territory
        var emptyTiles = allTiles.Where(tile =>
            !pieceStates.Any(ps => ps.CurrentTile.Id == tile.Id)).ToList();

        var visited = new HashSet<string>();

        foreach (var emptyTile in emptyTiles)
        {
            if (visited.Contains(emptyTile.Id))
            {
                continue;
            }

            // Flood-fill to find connected empty region
            var (region, borderingColors) = FloodFillEmptyRegion(state, emptyTile, pieceStates, visited);

            // If region borders only one color, assign territory to that player
            if (borderingColors.Count == 1)
            {
                var controllingColor = borderingColors.First();
                var controllingPlayer = _game.Players.FirstOrDefault(p => p.Id == controllingColor);
                if (controllingPlayer != null)
                {
                    scores[controllingPlayer.Id] += region.Count;
                }
            }
            // If region borders multiple colors or no colors, it's neutral (no points)
        }

        return scores;
    }

    /// <summary>
    /// Flood-fills an empty region starting from the specified tile, returning the region and bordering colors.
    /// </summary>
    private (HashSet<string> Region, HashSet<string> BorderingColors) FloodFillEmptyRegion(
        GameState state,
        Tile startTile,
        List<PieceState> pieceStates,
        HashSet<string> globalVisited)
    {
        var region = new HashSet<string>();
        var borderingColors = new HashSet<string>();
        var queue = new Queue<Tile>();

        queue.Enqueue(startTile);
        region.Add(startTile.Id);
        globalVisited.Add(startTile.Id);

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();

            // Check all adjacent tiles
            var adjacentTiles = GetAdjacentTiles(currentTile);
            foreach (var adjTile in adjacentTiles)
            {
                var adjPiece = pieceStates.FirstOrDefault(ps => ps.CurrentTile.Id == adjTile.Id);

                if (adjPiece == null)
                {
                    // Empty tile - part of region
                    if (!region.Contains(adjTile.Id))
                    {
                        region.Add(adjTile.Id);
                        globalVisited.Add(adjTile.Id);
                        queue.Enqueue(adjTile);
                    }
                }
                else
                {
                    // Occupied tile - record bordering color
                    var owner = adjPiece.Artifact.Owner;
                    if (owner != null)
                    {
                        borderingColors.Add(owner.Id);
                    }
                }
            }
        }

        return (region, borderingColors);
    }

    /// <summary>
    /// Gets all orthogonally adjacent tiles for the specified tile.
    /// </summary>
    private IEnumerable<Tile> GetAdjacentTiles(Tile tile)
    {
        var relations = _game.Board.TileRelations.Where(r => r.From.Id == tile.Id);
        foreach (var relation in relations)
        {
            yield return relation.To;
        }
    }
}
