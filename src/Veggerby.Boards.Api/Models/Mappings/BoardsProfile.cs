using AutoMapper;


using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Api.Models.Mappings
{
    public class BoardsProfile : Profile
    {
        public BoardsProfile()
        {
            CreateMap<GameProgress, GameModel>()
                .ForMember(x => x.Board, o => o.ResolveUsing(x => x));

            CreateMap<GameProgress, BoardModel>()
                .ConvertUsing<BoardModelTypeConverter>();
        }
    }
}