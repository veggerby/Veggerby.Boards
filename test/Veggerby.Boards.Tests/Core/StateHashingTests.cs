using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core;

public class StateHashingTests
{
    private static GameProgress Build(bool hashing)
    {
        using var scope = new FeatureFlagScope(hashing: hashing);
        var builder = new TestGameBuilder(useSimpleGamePhase: false);
        return builder.Compile();
    }



    [Fact]
    public void GivenHashingEnabled_WhenBuildingState_ThenHashHasValue()
    {
        // arrange

        // act

        // assert

        var progress = Build(true);

        // act (no action – construction only)

        // assert
        progress.State.Hash.HasValue.Should().BeTrue();
    }

    [Fact]
    public void GivenSameSequence_WhenHashingEnabled_ThenFinalHashesMatch()
    {
        // arrange

        // act

        // assert

        using var scope = new FeatureFlagScope(hashing: true);
        var a = Build(true);
        var b = Build(true);
        var pieceId = "piece-1";
        var from = a.Game.GetTile("tile-1");
        var to = a.Game.GetTile("tile-2");
        var relation = a.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        from.Should().NotBeNull();
        to.Should().NotBeNull();
        relation.Should().NotBeNull();
        var path = new TilePath([relation]);
        var piece = a.Game.GetPiece(pieceId);
        piece.Should().NotBeNull();
        var move = new MovePieceGameEvent(piece!, path);

        // act
        a = a.HandleEvent(move);
        b = b.HandleEvent(move);

        // assert
        a.State.Hash.Should().Be(b.State.Hash);
    }

    [Fact]
    public void GivenDifferentEvents_WhenHashingEnabled_ThenFinalHashesDiffer()
    {
        // arrange

        // act

        // assert

        using var scope = new FeatureFlagScope(hashing: true);
        var a = Build(true);
        var b = Build(true);
        var pieceId = "piece-1";
        var from = a.Game.GetTile("tile-1");
        var to = a.Game.GetTile("tile-2");
        var relation = a.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        from.Should().NotBeNull();
        to.Should().NotBeNull();
        relation.Should().NotBeNull();
        var path = new TilePath([relation]);
        var piece = a.Game.GetPiece(pieceId);
        piece.Should().NotBeNull();
        var move = new MovePieceGameEvent(piece!, path);

        // act
        a = a.HandleEvent(move); // move piece
        // b does not move

        // assert
        a.State.Hash.Should().NotBe(b.State.Hash);
    }

    [Fact]
    public void GivenHashingEnabled_WhenBuildingState_ThenHash128HasValue()
    {
        // arrange

        // act

        // assert

        var progress = Build(true);

        // act (no action – construction only)

        // assert
        progress.State.Hash128.HasValue.Should().BeTrue();
    }

    [Fact]
    public void GivenSameSequence_WhenHashingEnabled_ThenFinalHash128Match()
    {
        // arrange

        // act

        // assert

        using var scope = new FeatureFlagScope(hashing: true);
        var a = Build(true);
        var b = Build(true);
        var pieceId = "piece-1";
        var from = a.Game.GetTile("tile-1");
        var to = a.Game.GetTile("tile-2");
        var relation = a.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        from.Should().NotBeNull();
        to.Should().NotBeNull();
        relation.Should().NotBeNull();
        var path = new TilePath([relation]);
        var piece = a.Game.GetPiece(pieceId);
        piece.Should().NotBeNull();
        var move = new MovePieceGameEvent(piece!, path);

        // act
        a = a.HandleEvent(move);
        b = b.HandleEvent(move);

        // assert
        a.State.Hash128.Should().Be(b.State.Hash128);
    }
}
