using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Veggerby.Boards.Core.Contracts.Builders;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Models.Definitions.Builder;
using Veggerby.Boards.Core.Contracts.Persistence;

namespace Veggerby.Boards.Core.Persistence.Static
{
    public class StaticReadBoardDefinitionRepository : IReadBoardDefinitionRepository
    {
        private readonly IDictionary<string, BoardDefinition> _definitions;

        public StaticReadBoardDefinitionRepository()
        {
            _definitions = new Dictionary<string, BoardDefinition>();
            AddBoardDefinition(new BackgammonBoardDefinitionBuilder());
            AddBoardDefinition(new ChessBoardDefinitionBuilder());
        }

        private void AddBoardDefinition(BoardDefinitionBuilder builder)
        {
            var definition = builder.Compile();
            _definitions.Add(definition.BoardId, definition);
        }

        public Task<BoardDefinition> GetAsync(string id)
        {
            return Task.FromResult(_definitions.ContainsKey(id) ? _definitions[id] : null);
        }

        public Task<IEnumerable<BoardDefinition>> ListAsync()
        {
            return Task.FromResult(_definitions.Values.AsEnumerable());
        }

        public Task<IEnumerable<BoardDefinition>> ListAsync(Expression<Predicate<BoardDefinition>> query)
        {
            return Task.FromResult(_definitions.Values.Where(x => query.Compile()(x)).ToList().AsEnumerable());
        }
    }
}
