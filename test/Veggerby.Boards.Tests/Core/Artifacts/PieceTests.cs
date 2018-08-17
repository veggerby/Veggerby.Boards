using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

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
                var actual = new Piece("piece", player, new [] { new DirectionPattern(Direction.North) });

                // assert
                actual.Owner.ShouldBe(player);
                actual.Patterns.ShouldBe(new [] { new DirectionPattern(Direction.North) });
            }
       }
    }
}