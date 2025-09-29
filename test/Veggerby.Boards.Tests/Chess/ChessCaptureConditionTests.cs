using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

public class ChessCaptureConditionTests
{
    private static (GameState State, MovePieceGameEvent Event) BuildEvent(string pieceId, string toTileId)
    {
        var progress = new ChessGameBuilder().Compile();
        var piece = progress.Game.GetPiece(pieceId);
        var fromState = progress.State.GetState<PieceState>(piece);
        var toTile = progress.Game.GetTile(toTileId);
        var path = piece.Patterns.Select(p =>
        {
            var v = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(progress.Game.Board, fromState.CurrentTile, toTile);
            p.Accept(v);
            return v.ResultPath;
        }).FirstOrDefault(p => p is not null);
        path.Should().NotBeNull();
        return (progress.State, new MovePieceGameEvent(piece, path));
    }

    [Fact]
    public void DestinationHasOpponentPiece_ReturnsValid_WhenOpponentPresent()
    {
        // queen to d7 along blocked path (will not matter for condition itself, just occupancy)
        var (state, evt) = BuildEvent("white-queen", ChessIds.Tiles.D7);
        var engine = new ChessGameBuilder().Compile().Engine;
        var condition = new DestinationHasOpponentPieceGameEventCondition();
        state.GetPiecesOnTile(evt.To).Any(p => !p.Owner.Equals(evt.Piece.Owner)).Should().BeTrue();
        var result = condition.Evaluate(engine, state, evt);
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void DestinationHasOpponentPiece_ReturnsIgnore_WhenEmpty()
    {
        var (state, evt) = BuildEvent("white-queen", ChessIds.Tiles.D2); // friendly occupied -> not opponent
        var engine = new ChessGameBuilder().Compile().Engine;
        var condition = new DestinationHasOpponentPieceGameEventCondition();
        var result = condition.Evaluate(engine, state, evt);
        result.Result.Should().Be(ConditionResult.Ignore);
    }
}