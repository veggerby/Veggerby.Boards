using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Core.Contracts.Services
{
    public interface IDefinitionService
    {
        Task<BoardDefinition> GetBoardDefinitionAsync(string boardId);
    }
}
