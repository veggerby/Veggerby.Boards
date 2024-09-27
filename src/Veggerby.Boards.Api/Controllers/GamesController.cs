using System;
using System.Linq;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Veggerby.Boards.Api.Models;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

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
            GameProgress progress;

            if (id.ToString().EndsWith("1"))
            {
                var builder = new BackgammonGameBuilder();
                progress = builder.Compile();
                var dice1 = progress.Game.GetArtifact<Dice>("dice-1");
                var dice2 = progress.Game.GetArtifact<Dice>("dice-2");

                var piece = progress.Game.GetPiece("white-1");
                var from = progress.Game.GetTile("point-1");
                var to = progress.Game.GetTile("point-5");

                progress = progress.HandleEvent(
                    new RollDiceGameEvent<int>(
                        new DiceState<int>(dice1, 3), // white player active
                        new DiceState<int>(dice2, 1)
                    )
                );

                var visitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
                piece.Patterns.Single().Accept(visitor);
                var @event = new MovePieceGameEvent(piece, visitor.ResultPath);

                progress = progress.HandleEvent(@event);
            }
            else
            {
                var builder = new ChessGameBuilder();
                progress = builder.Compile();
            }

            var result = _mapper.Map<GameModel>(progress);
            return Ok(result);
        }
    }
}
