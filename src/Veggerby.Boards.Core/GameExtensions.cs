using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
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
            var tile = progress.Game.GetTile(toTileId);
            var state = progress.GameState.GetState<PieceState>(piece);
            var @event = new MovePieceGameEvent(piece, state.CurrentTile, tile);
            return progress.HandleEvent(@event);
        }
    }
}