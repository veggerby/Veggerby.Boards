using System.Collections.Generic;
using System.Linq;
using Xunit;
using Veggerby.Boards.Core.Contracts.Builders;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Tests.Core.Builders
{
    public class ChessBoardDefinitionBuilderTests
    {
        public class Compile
        {
            [Fact]
            public void Should_return_valid_chess_board_definition()
            {
                // arrange
                var builder = new ChessBoardDefinitionBuilder();

                // act
                var actual = builder.Compile();

                // assert
                Assert.Equal("chess", actual.BoardId);

                var tiles = new List<TileDefinition>();
                for (int x = 1; x <= 8; x++)
                {
                    for (int y = 1; y <= 8; y++)
                    {
                        tiles.Add(actual.GetTile($"tile-{x}-{y}"));
                    }
                }

                Assert.Equal(64, tiles.Count());
                Assert.Equal(64, tiles.Count(x => x != null));

                Assert.NotNull(actual.GetPiece("white-pawn"));
                Assert.NotNull(actual.GetPiece("white-rook"));
                Assert.NotNull(actual.GetPiece("white-knight"));
                Assert.NotNull(actual.GetPiece("white-bishop"));
                Assert.NotNull(actual.GetPiece("white-queen"));
                Assert.NotNull(actual.GetPiece("white-king"));

                Assert.NotNull(actual.GetPiece("black-pawn"));
                Assert.NotNull(actual.GetPiece("black-rook"));
                Assert.NotNull(actual.GetPiece("black-knight"));
                Assert.NotNull(actual.GetPiece("black-bishop"));
                Assert.NotNull(actual.GetPiece("black-queen"));
                Assert.NotNull(actual.GetPiece("black-king"));
            }
        }
    }
}
