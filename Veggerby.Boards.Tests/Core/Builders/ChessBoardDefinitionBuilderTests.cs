using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Builders;

namespace Veggerby.Boards.Tests.Core.Builders
{
    public class ChessBoardDefinitionBuilderTests
    {
        [TestFixture]
        public class Compile
        {
            [Test]
            public void Should_return_valid_chess_board_definition()
            {
                // arrange
                var builder = new ChessBoardDefinitionBuilder();

                // act
                var actual = builder.Compile();

                // assert
                Assert.AreEqual("chess", actual.BoardId);
            }
        }
    }
}
