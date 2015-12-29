using System.Threading.Tasks;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Persistence;
using Veggerby.Boards.Core.Contracts.Services;

namespace Veggerby.Boards.Core.Services
{
    public class DefinitionService : IDefinitionService
    {
        private readonly IReadBoardDefinitionRepository _readBoardDefinitionRepository;

        public DefinitionService(IReadBoardDefinitionRepository readBoardDefinitionRepository)
        {
            _readBoardDefinitionRepository = readBoardDefinitionRepository;
        }

        public Task<BoardDefinition> GetBoardDefinitionAsync(string boardId)
        {
            return _readBoardDefinitionRepository.GetAsync(boardId);
        }
    }
}
