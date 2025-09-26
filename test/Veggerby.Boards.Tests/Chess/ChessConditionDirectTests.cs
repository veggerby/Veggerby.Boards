using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

public class ChessConditionDirectTests
{
    private static (GameState State, MovePieceGameEvent Event) BuildEvent(string pieceId, string toTileId)
    {
        var progress = new ChessGameBuilder().Compile();
        var piece = progress.Game.GetPiece(pieceId);
        var fromState = progress.State.GetState<PieceState>(piece);
        var toTile = progress.Game.GetTile(toTileId);

        // Re-use pattern resolution like production Move helper
        var path = piece.Patterns
            .Select(p =>
            {
                var v = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(progress.Game.Board, fromState.CurrentTile, toTile);
                p.Accept(v);
                return v.ResultPath;
            })
            .FirstOrDefault(p => p is not null);

        path.Should().NotBeNull($"expected a resolvable path for test from {fromState.CurrentTile.Id} to {toTileId}");

        var evt = new MovePieceGameEvent(piece, path);
        return (progress.State, evt);
    }

    [Fact]
    public void PathNotObstructed_Should_Ignore_Queen_E1_E4()
    {
        // arrange
        var (state, evt) = BuildEvent("white-queen", "tile-e4"); // passes over e2 (occupied by pawn)
        var engine = new ChessGameBuilder().Compile().Engine; // fresh engine for condition context
        var condition = new PathNotObstructedGameEventCondition();

        // act
        var result = condition.Evaluate(engine, state, evt);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore, $"expected Ignore when path blocked: {result.Reason}");
    }

    [Fact]
    public void DestinationNotOwnPiece_Should_Ignore_Queen_E1_E2()
    {
        // arrange
        var (state, evt) = BuildEvent("white-queen", "tile-e2"); // tile occupied by white pawn
        var engine = new ChessGameBuilder().Compile().Engine;
        var condition = new DestinationNotOwnPieceGameEventCondition();

        // act
        var result = condition.Evaluate(engine, state, evt);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore, $"expected Ignore for friendly occupied destination: {result.Reason}");
    }
}