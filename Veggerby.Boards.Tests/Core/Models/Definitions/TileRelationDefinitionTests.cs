using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Tests.Core.Models.Definitions
{
    public class TileRelationDefinitionTests
    {
        [TestFixture]
        public class Distance
        {
            [Test]
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
                Assert.AreEqual(1, actual);
            }

            [Test]
            [TestCase(1)]
            [TestCase(2)]
            [TestCase(3)]
            [TestCase(4)]
            [TestCase(5)]
            [TestCase(-1)]
            [TestCase(int.MinValue)]
            [TestCase(int.MaxValue)]
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
                Assert.AreEqual(distance, actual);
            }
        }
    }
}
