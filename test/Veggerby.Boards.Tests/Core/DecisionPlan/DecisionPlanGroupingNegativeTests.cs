using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

internal sealed class CountingFalseCondition : IGameStateCondition
{
    public int Evaluations { get; private set; }
    public ConditionResponse Evaluate(GameState state)
    {
        Evaluations++;
        return ConditionResponse.Invalid;
    }
}

public class DecisionPlanGroupingNegativeTests
{
    [Fact]
    public void GivenFalseGateCondition_WhenGroupingEnabled_ThenGroupSkippedAndEvaluatedOnce()
    {
        // arrange
        var falseGate = new CountingFalseCondition();
        var root = GamePhase.NewParent(200, "root", new NullGameStateCondition());
        GamePhase.New(10, "a", falseGate, GameEventRule<IGameEvent>.Null, root);
        GamePhase.New(11, "b", falseGate, GameEventRule<IGameEvent>.Null, root);
        GamePhase.New(12, "c", falseGate, GameEventRule<IGameEvent>.Null, root);
        var plan = Boards.Flows.DecisionPlan.DecisionPlan.Compile(root);
        using var _ = new Infrastructure.FeatureFlagScope(decisionPlan: true, grouping: true);

        // act - simulate grouped evaluation gate path
        foreach (var g in plan.Groups)
        {
            var gate = plan.Entries[g.StartIndex];
            gate.Condition.Evaluate(null); // one evaluation only
        }

        // assert
        falseGate.Evaluations.Should().Be(1);
    }
}