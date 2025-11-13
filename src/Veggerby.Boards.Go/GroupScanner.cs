using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go;

/// <summary>
/// Scans a Go board to find connected stone groups and count their liberties.
/// Uses iterative flood-fill to avoid stack overflow on large boards (19x19).
/// </summary>
public sealed class GroupScanner
{
    private readonly Game _game;

    /// <summary>
    /// Creates a new <see cref="GroupScanner"/> for the specified game.
    /// </summary>
    /// <param name="game">The Go game instance.</param>
    public GroupScanner(Game game)
    {
        _game = game ?? throw new System.ArgumentNullException(nameof(game));
    }

    /// <summary>
    /// Scans the group containing the specified stone, returning all connected stones and liberty count.
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <param name="stone">Starting stone to scan from.</param>
    /// <returns>Group information including connected stones and liberty count.</returns>
    public GroupInfo ScanGroup(GameState state, Piece stone)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(stone);

        var pieceStates = state.GetStates<PieceState>().ToList();
        var stoneTile = pieceStates.FirstOrDefault(ps => ps.Artifact.Id == stone.Id)?.CurrentTile;
        if (stoneTile == null)
        {
            return new GroupInfo(new HashSet<Piece>(), 0);
        }

        var stoneOwner = stone.Owner;
        var groupStones = new HashSet<Piece>();
        var visited = new HashSet<string>();
        var liberties = new HashSet<string>();
        var queue = new Queue<Tile>();

        queue.Enqueue(stoneTile);
        visited.Add(stoneTile.Id);

        // Iterative flood-fill to find all connected stones of same color
        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            var pieceOnTile = pieceStates.FirstOrDefault(ps => ps.CurrentTile.Id == currentTile.Id);

            if (pieceOnTile != null && pieceOnTile.Artifact.Owner?.Id == stoneOwner?.Id)
            {
                groupStones.Add((Piece)pieceOnTile.Artifact);

                // Check all adjacent tiles (orthogonal neighbors)
                var adjacentTiles = GetAdjacentTiles(currentTile);
                foreach (var adj in adjacentTiles)
                {
                    if (!visited.Contains(adj.Id))
                    {
                        visited.Add(adj.Id);
                        var adjPiece = pieceStates.FirstOrDefault(ps => ps.CurrentTile.Id == adj.Id);

                        if (adjPiece == null)
                        {
                            // Empty intersection = liberty
                            liberties.Add(adj.Id);
                        }
                        else if (adjPiece.Artifact.Owner?.Id == stoneOwner?.Id)
                        {
                            // Same color stone = part of group
                            queue.Enqueue(adj);
                        }
                        // Different color = not a liberty, not part of group
                    }
                }
            }
        }

        return new GroupInfo(groupStones, liberties.Count);
    }

    /// <summary>
    /// Gets all orthogonally adjacent tiles (N, E, S, W) for the specified tile.
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

/// <summary>
/// Represents a connected group of stones and its liberty count.
/// </summary>
/// <param name="Stones">Set of stones in the group.</param>
/// <param name="Liberties">Number of empty adjacent intersections (liberties).</param>
public sealed record GroupInfo(HashSet<Piece> Stones, int Liberties);
