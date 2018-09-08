using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts
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
                .Where(x => (ids?.Any() ?? false) && ids.Contains(x.Id))
                .ToList();
        }
    }
}