using System;
using System.Linq;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Veggerby.Boards;
using Veggerby.Boards.Api.Models;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Api.Controllers;

/// <summary>
/// HTTP API for accessing game projections (demo purposes only).
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class GamesController : ControllerBase
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GamesController"/> class.
    /// </summary>
    public GamesController(IMapper mapper)
    {
        if (mapper is null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        _mapper = mapper;
    }

    // GET api/games/{id}
    /// <summary>
    /// Gets a demo game instance (GUID ending with 1 selects Backgammon otherwise Chess).
    /// </summary>
    /// <param name="id">Game identifier (pattern-based demo selector).</param>
    /// <returns>Serialized game projection.</returns>
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