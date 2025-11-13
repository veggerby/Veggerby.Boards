using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go.Mutators;

/// <summary>
/// Applies a <see cref="PlaceStoneGameEvent"/>, producing new piece state and resolving captures.
/// Implements capture logic: removes opponent groups with zero liberties, then checks suicide rule.
/// </summary>
public sealed class PlaceStoneStateMutator : IStateMutator<PlaceStoneGameEvent>
{
    /// <summary>
    /// Places the stone if the intersection is empty and the move is legal (not suicide unless capturing).
    /// Captures opponent groups with zero liberties, enforces ko rule, resets pass counter.
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, PlaceStoneGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Reject placement if tile occupied
        if (gameState.GetStates<PieceState>().Any(ps => ps.CurrentTile.Id == @event.Target.Id))
        {
            return gameState;
        }

        // Reject placement if ko rule forbids it
        var extras = gameState.GetExtras<GoStateExtras>() ?? new GoStateExtras(null, 0, 19);
        if (extras.KoTileId != null && extras.KoTileId == @event.Target.Id)
        {
            return gameState;
        }

        // Tentatively place the stone for scanning purposes
        var newPieceState = new PieceState(@event.Stone, @event.Target);
        var tentativeState = gameState.Next([newPieceState]);

        var scanner = new GroupScanner(engine.Game);
        var capturedStones = new List<Piece>();

        // Find and capture opponent groups with zero liberties
        var adjacentTiles = GetAdjacentTiles(engine.Game, @event.Target).ToList();
        var placedStoneOwner = @event.Stone.Owner;
        var processedGroups = new HashSet<string>();

        foreach (var adjTile in adjacentTiles)
        {
            var adjPiece = tentativeState.GetStates<PieceState>()
                .FirstOrDefault(ps => ps.CurrentTile.Id == adjTile.Id);

            if (adjPiece != null && adjPiece.Artifact.Owner?.Id != placedStoneOwner?.Id)
            {
                // Found an opponent stone, check if its group has zero liberties
                var groupKey = adjPiece.Artifact.Id;
                if (!processedGroups.Contains(groupKey))
                {
                    var groupInfo = scanner.ScanGroup(tentativeState, (Piece)adjPiece.Artifact);
                    processedGroups.Add(groupKey);

                    // Mark all stones in group as processed
                    foreach (var stone in groupInfo.Stones)
                    {
                        processedGroups.Add(stone.Id);
                    }

                    if (groupInfo.Liberties == 0)
                    {
                        // Capture the entire group
                        capturedStones.AddRange(groupInfo.Stones);
                    }
                }
            }
        }

        // Create final state with placement + captures in one transition
        var allStateChanges = new List<IArtifactState> { newPieceState };
        allStateChanges.AddRange(capturedStones.Select(stone => new CapturedPieceState(stone)));
        var finalState = gameState.Next(allStateChanges);

        // Check suicide rule: if own group has zero liberties, reject (unless we captured stones)
        var ownGroup = scanner.ScanGroup(finalState, @event.Stone);
        if (ownGroup.Liberties == 0 && capturedStones.Count == 0)
        {
            // Suicide move without capture is illegal
            return gameState;
        }

        // Detect ko: single stone capture that could be immediately recaptured
        string? newKoTile = null;
        if (capturedStones.Count == 1 && ownGroup.Stones.Count == 1)
        {
            // Check if recapture would recreate the position
            var capturedTile = gameState.GetStates<PieceState>()
                .FirstOrDefault(ps => ps.Artifact.Id == capturedStones[0].Id)?.CurrentTile;
            if (capturedTile != null)
            {
                newKoTile = capturedTile.Id;
            }
        }

        // Update extras: reset pass counter, set ko if applicable
        var updatedExtras = extras with
        {
            KoTileId = newKoTile,
            ConsecutivePasses = 0
        };

        return finalState.ReplaceExtras(updatedExtras);
    }

    /// <summary>
    /// Gets all orthogonally adjacent tiles for the specified tile.
    /// </summary>
    private static IEnumerable<Tile> GetAdjacentTiles(Game game, Tile tile)
    {
        var relations = game.Board.TileRelations.Where(r => r.From.Id == tile.Id);
        foreach (var relation in relations)
        {
            yield return relation.To;
        }
    }
}