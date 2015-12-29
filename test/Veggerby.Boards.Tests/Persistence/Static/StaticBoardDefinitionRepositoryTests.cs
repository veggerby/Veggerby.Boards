using System.Threading.Tasks;
using Xunit;
using Veggerby.Boards.Core.Persistence.Static;

namespace Veggerby.Boards.Tests.Core.Persistence.Static
{
    public class StaticBoardDefinitionRepositoryTests
    {
        public class GetAsync
        {
            [Fact]
            public async Task Should_return_valid_backgammon_board_definition()
            {
                // arrange
                var repository = new StaticReadBoardDefinitionRepository();

                // act
                var actual = await repository.GetAsync("backgammon");

                // assert
                Assert.Equal("backgammon", actual.BoardId);
            }
        }
    }
}
