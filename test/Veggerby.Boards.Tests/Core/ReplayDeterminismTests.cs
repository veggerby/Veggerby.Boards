using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core;

public class ReplayDeterminismTests
{
    [Fact]
    public void GivenSameSeedAndEventSequence_WhenReplayed_ThenFinalHashesMatch()
    {
        // arrange
        const ulong seed = 1337UL;
        using var flags = new FeatureFlagScope(hashing: true);
        var builder = new TestGameBuilder(useSimpleGamePhase: false).WithSeed(seed);
        var a = builder.Compile();
        var b = builder.WithSeed(seed).Compile();

        var pieceId = "piece-1";
        var from = a.Game.GetTile("tile-1");
        var to = a.Game.GetTile("tile-2");
        var relation = a.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        var path = new TilePath([relation]);
        var move = new MovePieceGameEvent(a.Game.GetPiece(pieceId), path);

        // deterministic sequence: move piece forth then back twice
        var events = new[] { move, move, move, move }; // idempotent path for hashing equivalence

        // act
        foreach (var e in events)
        {
            a = a.HandleEvent(e);
            b = b.HandleEvent(e);
        }

        // assert
        a.State.Hash.Should().Be(b.State.Hash);
        a.State.Hash128.Should().Be(b.State.Hash128);
        a.State.Random.Seed.Should().Be(b.State.Random.Seed);
    }
}