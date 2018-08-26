using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class SimpleGameStateBuilder : InitialGameStateBuilder
    {
        public override void Build(Game game)
        {
            AddPieceOnTile("piece-1", "tile-1");
        }
    }
}