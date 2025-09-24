using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

internal sealed class CountingCondition(bool result) : IGameStateCondition
{
    private readonly bool _result = result;
    public int Evaluations { get; private set; }

    public ConditionResponse Evaluate(GameState state)
    {
        Evaluations++;
        return _result ? ConditionResponse.Valid : ConditionResponse.Invalid;
    }
}

public class DecisionPlanGroupingTests
{
    [Fact]
    public void GivenContiguousIdenticalConditions_WhenGroupingEnabled_ThenGateEvaluatedOnce()
    {
        // arrange
        var cTrue = new CountingCondition(true);
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        // three phases sharing identical condition reference
        GamePhase.New(1, "a", cTrue, GameEventRule<IGameEvent>.Null, root);
        GamePhase.New(2, "b", cTrue, GameEventRule<IGameEvent>.Null, root);
        GamePhase.New(3, "c", cTrue, GameEventRule<IGameEvent>.Null, root);

        var plan = Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root);
        FeatureFlags.EnableDecisionPlan = true;
        FeatureFlags.EnableDecisionPlanGrouping = true;

        // act (simulate evaluation loop using plan directly)
        // We trigger condition evaluation by invoking the linear logic intentionally via a minimal GameProgress-like check.
        // Instead of constructing full engine, directly iterate groups similar to GameProgress logic.
        var groups = plan.Groups;
        foreach (var g in groups)
        {
            var gate = plan.Entries[g.StartIndex];
            if (!gate.ConditionIsAlwaysValid)
            {
                gate.Condition.Evaluate(null); // pass null; CountingCondition ignores
            }
        }

        // assert
        cTrue.Evaluations.Should().Be(1);
    }
}