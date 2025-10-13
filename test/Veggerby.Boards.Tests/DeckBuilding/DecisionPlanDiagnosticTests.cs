using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;

namespace Veggerby.Boards.Tests.DeckBuilding;

/// <summary>
/// Temporary diagnostic tests to dump the deck-building DecisionPlan structure and basic condition viability.
/// Marked Skip=false intentionally for immediate visibility; remove once underlying issue fixed.
/// </summary>
public class DecisionPlanDiagnosticTests
{
    private static IEnumerable<(int Index, string Phase, Type EventType, bool AlwaysValid)> Flatten(DecisionPlan plan)
    {
        var i = 0;
        foreach (var entry in plan.Entries)
        {
            foreach (var evtType in ExtractEventTypes(entry.Rule))
            {
                yield return (i, entry.Phase?.Label, evtType, entry.ConditionIsAlwaysValid);
            }
            i++;
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
        var type = rule.GetType();
        while (type != null && type != typeof(object))
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

    [Fact(Skip = "Diagnostic output only; remove once fixed")]
    public void Print_Plan_Flattened()
    {
        // arrange
        var progress = new DeckBuildingGameBuilder().Compile();
        var plan = progress.Engine.DecisionPlan;

        // act
        var flattened = Flatten(plan).ToList();

        // assert (sanity: expect at least 1 entry)
        flattened.Should().NotBeEmpty();

        // Write diagnostic lines to test output
        var lines = flattened
            .Select(f => $"[{f.Index}] phase={f.Phase ?? "<null>"} evt={f.EventType.Name} alwaysValid={(f.AlwaysValid ? "Y" : "N")}")
            .ToList();
        // We force an assertion that includes the dump for visibility if future failures occur.
        lines.Count.Should().BeGreaterThan(0, string.Join("\n", lines));
    }

    [Fact]
    public void DeckBuildingCore_Expected_EventTypes_Appear_InPlan()
    {
        // arrange
        var progress = new DeckBuildingGameBuilder().Compile();
        var plan = progress.Engine.DecisionPlan;
        var flattened = Flatten(plan).ToList();

        // act
        bool Has(string phase, Type evt) => flattened.Any(x => string.Equals(x.Phase, phase, StringComparison.Ordinal) && x.EventType == evt);

        var hasCreate = Has("db-setup", typeof(CreateDeckEvent));
        var hasDraw = Has("db-action", typeof(DrawWithReshuffleEvent));
        var hasTrash = Has("db-action", typeof(TrashFromHandEvent));
        var hasGain = Has("db-buy", typeof(GainFromSupplyEvent));
        var hasCleanup = Has("db-cleanup", typeof(CleanupToDiscardEvent));
        var hasAdvanceSetup = Has("db-setup", typeof(EndTurnSegmentEvent));
        var hasAdvanceAction = Has("db-action", typeof(EndTurnSegmentEvent));
        var hasAdvanceBuy = Has("db-buy", typeof(EndTurnSegmentEvent));
        var hasAdvanceCleanup = Has("db-cleanup", typeof(EndTurnSegmentEvent));

        // assert
        hasCreate.Should().BeTrue("CreateDeckEvent should appear under db-setup");
        hasDraw.Should().BeTrue("DrawWithReshuffleEvent should appear under db-action");
        hasTrash.Should().BeTrue("TrashFromHandEvent should appear under db-action");
        hasGain.Should().BeTrue("GainFromSupplyEvent should appear under db-buy");
        hasCleanup.Should().BeTrue("CleanupToDiscardEvent should appear under db-cleanup");
        hasAdvanceSetup.Should().BeTrue("EndTurnSegmentEvent should appear under db-setup");
        hasAdvanceAction.Should().BeTrue("EndTurnSegmentEvent should appear under db-action");
        hasAdvanceBuy.Should().BeTrue("EndTurnSegmentEvent should appear under db-buy");
        hasAdvanceCleanup.Should().BeTrue("EndTurnSegmentEvent should appear under db-cleanup");
    }
}