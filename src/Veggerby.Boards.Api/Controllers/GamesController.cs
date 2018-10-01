using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Veggerby.Boards.Api.Models;
using Veggerby.Boards.Backgammon;
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
            var builder = new BackgammonGameEngineBuilder();
            var engine = builder.Compile();
            engine.RollDice("dice-1", "dice-2");
            engine.Move("black-1", "point-9");
            engine.Move("black-2", "point-9");
            engine.Move("black-3", "point-6");
            var result = _mapper.Map<BoardModel>(engine);
            return Ok(result);
        }
    }
}
