using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Convenience helpers for artifact and event operations on <see cref="Game"/> and <see cref="GameProgress"/>.
/// </summary>
public static partial class GameExtensions
{
    /// <summary>
    /// Retrieves a piece by identifier.
    /// </summary>
    public static Piece? GetPiece(this Game game, string id)
    {
        ArgumentNullException.ThrowIfNull(game, nameof(game));
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return game.GetArtifact<Piece>(id);
    }

    /// <summary>
    /// Retrieves a tile by identifier.
    /// </summary>
    public static Tile? GetTile(this Game game, string id)
    {
        ArgumentNullException.ThrowIfNull(game, nameof(game));
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        var tile = game
            .Board
            .GetTile(id);
        if (tile is null && !string.IsNullOrEmpty(id) && !id.StartsWith("tile-", StringComparison.Ordinal))
        {
            tile = game.Board.GetTile($"tile-{id}");
        }

        return tile;
    }

    /// <summary>
    /// Retrieves a player by identifier.
    /// </summary>
    public static Player? GetPlayer(this Game game, string id)
    {
        ArgumentNullException.ThrowIfNull(game, nameof(game));
        ArgumentNullException.ThrowIfNull(id, nameof(id));

        // Use O(1) dictionary lookup via internal method
        return game.GetPlayerById(id);
    }

    /// <summary>
    /// Retrieves an artifact by identifier and type.
    /// </summary>
    public static T? GetArtifact<T>(this Game game, string id) where T : Artifact
    {
        ArgumentNullException.ThrowIfNull(game, nameof(game));
        ArgumentNullException.ThrowIfNull(id, nameof(id));

        // Use O(1) dictionary lookup via internal method, then verify type
        return game.GetArtifactById(id) as T;
    }

    /// <summary>
    /// Retrieves all artifacts of type (optionally filtered by identifiers).
    /// </summary>
    public static IEnumerable<T> GetArtifacts<T>(this Game game, params string[] ids) where T : Artifact
    {
        ArgumentNullException.ThrowIfNull(game);

        if (ids is null || ids.Length == 0)
        {
            return [.. game.Artifacts.OfType<T>()];
        }

        // Use O(1) lookups for each specified id
        var results = new List<T>(ids.Length);
        foreach (var id in ids)
        {
            if (game.GetArtifactById(id) is T artifact)
            {
                results.Add(artifact);
            }
        }

        return results;
    }

    /// <summary>
    /// Issues a roll event assigning sequential values (index based) to specified dice.
    /// </summary>
    public static GameProgress RollDice(this GameProgress progress, params string[] ids)
    {
        ArgumentNullException.ThrowIfNull(progress, nameof(progress));
        var dice = progress.Game.GetArtifacts<Dice>(ids);

        var states = dice
            .Where(d => d.Id != "doubling-dice")
            .Select((x, i) => new DiceState<int>(x, i))
            .ToArray();

        if (states.Length == 0)
        {
            return progress;
        }
        var @event = new RollDiceGameEvent<int>(states);
        return progress.HandleEvent(@event);
    }

    /// <summary>
    /// Attempts to move a piece along the shortest matching pattern path to the target tile.
    /// </summary>
    /// <remarks>
    /// Test parity note: The observer batching parity test (see ObserverBatchingTests.GivenSingleMove_WhenBatchedEnabled_ThenOrderingMatchesUnbatched)
    /// replicates the core of this resolution logic (pattern.Accept + shortest path selection) via a helper to ensure
    /// ordering comparisons remain stable even if movement pattern internals evolve. If you change the semantics here,
    /// update the helper in tests (TestPathHelper.ResolveFirstValidPath) accordingly.
    /// </remarks>
    public static GameProgress Move(this GameProgress progress, string pieceId, string toTileId)
    {
        ArgumentNullException.ThrowIfNull(progress, nameof(progress));
        ArgumentNullException.ThrowIfNull(pieceId, nameof(pieceId));
        ArgumentNullException.ThrowIfNull(toTileId, nameof(toTileId));
        var piece = progress.Game.GetPiece(pieceId);
        var toTile = progress.Game.GetTile(toTileId);
        if (piece is null || toTile is null)
        {
            return progress;
        }
        var state = progress.State.GetState<PieceState>(piece);
        if (state is null || state.CurrentTile is null)
        {
            return progress;
        }

        TilePath? best = null;
        foreach (var pattern in piece.Patterns)
        {
            var visitor = new ResolveTilePathPatternVisitor(progress.Engine.Game.Board, state.CurrentTile, toTile);
            pattern.Accept(visitor);
            if (visitor.ResultPath is not null && (best is null || visitor.ResultPath.Distance < best.Distance))
            {
                best = visitor.ResultPath;
            }
        }

        var path = best;

        if (path is null)
        {
            return progress;
        }

        var @event = new MovePieceGameEvent(piece, path);
        return progress.HandleEvent(@event);
    }
}