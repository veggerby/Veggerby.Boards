using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class TileState : State<Tile>
    {
        public IEnumerable<Piece> CurrentPieces { get; }

        public TileState(Tile tile, IEnumerable<Piece> pieces) : base(tile)
        {
            CurrentPieces = (pieces ?? Enumerable.Empty<Piece>()).ToList();
        }
    }
}