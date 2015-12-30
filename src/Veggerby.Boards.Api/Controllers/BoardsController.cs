using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Services;

namespace Veggerby.Boards.Api.Controllers
{
    [Route("api/[controller]")]
    public class BoardsController : Controller
    {
        private readonly IDefinitionService _definitionService;
        
        public BoardsController(IDefinitionService definitionService) 
        {
            _definitionService = definitionService;
        }
        
        // GET: api/boards
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            return new string[] { "chess", "backgammon" };
        }

        // GET api/boards/boardId
        [HttpGet("{boardId}")]
        public async Task<BoardDefinition> Get(string boardId)
        {
            var definition = await _definitionService.GetBoardDefinitionAsync(boardId);
            return definition;
        }
    }
}
