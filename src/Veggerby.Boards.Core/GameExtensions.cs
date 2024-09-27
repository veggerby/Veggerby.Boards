using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Veggerby.Boards.Tests")]

namespace Veggerby.Boards.Core;

public static class GameExtensions
{
    public static Piece GetPiece(this Game game, string id)
    {
        return game
            .GetArtifact<Piece>(id);
    }

    public static Tile GetTile(this Game game, string id)
    {
        return game
            .Board
            .GetTile(id);
    }

    public static Player GetPlayer(this Game game, string id)
    {
        return game
            .Players
            .SingleOrDefault(x => x.Id.Equals(id));
    }

    public static T GetArtifact<T>(this Game game, string id) where T : Artifact
    {
        return game
            .Artifacts
            .OfType<T>()
            .SingleOrDefault(x => x.Id.Equals(id));
    }

    public static IEnumerable<T> GetArtifacts<T>(this Game game, params string[] ids) where T : Artifact
    {
        return game
            .Artifacts
            .OfType<T>()
            .Where(x => !(ids?.Any() ?? false) || ids.Contains(x.Id))
            .ToList();
    }

    public static GameProgress RollDice(this GameProgress progress, params string[] ids)
    {
        var dice = progress.Game.GetArtifacts<Dice>(ids);
        var states = dice.Select((x, i) => new DiceState<int>(x, i)).ToArray();
        var @event = new RollDiceGameEvent<int>(states);
        return progress.HandleEvent(@event);
    }

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