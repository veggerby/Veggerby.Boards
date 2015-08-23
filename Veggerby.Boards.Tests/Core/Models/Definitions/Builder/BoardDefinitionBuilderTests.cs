using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Veggerby.Boards.Core.Contracts.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Definitions.Builder
{
    public class BoardDefinitionBuilderTests
    {
        [TestFixture]
        public class AddTileDefinition
        {
            public class GenericBoardBuilder : BoardDefinitionBuilder
            {
                public override void Build()
                {
                }

                public new void AddTileDefinition(string tileId)
                {
                    base.AddTileDefinition(tileId);
                }
            }

            [Test]
            public void Should_add_single_tile_definition()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddTileDefinition("tile1");

                // act
                var actual = builder.Compile();

                // assert
                Assert.AreEqual("tile1", actual.GetTile("tile1").TileId);
            }

            [Test]
            public void Should_add_two_tile_definition()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddTileDefinition("tile1");
                builder.AddTileDefinition("tile2");

                // act
                var actual = builder.Compile();

                // assert
                Assert.AreEqual("tile1", actual.GetTile("tile1").TileId);
                Assert.AreEqual("tile2", actual.GetTile("tile2").TileId);
            }
        }

        [TestFixture]
        public class AddTileRelationDefinition
        {
            public class GenericBoardBuilder : BoardDefinitionBuilder
            {
                public override void Build()
                {
                    AddTileDefinition("tile1");
                    AddTileDefinition("tile2");
                    AddTileDefinition("tile3");

                    AddDirectionDefinition("noth");
                    AddDirectionDefinition("west");
                    AddDirectionDefinition("east");
                    AddDirectionDefinition("south");
                }

                public new void AddTileRelationDefinition(string sourceTileId, string destinationTileId, string directionId, int distance = 1)
                {
                    base.AddTileRelationDefinition(sourceTileId, destinationTileId, directionId, distance);
                }
            }

            [Test]
            public void Should_add_single_tile_relation()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddTileRelationDefinition("tile1", "tile2", "west");

                // act
                var actual = builder.Compile();
                var tile1 = actual.GetTile("tile1");
                var tile2 = actual.GetTile("tile2");
                var relation1 = actual.GetTileRelation("tile1", "tile2");
                var relation2 = actual.GetTileRelation("tile2", "tile1");

                // assert
                Assert.AreSame(tile1, relation1.SourceTile);
                Assert.AreSame(tile2, relation1.DestinationTile);
                Assert.AreEqual("west", relation1.Direction.DirectionId);
                Assert.AreEqual(1, relation1.Distance);

                Assert.IsNull(relation2);
            }

            [Test]
            public void Should_add_two_tile_relations()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddTileRelationDefinition("tile1", "tile2", "west");
                builder.AddTileRelationDefinition("tile2", "tile1", "east");

                // act
                var actual = builder.Compile();
                var tile1 = actual.GetTile("tile1");
                var tile2 = actual.GetTile("tile2");
                var relation1 = actual.GetTileRelation("tile1", "tile2");
                var relation2 = actual.GetTileRelation("tile2", "tile1");

                // assert
                Assert.AreSame(tile1, relation1.SourceTile);
                Assert.AreSame(tile2, relation1.DestinationTile);
                Assert.AreEqual("west", relation1.Direction.DirectionId);
                Assert.AreEqual(1, relation1.Distance);

                Assert.AreSame(tile2, relation2.SourceTile);
                Assert.AreSame(tile1, relation2.DestinationTile);
                Assert.AreEqual("east", relation2.Direction.DirectionId);
                Assert.AreEqual(1, relation2.Distance);
            }
        }


        [TestFixture]
        public class AddPieceDefinition
        {
            public class GenericBoardBuilder : BoardDefinitionBuilder
            {
                public override void Build()
                {
                }

                public new void AddPieceDefinition(string pieceId)
                {
                    base.AddPieceDefinition(pieceId);
                }
            }

            [Test]
            public void Should_add_single_piece_definition()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddPieceDefinition("piece1");

                // act
                var actual = builder.Compile();
                var piece1 = actual.GetPiece("piece1");
                var piece2 = actual.GetPiece("piece2");

                // assert
                Assert.AreEqual("piece1", piece1.PieceId);
                Assert.IsNull(piece2);
            }

            [Test]
            public void Should_add_two_piece_definitions()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddPieceDefinition("piece1");
                builder.AddPieceDefinition("piece2");

                // act
                var actual = builder.Compile();
                var piece1 = actual.GetPiece("piece1");
                var piece2 = actual.GetPiece("piece2");

                // assert
                Assert.AreEqual("piece1", piece1.PieceId);
                Assert.AreEqual("piece2", piece2.PieceId);
            }
        }
    }
}
