using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class TestInitialGameStateBuilder : InitialGameStateBuilder
    {
        public override void Build(Game game)
        {
            AddPieceOnTile("piece-1", "tile-1");
            AddPieceOnTile("piece-2", "tile-2");
            AddPieceOnTile("piece-n", "tile-1");
        }
    }
}