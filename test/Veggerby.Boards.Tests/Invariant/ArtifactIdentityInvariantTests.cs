using System;
using System.Linq;

using Veggerby.Boards.Chess;

namespace Veggerby.Boards.Tests.Invariant;

public class ArtifactIdentityInvariantTests
{
    [Fact(DisplayName = "Artifact Ids are globally unique in initial Chess game")]
    [Trait("Category", "Invariant")]
    public void GivenInitialChessGame_WhenInspectingArtifacts_ThenIdsAreUnique()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();

        // act
        var ids = progress.Game.Artifacts.Select(a => a.Id).ToArray();

        // assert
        ids.Length.Should().Be(ids.Distinct(StringComparer.Ordinal).Count());
    }
}