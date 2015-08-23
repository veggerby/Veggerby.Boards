using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Persistence;
using Veggerby.Boards.Core.Contracts.Services;

namespace Veggerby.Boards.Core.Services
{
    public class DefinitionService : IDefinitionService
    {
        private readonly IBoardDefinitionRepository _boardDefinitionRepository;

        public DefinitionService(IBoardDefinitionRepository boardDefinitionRepository)
        {
            _boardDefinitionRepository = boardDefinitionRepository;
        }

        public Task<BoardDefinition> GetBoardDefinitionAsync(string boardId)
        {
            return _boardDefinitionRepository.GetAsync(boardId);
        }
    }
}
