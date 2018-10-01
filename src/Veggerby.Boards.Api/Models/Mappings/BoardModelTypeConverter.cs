using System.Linq;
using AutoMapper;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Api.Models.Mappings
{
    public class BoardModelTypeConverter : ITypeConverter<GameEngine, BoardModel>
    {
        public PieceModel Convert(GameEngine source, Piece piece, ResolutionContext context)
        {
            return new PieceModel
            {
                PieceId = piece.Id,
                OwnerId = piece.Owner?.Id
            };
        }

        public TileModel Convert(GameEngine source, Tile tile, ResolutionContext context)
        {
            var states = source.GameState.GetStates<PieceState>().Where(x => tile.Equals(x.CurrentTile));
            var result = new TileModel
            {
                TileId = tile.Id,
                Pieces = states.Select(x => Convert(source, x.Artifact, context)).ToArray()
            };

            return result;
        }

        public BoardModel Convert(GameEngine source, BoardModel destination, ResolutionContext context)
        {
            destination = destination ?? new BoardModel();
            destination.Id = source.Game.Board.Id;
            destination.Tiles = source.Game.Board.Tiles.Select(x => Convert(source, x, context)).ToArray();

            return destination;
        }
    }
}