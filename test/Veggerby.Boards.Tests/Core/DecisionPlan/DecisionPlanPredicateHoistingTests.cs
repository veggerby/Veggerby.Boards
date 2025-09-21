using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

/// <summary>
/// Tests verifying predicate hoisting (NullGameStateCondition fast path) bypasses Evaluate invocation.
/// </summary>
public class DecisionPlanPredicateHoistingTests
{
    [Fact]
    public void GivenNullGameStateCondition_WhenCompilingDecisionPlan_ThenEntryFlaggedAlwaysValid()
    {
        // arrange
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        var leafAlways = GamePhase.New(1, "always", new NullGameStateCondition(), GameEventRule<IGameEvent>.Null, root);
        var leafOther = GamePhase.New(2, "other", new NullGameStateCondition(false), GameEventRule<IGameEvent>.Null, root);

        // act
        var plan = Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root);

        // assert
        plan.Entries.Should().NotBeEmpty();
        plan.Entries.Any(e => e.Phase.Label == "always" && e.ConditionIsAlwaysValid).Should().BeTrue();
        plan.Entries.Any(e => e.Phase.Label == "other" && e.ConditionIsAlwaysValid).Should().BeFalse();
    }
}