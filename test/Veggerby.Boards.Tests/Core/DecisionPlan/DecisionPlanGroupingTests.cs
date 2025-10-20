using System.Linq;

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
    public int Evaluations
    {
        get; private set;
    }

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

        var plan = Boards.Flows.DecisionPlan.DecisionPlan.Compile(root);
        // DecisionPlan always enabled
        FeatureFlags.EnableDecisionPlanGrouping = true;

        // act (simulate evaluation loop using plan directly)
        // We trigger condition evaluation by invoking the linear logic intentionally via a minimal GameProgress-like check.
        // Instead of constructing full engine, directly iterate groups similar to GameProgress logic.
        var groups = plan.Groups;
        var dummyState = GameState.New(Array.Empty<IArtifactState>());
        foreach (var g in groups)
        {
            var gate = plan.Entries[g.StartIndex];
            if (!gate.ConditionIsAlwaysValid)
            {
                gate.Condition.Evaluate(dummyState); // pass non-null state for nullability satisfaction
            }
        }

        // assert
        cTrue.Evaluations.Should().Be(1);
    }

    [Fact]
    public void GivenGroupedFalseGate_WhenGroupingEnabled_ThenOnlyNonGroupedConditionEvaluatesIndependently()
    {
        // arrange
        var cFalse = new CountingCondition(false);
        var cInner = new CountingCondition(true);
        var root = GamePhase.NewParent(200, "root", new NullGameStateCondition());
        GamePhase.New(1, "a", cFalse, GameEventRule<IGameEvent>.Null, root);
        GamePhase.New(2, "b", cFalse, GameEventRule<IGameEvent>.Null, root); // identical reference ensures grouping
        GamePhase.New(3, "c", cInner, GameEventRule<IGameEvent>.Null, root); // different condition after group

        var plan = Boards.Flows.DecisionPlan.DecisionPlan.Compile(root);
        // DecisionPlan always enabled
        FeatureFlags.EnableDecisionPlanGrouping = true;
        // act
        // Evaluate using same semantics as internal plan evaluation for gating: evaluate group gate only.
        var dummyState = GameState.New(Array.Empty<IArtifactState>());
        foreach (var g in plan.Groups)
        {
            var gate = plan.Entries[g.StartIndex];
            if (!gate.ConditionIsAlwaysValid)
            {
                var resp = gate.Condition.Evaluate(dummyState);
                if (resp != ConditionResponse.Valid)
                {
                    continue; // skip grouped identical phases
                }
            }
            // if gate valid, evaluate rest of group (none should evaluate here because gate is false)
            for (int i = 1; i < g.Length; i++)
            {
                var entry = plan.Entries[g.StartIndex + i];
                if (!entry.ConditionIsAlwaysValid)
                {
                    entry.Condition.Evaluate(dummyState);
                }
            }
        }
        // After groups, evaluate remaining entries (non grouped) — this includes cInner only if gate group passed (it did not)
        var groupedSpan = plan.Groups.SelectMany(g => Enumerable.Range(g.StartIndex, g.Length)).ToHashSet();
        for (int i = 0; i < plan.Entries.Count; i++)
        {
            if (groupedSpan.Contains(i))
            {
                continue;
            }
            var entry = plan.Entries[i];
            if (!entry.ConditionIsAlwaysValid)
            {
                entry.Condition.Evaluate(dummyState);
            }
        }

        // assert
        cFalse.Evaluations.Should().Be(1, "gate evaluated once");
        cInner.Evaluations.Should().Be(1, "non-grouped subsequent condition evaluated independently of failed grouped gate");
    }
}