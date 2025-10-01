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
    public static Piece GetPiece(this Game game, string id)
    {
        return game
            .GetArtifact<Piece>(id);
    }

    /// <summary>
    /// Retrieves a tile by identifier.
    /// </summary>
    public static Tile GetTile(this Game game, string id)
    {
        var tile = game
            .Board
            .GetTile(id);
        if (tile is null && !string.IsNullOrEmpty(id) && !id.StartsWith("tile-", System.StringComparison.Ordinal))
        {
            tile = game.Board.GetTile($"tile-{id}");
        }
        return tile;
    }

    /// <summary>
    /// Retrieves a player by identifier.
    /// </summary>
    public static Player GetPlayer(this Game game, string id)
    {
        return game
            .Players
            .SingleOrDefault(x => x.Id.Equals(id));
    }

    /// <summary>
    /// Retrieves an artifact by identifier and type.
    /// </summary>
    public static T GetArtifact<T>(this Game game, string id) where T : Artifact
    {
        return game
            .Artifacts
            .OfType<T>()
            .SingleOrDefault(x => x.Id.Equals(id));
    }

    /// <summary>
    /// Retrieves all artifacts of type (optionally filtered by identifiers).
    /// </summary>
    public static IEnumerable<T> GetArtifacts<T>(this Game game, params string[] ids) where T : Artifact
    {
        return [.. game
            .Artifacts
            .OfType<T>()
            .Where(x => !(ids?.Any() ?? false) || ids.Contains(x.Id))];
    }

    /// <summary>
    /// Issues a roll event assigning sequential values (index based) to specified dice.
    /// </summary>
    public static GameProgress RollDice(this GameProgress progress, params string[] ids)
    {
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
        var piece = progress.Game.GetPiece(pieceId);
        var toTile = progress.Game.GetTile(toTileId);
        var state = progress.State.GetState<PieceState>(piece);

        var path = piece.Patterns.Select(pattern =>
        {
            var visitor = new ResolveTilePathPatternVisitor(progress.Engine.Game.Board, state.CurrentTile, toTile);
            pattern.Accept(visitor);
            return visitor.ResultPath;
        }).Where(x => x is not null).OrderBy(x => x.Distance).FirstOrDefault();

        if (path is null)
        {
            return progress;
        }

        var @event = new MovePieceGameEvent(piece, path);
        return progress.HandleEvent(@event);
    }
}