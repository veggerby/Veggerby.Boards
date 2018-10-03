using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Veggerby.Boards.Api.Models;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Core;

namespace Veggerby.Boards.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IMapper _mapper;

        public GamesController(IMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            _mapper = mapper;
        }

        // GET api/games/{id}
        [HttpGet("{id}", Name = "GetGame")]
        public IActionResult Get(Guid id)
        {
            var builder = id.ToString().EndsWith("1")
                ? new BackgammonGameEngineBuilder() as GameEngineBuilder
                : new ChessGameEngineBuilder();
            var engine = builder.Compile();
            var result = _mapper.Map<GameModel>(engine);
            return Ok(result);
        }
    }
}
