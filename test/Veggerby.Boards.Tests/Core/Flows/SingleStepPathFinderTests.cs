using System;
using System.Linq;


using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Flows;

public class SingleStepPathFinderTests
{
    [Theory]
    [InlineData(2, 3, "tile-1", "tile-6", 2)]
    [InlineData(2, 1, "tile-2", "tile-5", 0)] // piece is not on tile-2
    [InlineData(2, null, "tile-1", "tile-3", 1)]
    [InlineData(2, 3, "tile-1", "tile-3", 1)]
    [InlineData(1, 1, "tile-1", "tile-3", 0)]
    public void Should_validate_steps(int? diceValue1, int? diceValue2, string fromTileId, string toTileId, int expectedResult)
    {
        // arrange
        var progress = new TestForPathValidationGameBuilder(diceValue1, diceValue2).Compile();
        var piece1 = progress.Game.GetPiece("piece-1");
        var fromTile = progress.Game.GetTile(fromTileId);
        var toTile = progress.Game.GetTile(toTileId);
        var dice1 = progress.Game.GetArtifact<Dice>("dice-1");
        var dice2 = progress.Game.GetArtifact<Dice>("dice-2");
        var finder = new SingleStepPathFinder(
            new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => e.To.Id.Equals("tile-2") ? ConditionResponse.Invalid : ConditionResponse.Valid),
            dice1, dice2
        );

        // act
        var actual = finder.GetPaths(progress.Engine, progress.State, piece1, fromTile, toTile);

        // assert
        actual.Count().Should().Be(expectedResult);
    }
}