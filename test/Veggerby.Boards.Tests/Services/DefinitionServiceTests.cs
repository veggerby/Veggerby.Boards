using System.Threading.Tasks;
using Moq;
using Xunit;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Persistence;
using Veggerby.Boards.Core.Services;

namespace Veggerby.Boards.Tests.Core.Services
{
    public class DefinitionServiceTests
    {
        public class GetAsync
        {
            [Fact]
            public async Task Should_return_definition()
            {
                // arrange
                var expected = new BoardDefinition("board", null, null, null);

                var repository = new Mock<IReadBoardDefinitionRepository>();
                repository.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var service = new DefinitionService(repository.Object);

                // act
                var actual = await service.GetBoardDefinitionAsync("board"); 

                // assert
                Assert.NotNull(actual);
                Assert.Equal(expected, actual);

                repository.Verify(x => x.GetAsync("board"), Times.Once);
            }

            [Fact]
            public async Task Should_return_null_for_nonexisting_definition()
            {
                // arrange
                var repository = new Mock<IReadBoardDefinitionRepository>();

                var service = new DefinitionService(repository.Object);

                // act
                var actual = await service.GetBoardDefinitionAsync("board");

                // assert
                Assert.Null(actual);

                repository.Verify(x => x.GetAsync("board"), Times.Once);
            }
        }
    }
}
