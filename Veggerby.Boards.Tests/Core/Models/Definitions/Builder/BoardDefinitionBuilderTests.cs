﻿using System;
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

        [TestFixture]
        public class AddPiecAddPieceDirectionPatternDefinitioneDefinition
        {
            public class GenericBoardBuilder : BoardDefinitionBuilder
            {
                public override void Build()
                {
                    AddPieceDefinition("white");
                    AddPieceDefinition("black");

                    AddDirectionDefinition("north");
                    AddDirectionDefinition("west");
                    AddDirectionDefinition("east");
                    AddDirectionDefinition("south");
                }

                public new void AddPieceDirectionPatternDefinition(string pieceId, bool isRepeatable, params string[] directions)
                {
                    base.AddPieceDirectionPatternDefinition(pieceId, isRepeatable, directions);
                }
            }

            [Test]
            public void Should_add_single_piece_definition()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddPieceDirectionPatternDefinition("white", true, "north");

                // act
                var actual = builder.Compile();
                var piece1 = actual.GetPiece("white");

                // assert
                Assert.IsNotNull(piece1);
                Assert.AreEqual(1, piece1.DirectionPatterns.Count());
                var dp = piece1.DirectionPatterns.Single();

                Assert.AreEqual("north", dp.Directions.Single().DirectionId);
                Assert.AreEqual(true, dp.IsRepeatable);
            }

            [Test]
            public void Should_add_two_piece_definitions()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddPieceDirectionPatternDefinition("white", true, "north");
                builder.AddPieceDirectionPatternDefinition("white", false, "south");

                // act
                var actual = builder.Compile();
                var piece1 = actual.GetPiece("white");

                // assert
                Assert.IsNotNull(piece1);
                Assert.AreEqual(2, piece1.DirectionPatterns.Count());
                var dp1 = piece1.DirectionPatterns.First();
                var dp2 = piece1.DirectionPatterns.Last();

                Assert.AreEqual("north", dp1.Directions.Single().DirectionId);
                Assert.AreEqual(true, dp1.IsRepeatable);

                Assert.AreEqual("south", dp2.Directions.Single().DirectionId);
                Assert.AreEqual(false, dp2.IsRepeatable);
            }

            [Test]
            public void Should_add_two_piece_definitions_two_different_pieces()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddPieceDirectionPatternDefinition("white", true, "north");
                builder.AddPieceDirectionPatternDefinition("white", false, "south");

                builder.AddPieceDirectionPatternDefinition("black", true, "east");
                builder.AddPieceDirectionPatternDefinition("black", false, "west");

                // act
                var actual = builder.Compile();
                var piece1 = actual.GetPiece("white");
                var piece2 = actual.GetPiece("black");

                // assert
                Assert.IsNotNull(piece1);
                Assert.AreEqual(2, piece1.DirectionPatterns.Count());
                var dp1 = piece1.DirectionPatterns.First();
                var dp2 = piece1.DirectionPatterns.Last();

                Assert.AreEqual("north", dp1.Directions.Single().DirectionId);
                Assert.AreEqual(true, dp1.IsRepeatable);

                Assert.AreEqual("south", dp2.Directions.Single().DirectionId);
                Assert.AreEqual(false, dp2.IsRepeatable);

                Assert.IsNotNull(piece2);
                Assert.AreEqual(2, piece2.DirectionPatterns.Count());
                var dp21 = piece2.DirectionPatterns.First();
                var dp22 = piece2.DirectionPatterns.Last();

                Assert.AreEqual("east", dp21.Directions.Single().DirectionId);
                Assert.AreEqual(true, dp21.IsRepeatable);

                Assert.AreEqual("west", dp22.Directions.Single().DirectionId);
                Assert.AreEqual(false, dp22.IsRepeatable);
            }

            [Test]
            public void Should_add_complex_definitions()
            {
                // arrange
                var builder = new GenericBoardBuilder();
                builder.AddPieceDirectionPatternDefinition("white", false, "north", "north", "west", "west");

                // act
                var actual = builder.Compile();
                var piece1 = actual.GetPiece("white");

                // assert
                Assert.IsNotNull(piece1);
                Assert.AreEqual(1, piece1.DirectionPatterns.Count());
                var dp1 = piece1.DirectionPatterns.First();

                Assert.AreEqual("north", dp1.Directions.First().DirectionId);
                Assert.AreEqual("north", dp1.Directions.Skip(1).First().DirectionId);
                Assert.AreEqual("west", dp1.Directions.Skip(2).First().DirectionId);
                Assert.AreEqual("west", dp1.Directions.Skip(3).First().DirectionId);
            }

        }
    }
}
