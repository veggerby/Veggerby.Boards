using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.DeckBuilding.Artifacts;
using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.DeckBuilding.States;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;
namespace Veggerby.Boards.Tests.DeckBuilding;

/// <summary>
/// Structural invariants for the deck-building DecisionPlan to guard against accidental phase/rule wiring regressions.
/// </summary>
public class DecisionPlanInvariants
{
    private static IEnumerable<(string PhaseLabel, Type EventType)> Flatten(DecisionPlan plan)
    {
        foreach (var entry in plan.Entries)
        {
            var phaseLabel = entry.Phase?.Label ?? string.Empty;
            foreach (var evtType in ExtractEventTypes(entry.Rule))
            {
                yield return (phaseLabel, evtType);
            }
        }
    }

    private static IEnumerable<Type> ExtractEventTypes(IGameEventRule rule)
    {
        if (rule is null)
        {
            yield break;
        }

        if (rule is CompositeGameEventRule composite)
        {
            foreach (var child in composite.Rules)
            {
                foreach (var t in ExtractEventTypes(child))
                {
                    yield return t;
                }
            }
            yield break;
        }

        // Heuristic: non-composite rules are generic GameEventRule<TEvent>. We discover TEvent via interfaces/base types.
        var type = rule.GetType();
        while (type is not null && type != typeof(object))
        {
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                if (args.Length == 1 && typeof(IGameEvent).IsAssignableFrom(args[0]))
                {
                    yield return args[0];
                    yield break;
                }
            }
            type = type.BaseType;
        }
    }

    private static bool HasPhaseEvent(DecisionPlan plan, string phaseLabel, Type eventType)
    {
        return Flatten(plan).Any(x => string.Equals(x.PhaseLabel, phaseLabel, StringComparison.Ordinal) && x.EventType == eventType);
    }

    [Fact]
    public void DecisionPlan_Contains_DrawRule_In_ActionPhase()
    {
        // arrange

        // act

        // assert

        var progress = new DeckBuildingGameBuilder().Compile();

        // act
        var hasDrawRule = HasPhaseEvent(progress.Engine.DecisionPlan, "db-action", typeof(DrawWithReshuffleEvent));

        // assert
        hasDrawRule.Should().BeTrue("DrawWithReshuffleEvent rule must be present under db-action phase");
    }

    [Fact]
    public void DecisionPlan_Contains_GainRule_In_BuyPhase()
    {
        // arrange

        // act

        // assert

        var progress = new DeckBuildingGameBuilder().Compile();

        // act
        var hasGainRule = HasPhaseEvent(progress.Engine.DecisionPlan, "db-buy", typeof(GainFromSupplyEvent));

        // assert
        hasGainRule.Should().BeTrue("GainFromSupplyEvent rule must be present under db-buy phase");
    }

    [Fact]
    public void DecisionPlan_Contains_TrashRule_In_ActionPhase()
    {
        // arrange

        // act

        // assert

        var progress = new DeckBuildingGameBuilder().Compile();

        // act
        var hasTrashRule = HasPhaseEvent(progress.Engine.DecisionPlan, "db-action", typeof(TrashFromHandEvent));

        // assert
        hasTrashRule.Should().BeTrue("TrashFromHandEvent rule must be present under db-action phase");
    }

    [Fact]
    public void DecisionPlan_Contains_CleanupRule_In_CleanupPhase()
    {
        // arrange

        // act

        // assert

        var progress = new DeckBuildingGameBuilder().Compile();

        // act
        var hasCleanupRule = HasPhaseEvent(progress.Engine.DecisionPlan, "db-cleanup", typeof(CleanupToDiscardEvent));

        // assert
        hasCleanupRule.Should().BeTrue("CleanupToDiscardEvent rule must be present under db-cleanup phase");
    }

    [Fact]
    public void DecisionPlan_Contains_TurnAdvance_In_AllSegments()
    {
        // arrange

        // act

        // assert

        var progress = new DeckBuildingGameBuilder().Compile();
        var plan = progress.Engine.DecisionPlan;

        // act
        var actionAdvance = HasPhaseEvent(plan, "db-action", typeof(EndTurnSegmentEvent));
        var buyAdvance = HasPhaseEvent(plan, "db-buy", typeof(EndTurnSegmentEvent));
        var setupAdvance = HasPhaseEvent(plan, "db-setup", typeof(EndTurnSegmentEvent));
        var cleanupAdvance = HasPhaseEvent(plan, "db-cleanup", typeof(EndTurnSegmentEvent));

        // assert
        setupAdvance.Should().BeTrue("EndTurnSegmentEvent advancement must be present in setup phase");
        actionAdvance.Should().BeTrue("EndTurnSegmentEvent advancement must be present in action phase");
        buyAdvance.Should().BeTrue("EndTurnSegmentEvent advancement must be present in buy phase");
        cleanupAdvance.Should().BeTrue("EndTurnSegmentEvent advancement must be present in cleanup phase");
    }

    [Fact]
    public void DecisionPlan_EndGameEvent_Comes_After_ComputeScoresEvent()
    {
        // arrange

        // act

        // assert

        var progress = new DeckBuildingGameBuilder().Compile();
        var flattened = Flatten(progress.Engine.DecisionPlan)
            .Select((x, idx) => new { x.PhaseLabel, x.EventType, Index = idx })
            .ToList();

        // act
        var computeIdx = flattened.FirstOrDefault(e => e.PhaseLabel == "db-cleanup" && e.EventType == typeof(ComputeScoresEvent))?.Index ?? -1;
        var endGameIdx = flattened.FirstOrDefault(e => e.PhaseLabel == "db-cleanup" && e.EventType == typeof(EndGameEvent))?.Index ?? -1;

        // assert
        computeIdx.Should().BeGreaterThanOrEqualTo(0, "ComputeScoresEvent must exist in cleanup phase");
        endGameIdx.Should().BeGreaterThanOrEqualTo(0, "EndGameEvent must exist in cleanup phase");
        endGameIdx.Should().BeGreaterThan(computeIdx, "EndGameEvent must occur after ComputeScoresEvent within cleanup phase to ensure scores are finalized before termination");
    }
}
