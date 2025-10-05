using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Chess;

public class ChessStateExtrasTests
{
    [Fact]
    public void Should_initialize_chess_state_extras_defaults()
    {
        // arrange

        // act
        var progress = new ChessGameBuilder().Compile();
        var extras = progress.State.GetExtras<ChessStateExtras>();

        // assert
        extras.Should().NotBeNull();
        extras.WhiteCanCastleKingSide.Should().BeTrue();
        extras.WhiteCanCastleQueenSide.Should().BeTrue();
        extras.BlackCanCastleKingSide.Should().BeTrue();
        extras.BlackCanCastleQueenSide.Should().BeTrue();
        extras.EnPassantTargetTileId.Should().BeNull();
        extras.HalfmoveClock.Should().Be(0);
        extras.FullmoveNumber.Should().Be(1);
        extras.MovedPieceIds.Should().BeEmpty();
    }
}