using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

using static Veggerby.Boards.Chess.ChessIds.Pieces;

namespace Veggerby.Boards.Tests.Chess;

public class ChessCaptureConditionTests
{
    private static (GameState State, MovePieceGameEvent Event) BuildEvent(string pieceId, string toTileId)
    {
        var progress = new ChessGameBuilder().Compile();
        var piece = progress.Game.GetPiece(pieceId);
        piece.Should().NotBeNull();
        var fromState = progress.State.GetState<PieceState>(piece!);
        fromState.Should().NotBeNull();
        var toTile = progress.Game.GetTile(toTileId);
        toTile.Should().NotBeNull();
        var path = piece.Patterns.Select(p =>
        {
            var v = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(progress.Game.Board, fromState!.CurrentTile, toTile!);
            p.Accept(v);
            return v.ResultPath;
        }).FirstOrDefault(p => p is not null);
        path.Should().NotBeNull();
        return (progress.State, new MovePieceGameEvent(piece!, path!));
    }

    [Fact]
    public void DestinationHasOpponentPiece_ReturnsValid_WhenOpponentPresent()
    {
        // arrange

        // act

        // assert

        var (state, evt) = BuildEvent(WhiteQueen, ChessIds.Tiles.D7);
        var engine = new ChessGameBuilder().Compile().Engine;
        var condition = new DestinationHasOpponentPieceGameEventCondition();
        state.GetPiecesOnTile(evt.To!).Any(p => !p.Owner.Equals(evt.Piece.Owner)).Should().BeTrue();

        // act
        var result = condition.Evaluate(engine, state, evt);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void DestinationHasOpponentPiece_ReturnsIgnore_WhenEmpty()
    {
        // arrange

        // act

        // assert

        var (state, evt) = BuildEvent(WhiteQueen, ChessIds.Tiles.D2); // friendly occupied -> not opponent
        var engine = new ChessGameBuilder().Compile().Engine;
        var condition = new DestinationHasOpponentPieceGameEventCondition();

        // act
        var result = condition.Evaluate(engine, state, evt);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
    }
}
