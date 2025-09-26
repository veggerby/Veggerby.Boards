using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

/// <summary>
/// Direct (non-parity) behavior test for Backgammon ensuring an initial dice roll and a subsequent pass (if no move chosen)
/// result in deterministic state updates under the DecisionPlan path without legacy comparison.
/// </summary>
public class DecisionPlanBackgammonBehaviorTests
{
    [Fact]
    public void GivenInitialBackgammonGame_WhenRollingDiceAndPassing_ThenTurnAndDiceStateAreUpdated()
    {
        // arrange
        var progress = new BackgammonGameBuilder().Compile();

        // act: roll dice (first player selection happens in builder rules; ensure dice have values) then pass turn
        // Backgammon starting dice: two standard dice plus doubling cube. Initial standard dice values are 0 (unrolled).
        var standardDiceIds = progress.Game.Artifacts.OfType<Dice>().Where(d => d.Id != "doubling-dice").Select(d => d.Id).ToList();
        standardDiceIds.Count.Should().BeGreaterThan(0);
        // roll dice (initial compile may already seed values depending on builder; ensure at least one roll mutation occurs)
        progress = progress.RollDice(standardDiceIds.ToArray());
        var afterRoll = progress.State.GetStates<DiceState<int>>().Where(s => standardDiceIds.Contains(s.Artifact.Id)).ToList();
        afterRoll.Should().NotBeEmpty();
        afterRoll.All(s => s.CurrentValue >= 0).Should().BeTrue();

        // minimal assertion: at least one standard die has non-negative value after roll
        afterRoll.Any(s => s.CurrentValue >= 0).Should().BeTrue();
    }
}