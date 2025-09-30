using Veggerby.Boards.Chess;

namespace Veggerby.Boards.Tests.Core.Layout;

public class BoardShapeParityTests
{
    [Fact]
    public void GivenChessBoard_WhenBuildingBoardShape_ThenNeighborsMatchRelations()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var engine = progress.Engine;
        var topology = engine.Capabilities?.Topology;
        Assert.NotNull(topology);

        // act & assert: every relation must appear in shape mapping
        foreach (var rel in engine.Game.Board.TileRelations)
        {
            Assert.True(topology.TryGetNeighbor(rel.From, rel.Direction, out var to));
            Assert.Equal(rel.To, to);
        }
    }
}