using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class PieceTests
    {
        public class Constructor
        {
            [Fact]
            public void Should_initialize_properties()
            {
                // arrange
                var player = new Player("player");

                // act
                var actual = new Piece("piece", player, [new DirectionPattern(Direction.North)]);

                // assert
                actual.Owner.Should().Be(player);
                actual.Patterns.Should().Equal([new DirectionPattern(Direction.North)]);
            }

            [Fact]
            public void Should_initialize_properties_with_null_pattern_list()
            {
                // arrange
                var player = new Player("player");

                // act
                var actual = new Piece("piece", player, null);

                // assert
                actual.Owner.Should().Be(player);
                actual.Patterns.Should().BeEmpty();
            }
       }
    }
}