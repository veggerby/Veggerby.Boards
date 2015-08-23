using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Tests.Helpers;

namespace Veggerby.Boards.Tests.Core.Models
{
    public class BoardDefinitionTests
    {
        [TestFixture]
        public class GetTile
        {
            [Test]
            public void Should_return_tile_definition_that_exists()
            {
                // arrange
                var boardDefinition = BoardDefinitionHelper.GetBoardDefinition();
                var expected = boardDefinition.Tiles.First();

                // act
                var actual = boardDefinition.GetTile("tile1");

                // assert
                Assert.AreSame(expected, actual);
            }

            [Test]
            public void Should_return_null_for_tile_definition_that_does_not_exist()
            {
                // arrange
                var boardDefinition = BoardDefinitionHelper.GetBoardDefinition();

                // act
                var actual = boardDefinition.GetTile("bogus");

                // assert
                Assert.IsNull(actual);
            }
        }
    }
}
