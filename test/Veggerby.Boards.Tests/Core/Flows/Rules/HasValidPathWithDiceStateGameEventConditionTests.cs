using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Rules
{
    public class HasValidPathWithDiceStateGameEventConditionTests
    {
        [Theory]
        [InlineData(2, 3, "tile-1", "tile-6", ConditionResult.Valid)]
        [InlineData(2, 1, "tile-2", "tile-5", ConditionResult.Invalid)]
        [InlineData(2, null, "tile-1", "tile-3", ConditionResult.Valid)]
        [InlineData(2, 3, "tile-1", "tile-3", ConditionResult.Valid)]
        [InlineData(1, 1, "tile-1", "tile-3", ConditionResult.Invalid)]
        public void Should_validate_steps(int? diceValue1, int? diceValue2, string fromTileId, string toTileId, ConditionResult expectedResult)
        {
            // arrange
            var progress = new TestForPathValidationGameBuilder(diceValue1, diceValue2).Compile();
            var piece1 = progress.Game.GetPiece("piece-1");
            var fromTile = progress.Game.GetTile(fromTileId);
            var toTile = progress.Game.GetTile(toTileId);
            var dice1 = progress.Game.GetArtifact<Dice>("dice-1");
            var dice2 = progress.Game.GetArtifact<Dice>("dice-2");
            var @event = new MovePieceGameEvent(piece1, fromTile, toTile);
            var condition = new HasValidPathWithDiceStateGameEventCondition(
                new SimpleGameEventCondition<MovePieceGameEvent>((eng, s, e) => e.To.Id.Equals("tile-2") ? ConditionResponse.Invalid : ConditionResponse.Valid),
                dice1, dice2
            );

            // act
            var actual = condition.Evaluate(progress.Engine, progress.State, @event);

            // assert
            actual.Result.ShouldBe(expectedResult);
        }
    }
}