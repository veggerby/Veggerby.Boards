using AutoMapper;
using Veggerby.Boards.Core;

namespace Veggerby.Boards.Api.Models.Mappings
{
    public class BoardsProfile : Profile
    {
        public BoardsProfile()
        {
            CreateMap<GameEngine, GameModel>()
                .ForMember(x => x.Board, o => o.ResolveUsing(x => x));

            CreateMap<GameEngine, BoardModel>()
                .ConvertUsing<BoardModelTypeConverter>();
        }
    }
}