using AutoMapper;


using Veggerby.Boards.States;

namespace Veggerby.Boards.Api.Models.Mappings;

/// <summary>
/// AutoMapper profile for board related projections.
/// </summary>
public class BoardsProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoardsProfile"/> class.
    /// </summary>
    public BoardsProfile()
    {
        CreateMap<GameProgress, GameModel>()
            .ForMember(x => x.Board, o => o.MapFrom(x => x));

        CreateMap<GameProgress, BoardModel>()
            .ConvertUsing<BoardModelTypeConverter>();
    }
}