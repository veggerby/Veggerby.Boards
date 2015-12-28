using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Tests.Core.Models.Definitions
{
    public class TileRelationDefinitionTests
    {
        public class Distance
        {
            [Fact]
            public void Should_return_defaul_distance()
            {
                // arrange
                var tile1 = new TileDefinition("tile1");
                var tile2 = new TileDefinition("tile2");
                var direction = new DirectionDefinition("west");
                var tileRelation = new TileRelationDefinition(tile1, tile2, direction);

                // act
                var actual = tileRelation.Distance;

                // assert
                Assert.Equal(1, actual);
            }

            [Theory]
            [InlineData(1)]
            [InlineData(2)]
            [InlineData(3)]
            [InlineData(4)]
            [InlineData(5)]
            [InlineData(-1)]
            [InlineData(int.MinValue)]
            [InlineData(int.MaxValue)]
            public void Should_return_defaul_distance(int distance)
            {
                // arrange
                var tile1 = new TileDefinition("tile1");
                var tile2 = new TileDefinition("tile2");
                var direction = new DirectionDefinition("west");
                var tileRelation = new TileRelationDefinition(tile1, tile2, direction, distance);

                // act
                var actual = tileRelation.Distance;

                // assert
                Assert.Equal(distance, actual);
            }
        }
    }
}
