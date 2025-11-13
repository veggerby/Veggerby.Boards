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
        var existingPieces = gameState.GetPiecesOnTile(@event.Target);
        if (existingPieces.Any())
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
        var capturedTileIds = new List<string>();

        // Find and capture opponent groups with zero liberties
        var adjacentTiles = GetAdjacentTiles(engine.Game, @event.Target).ToList();
        var placedStoneOwner = @event.Stone.Owner;
        var processedGroups = new HashSet<string>();

        foreach (var adjTile in adjacentTiles)
        {
            var adjPieces = tentativeState.GetPiecesOnTile(adjTile);
            var adjPiece = adjPieces.FirstOrDefault();

            if (adjPiece != null && adjPiece.Owner?.Id != placedStoneOwner?.Id)
            {
                // Found an opponent stone, check if its group has zero liberties
                var groupKey = adjPiece.Id;
                if (!processedGroups.Contains(groupKey))
                {
                    var groupInfo = scanner.ScanGroup(tentativeState, adjPiece);
                    processedGroups.Add(groupKey);

                    // Mark all stones in group as processed
                    foreach (var stone in groupInfo.Stones)
                    {
                        processedGroups.Add(stone.Id);
                    }

                    if (groupInfo.Liberties == 0)
                    {
                        // Capture the entire group - record tiles before capturing
                        foreach (var stone in groupInfo.Stones)
                        {
                            capturedStones.Add(stone);
                            var stoneState = tentativeState.GetState<PieceState>(stone);
                            if (stoneState != null)
                            {
                                capturedTileIds.Add(stoneState.CurrentTile.Id);
                            }
                        }
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

        // Detect ko: single stone capture by single stone that could be immediately recaptured
        string? newKoTile = null;
        if (capturedStones.Count == 1 && ownGroup.Stones.Count == 1 && capturedTileIds.Count == 1)
        {
            // Ko is only when single stone captures single stone and recapture would restore position
            newKoTile = capturedTileIds[0];
        }

        // Update extras: reset pass counter, set ko if applicable, clear ko otherwise
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