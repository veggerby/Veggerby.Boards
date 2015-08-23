using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Persistence.Static;

namespace Veggerby.Boards.Tests.Core.Persistence.Static
{
    public class StaticBoardDefinitionRepositoryTests
    {
        [TestFixture]
        public class GetAsync
        {
            [Test]
            public async Task Should_return_valid_backgammon_board_definition()
            {
                // arrange
                var repository = new StaticReadBoardDefinitionRepository();

                // act
                var actual = await repository.GetAsync("backgammon");

                // assert
                Assert.AreEqual("backgammon", actual.BoardId);
            }
        }
    }
}
