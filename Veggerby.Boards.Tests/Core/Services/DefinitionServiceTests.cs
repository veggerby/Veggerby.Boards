using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Models.Definitions;
using Veggerby.Boards.Core.Contracts.Persistence;
using Veggerby.Boards.Core.Services;

namespace Veggerby.Boards.Tests.Core.Services
{
    public class DefinitionServiceTests
    {
        [TestFixture]
        public class GetAsync
        {
            [Test]
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
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);

                repository.Verify(x => x.GetAsync("board"), Times.Once);
            }

            [Test]
            public async Task Should_return_null_for_nonexisting_definition()
            {
                // arrange
                var repository = new Mock<IReadBoardDefinitionRepository>();

                var service = new DefinitionService(repository.Object);

                // act
                var actual = await service.GetBoardDefinitionAsync("board");

                // assert
                Assert.IsNull(actual);

                repository.Verify(x => x.GetAsync("board"), Times.Once);
            }
        }
    }
}
