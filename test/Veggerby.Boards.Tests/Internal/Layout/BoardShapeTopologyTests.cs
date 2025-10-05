using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Tests.Internal.Layout;

public class BoardShapeTopologyTests
{
    [Fact]
    public void GivenOrthogonalAndDiagonalDirections_WhenBuildingShape_ThenTopologyIsOrthogonalAndDiagonal()
    {
        // arrange
        var a1 = new Tile("a1"); var b1 = new Tile("b1"); var a2 = new Tile("a2"); var b2 = new Tile("b2");
        var north = new Direction(Constants.Directions.North); var east = new Direction(Constants.Directions.East); var ne = new Direction(Constants.Directions.NorthEast);
        var r1 = new TileRelation(a1, a2, north); // orth
        var r2 = new TileRelation(a1, b1, east); // orth
        var r3 = new TileRelation(a1, b2, ne); // diag
        var board = new Board("mini", [r1, r2, r3]);

        // act
        var shape = BoardShape.Build(board);

        // assert
        shape.Topology.Should().Be(BoardTopology.OrthogonalAndDiagonal);
    }

    [Fact]
    public void GivenOnlyOrthogonalDirections_WhenBuildingShape_ThenTopologyIsOrthogonal()
    {
        // arrange
        var a1 = new Tile("oa1"); var b1 = new Tile("ob1"); var a2 = new Tile("oa2");
        var north = new Direction(Constants.Directions.North); var east = new Direction(Constants.Directions.East);
        var r1 = new TileRelation(a1, a2, north);
        var r2 = new TileRelation(a1, b1, east);
        var board = new Board("mini-orth", [r1, r2]);

        // act
        var shape = BoardShape.Build(board);

        // assert
        shape.Topology.Should().Be(BoardTopology.Orthogonal);
    }

    [Fact]
    public void GivenArbitraryDirections_WhenBuildingShape_ThenTopologyIsArbitrary()
    {
        // arrange
        var a = new Tile("x1"); var b = new Tile("x2");
        var curve = new Direction("curve");
        var r = new TileRelation(a, b, curve);
        var board = new Board("mini-arb", [r]);

        // act
        var shape = BoardShape.Build(board);

        // assert
        shape.Topology.Should().Be(BoardTopology.Arbitrary);
    }
}