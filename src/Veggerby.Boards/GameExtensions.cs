using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Veggerby.Boards.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Veggerby.Boards.Benchmarks")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Veggerby.Boards.PropertyTests")]

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

        // Special handling: the Backgammon doubling cube ("doubling-dice") should not have its value overwritten
        // by the deterministic index-based helper. Its lifecycle is governed by DoublingDiceStateMutator which
        // doubles the current value and assigns ownership. If the helper is invoked with only the doubling cube
        // (e.g., tests calling RollDice("doubling-dice")) we preserve the existing state so that an ignored condition
        // results in a silent no-op (unchanged value & owner). We only construct transient DiceState<int> instances
        // for standard dice (non doubling cube) here.
        var states = dice
            .Where(d => d.Id != "doubling-dice")
            .Select((x, i) => new DiceState<int>(x, i))
            .ToArray();

        // If after filtering there are no states to roll (e.g., only doubling-dice requested) simply return original progress.
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
    /// update the helper in tests (`TestPathHelper.ResolveFirstValidPath`) accordingly.
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

    /// <summary>
    /// Explicit chess castling helper (king side or queen side) removing reliance on synthetic path inside generic Move.
    /// </summary>
    /// <param name="progress">Game progress.</param>
    /// <param name="color">"white" or "black".</param>
    /// <param name="kingSide">True for king-side, false for queen-side.</param>
    public static GameProgress Castle(this GameProgress progress, string color, bool kingSide)
    {
        var kingId = color + "-king";
        var game = progress.Game;
        var king = game.GetPiece(kingId);
        var kingState = progress.State.GetState<PieceState>(king);
        var start = kingState.CurrentTile.Id;
        var expectedStart = color == "white" ? "tile-e1" : "tile-e8";
        if (start != expectedStart) { return progress; }
        var destination = color == "white"
            ? (kingSide ? game.GetTile("tile-g1") : game.GetTile("tile-c1"))
            : (kingSide ? game.GetTile("tile-g8") : game.GetTile("tile-c8"));

        // Build horizontal path relations (two steps toward rook)
        char fromFile = start[5];
        char toFile = destination.Id[5];
        int step = fromFile < toFile ? 1 : -1;
        var rank = start[6];
        var relations = new List<TileRelation>();
        var dirId = step == 1 ? "east" : "west";
        var direction = new Direction(dirId);
        var current = kingState.CurrentTile;
        for (char f = (char)(fromFile + step); ; f = (char)(f + step))
        {
            var nextTile = game.GetTile($"tile-{f}{rank}");
            relations.Add(new TileRelation(current, nextTile, direction));
            current = nextTile;
            if (f == toFile) { break; }
        }
        var path = new TilePath(relations);
        var @event = new MovePieceGameEvent(king, path);
        return progress.HandleEvent(@event);
    }
}