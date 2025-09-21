using System;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal.Debug;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

public class DecisionPlanDebugParityTests
{
    [Fact]
    public void GivenSimpleMove_WhenDebugParityEnabled_ThenNoDivergence()
    {
        // arrange
        using var _ = new FeatureFlagScope(decisionPlan: true, debugParity: true);
        var progress = new ChessGameBuilder().Compile();
        var piece = progress.Game.GetPiece("white-pawn-5"); // located at e2
                                                            // use single-step advance (e2 -> e3) for deterministic, currently supported pawn movement
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e3");
        var visitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
        piece.Patterns.Single().Accept(visitor);
        var evt = new MovePieceGameEvent(piece, visitor.ResultPath!);

        // act
        var updated = progress.HandleEvent(evt);

        // assert
        updated.State.GetState<PieceState>(piece).CurrentTile.Should().Be(to);
    }

    [Fact]
    public void GivenForcedMismatch_WhenDebugParityEnabled_ThenException()
    {
        // arrange
        using var _ = new FeatureFlagScope(decisionPlan: true, debugParity: true);
        DebugParityTestHooks.ForceMismatch = true;
        var progress = new ChessGameBuilder().Compile();
        var piece = progress.Game.GetPiece("white-pawn-5");
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e3");
        var visitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
        piece.Patterns.Single().Accept(visitor);
        var evt = new MovePieceGameEvent(piece, visitor.ResultPath!);

        // act
        var act = () => progress.HandleEvent(evt);

        // assert
        act.Should().Throw<BoardException>().WithMessage("*parity divergence*");
        DebugParityTestHooks.ForceMismatch = false; // cleanup
    }
}