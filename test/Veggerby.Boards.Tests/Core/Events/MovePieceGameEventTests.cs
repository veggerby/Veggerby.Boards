using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Events;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Events
{
    public class MovePieceGameEventTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new AnyPattern() });
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = new MovePieceGameEvent(piece, from, to);
                
                // assert
                Assert.Equal(piece, actual.Piece);
                Assert.Equal(from, actual.From);
                Assert.Equal(to, actual.To);
            }

            [Theory]
            [InlineData(null, "tile-from", "tile-to", "piece")]
            [InlineData("piece", null, "tile-to", "from")]
            [InlineData("piece", "tile-from", null, "to")]
            [InlineData("piece", null, null, "from")]
            public void Should_throw_null_exception(string pieceId, string fromId, string toId, string expected)
            {
                // arrange
                var piece = pieceId != null ? new Piece(pieceId, null, new [] { new AnyPattern() }) : null;
                var from = fromId != null ? new Tile(fromId) : null;
                var to = toId != null ? new Tile(toId) : null;
                
                // act
                 var actual = Assert.Throws<ArgumentNullException>(() => new MovePieceGameEvent(piece, from, to));
                
                // assert
                 Assert.Equal(expected, actual.ParamName);
            }
        }   
    }
}