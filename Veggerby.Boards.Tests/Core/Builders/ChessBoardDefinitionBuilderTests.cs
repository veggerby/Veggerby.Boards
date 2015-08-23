using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Builders;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

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

                var tiles = new List<TileDefinition>();
                for (int x = 1; x <= 8; x++)
                {
                    for (int y = 1; y <= 8; y++)
                    {
                        tiles.Add(actual.GetTile($"tile-{x}-{y}"));
                    }
                }

                Assert.AreEqual(64, tiles.Count());
                Assert.AreEqual(64, tiles.Count(x => x != null));

                Assert.IsNotNull(actual.GetPiece("white-pawn"));
                Assert.IsNotNull(actual.GetPiece("white-rook"));
                Assert.IsNotNull(actual.GetPiece("white-knight"));
                Assert.IsNotNull(actual.GetPiece("white-bishop"));
                Assert.IsNotNull(actual.GetPiece("white-queen"));
                Assert.IsNotNull(actual.GetPiece("white-king"));

                Assert.IsNotNull(actual.GetPiece("black-pawn"));
                Assert.IsNotNull(actual.GetPiece("black-rook"));
                Assert.IsNotNull(actual.GetPiece("black-knight"));
                Assert.IsNotNull(actual.GetPiece("black-bishop"));
                Assert.IsNotNull(actual.GetPiece("black-queen"));
                Assert.IsNotNull(actual.GetPiece("black-king"));
            }
        }
    }
}
