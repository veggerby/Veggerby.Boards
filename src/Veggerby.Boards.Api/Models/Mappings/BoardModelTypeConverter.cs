using System.Linq;

using AutoMapper;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Api.Models.Mappings;

/// <summary>
/// Custom type converter for constructing a <see cref="BoardModel"/> from a <see cref="GameProgress"/>.
/// </summary>
public class BoardModelTypeConverter : ITypeConverter<GameProgress, BoardModel>
{
    /// <summary>
    /// Creates a piece projection.
    /// </summary>
    public PieceModel Convert(GameProgress source, Piece piece, ResolutionContext context)
    {
        return new PieceModel
        {
            PieceId = piece.Id,
            OwnerId = piece.Owner?.Id ?? string.Empty,
        };
    }

    /// <summary>
    /// Creates a tile projection including piece projections.
    /// </summary>
    public TileModel Convert(GameProgress source, Tile tile, ResolutionContext context)
    {
        var states = source.State.GetStates<PieceState>().Where(x => tile.Equals(x.CurrentTile));
        var result = new TileModel
        {
            TileId = tile.Id,
            Pieces = [.. states.Select(x => Convert(source, x.Artifact, context))]
        };

        return result;
    }

    /// <summary>
    /// Populates a board model from game progress.
    /// </summary>
    public BoardModel Convert(GameProgress source, BoardModel destination, ResolutionContext context)
    {
        destination = destination ?? new BoardModel();
        destination.Id = source.Game.Board.Id;
        destination.Tiles = [.. source.Game.Board.Tiles.Select(x => Convert(source, x, context))];

        return destination;
    }
}