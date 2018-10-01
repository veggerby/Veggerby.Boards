using AutoMapper;
using Veggerby.Boards.Core;

namespace Veggerby.Boards.Api.Models.Mappings
{
    public class BoardsProfile : Profile
    {
        public BoardsProfile()
        {
            CreateMap<GameEngine, BoardModel>()
                .ConvertUsing<BoardModelTypeConverter>();
        }
    }
}