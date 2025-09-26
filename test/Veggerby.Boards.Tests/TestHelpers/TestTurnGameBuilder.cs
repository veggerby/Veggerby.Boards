using Veggerby.Boards;
using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Tests.TestHelpers;

/// <summary>
/// Minimal concrete game builder used by tests that require a compiled engine with players only.
/// </summary>
internal sealed class TestTurnGameBuilder : GameBuilder
{
    private readonly Player[] _players;

    public TestTurnGameBuilder(params Player[] players)
    {
        _players = players;
    }

    protected override void Build()
    {
        BoardId = "test-board";
        foreach (var p in _players)
        {
            AddPlayer(p.Id);
        }
        // minimal board topology (2 tiles + 1 direction + relation) to satisfy Board invariants
        AddTile("tile-a");
        AddTile("tile-b");
        AddDirection("dir-forward");
        // declare a single directional relation tile-a -> tile-b
        WithTile("tile-a").WithRelationTo("tile-b").InDirection("dir-forward").Done();

        // add a single dummy piece artifact attached to first player (if any) so artifacts list is non-empty
        if (_players.Length > 0)
        {
            AddPiece("piece-dummy").WithOwner(_players[0].Id).OnTile("tile-a");
        }
    }
}