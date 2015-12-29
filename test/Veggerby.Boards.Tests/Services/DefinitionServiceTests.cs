using System.Threading.Tasks;
using Xunit;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Persistence;
using Veggerby.Boards.Core.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Veggerby.Boards.Tests.Core.Services
{
    public class DefinitionServiceTests
    {
        private class MockReadBoardDefinitionRepository : IReadBoardDefinitionRepository
        {
            private readonly BoardDefinition _boardDefinition;
            
            public MockReadBoardDefinitionRepository(BoardDefinition boardDefinition) 
            {
                _boardDefinition = boardDefinition;    
            }
            
            public Task<BoardDefinition> GetAsync(string id)
            {
                return Task.FromResult(_boardDefinition);
            }

            public Task<IEnumerable<BoardDefinition>> ListAsync()
            {
                return Task.FromResult(new [] { _boardDefinition }.AsEnumerable());
            }

            public Task<IEnumerable<BoardDefinition>> ListAsync(Expression<Predicate<BoardDefinition>> query)
            {
                return Task.FromResult(Enumerable.Empty<BoardDefinition>());
            }
        }

        public class GetAsync
        {
            [Fact]
            public async Task Should_return_definition()
            {
                // arrange
                var expected = new BoardDefinition("board", null, null, null);
                var repository = new MockReadBoardDefinitionRepository(expected);
                var service = new DefinitionService(repository);

                // act
                var actual = await service.GetBoardDefinitionAsync("board");

                // assert
                Assert.NotNull(actual);
                Assert.Equal(expected, actual);
            }

            [Fact]
            public async Task Should_return_null_for_nonexisting_definition()
            {
                // arrange
                var repository = new MockReadBoardDefinitionRepository(null);
                var service = new DefinitionService(repository);
                
                // act
                var actual = await service.GetBoardDefinitionAsync("board");

                // assert
                Assert.Null(actual);
            }
        }
    }
}
