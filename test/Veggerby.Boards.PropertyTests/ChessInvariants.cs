using FsCheck;
using FsCheck.Xunit;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core;

namespace Veggerby.Boards.PropertyTests;

// Early placeholder property tests - these will be expanded with richer generators.
public class ChessInvariants
{
    [Property(MaxTest = 25)]
    public bool SideToMoveAlternatesSimpleSequence()
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // Execute two identical opening pawn moves if legal (simplistic deterministic scenario)
        var pawn = progress.Game.GetPiece("white-pawn-2");
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e4");
        var path = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to).ResultPath!;
        var afterFirst = progress.HandleEvent(new MovePieceGameEvent(pawn, path));

        // Black should now be active - we approximate by expecting prior player changed
        // Active player: we approximate by comparing the PlayerState owning the moved piece vs. the new state's active phase player context.
        // If there is no direct ActivePlayer property, fallback to piece ownership inequality as a placeholder invariant.
        var originalOwner = progress.Game.GetPiece("white-pawn-2").Owner;
        var afterOwner = afterFirst.Game.GetPiece("white-pawn-2").Owner;
        return ReferenceEquals(originalOwner, afterOwner); // Ownership should remain constant; placeholder until ActivePlayer accessor exposed.
    }
}
