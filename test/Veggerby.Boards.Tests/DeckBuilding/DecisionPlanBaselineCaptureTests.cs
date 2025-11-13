using System.Linq;

using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;

namespace Veggerby.Boards.Tests.DeckBuilding;

/// <summary>
/// TEMPORARY: forces a failure to dump current deck-building DecisionPlan entries for establishing a locked baseline.
/// Will be removed once baseline constants are created.
/// </summary>
public class DecisionPlanBaselineCaptureTests
{
    private static System.Collections.Generic.IEnumerable<System.Type> ExtractEventTypes(IGameEventRule rule)
    {
        if (rule is null)
            yield break;
        if (rule is CompositeGameEventRule composite)
        {
            foreach (var child in composite.Rules)
            {
                foreach (var t in ExtractEventTypes(child))
                    yield return t;
            }
            yield break;
        }
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

    [Fact(Skip = "Baseline captured; leave skipped unless intentionally regenerating.")]
    public void CapturePlan()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var progress = new DeckBuildingGameBuilder().Compile();
        var plan = progress.Engine.DecisionPlan;
        var lines = plan.Entries
            .SelectMany(e => ExtractEventTypes(e.Rule).Select(t => (Phase: e.Phase?.Label ?? "<null>", Event: t.Name)))
            .Select(p => $"##PLAN-BL## {p.Phase}:{p.Event}")
            .ToList();
        var joined = string.Join("|", lines.Select(l => l.Substring("##PLAN-BL## ".Length)));
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(joined.Replace("##PLAN-BL## ", ""))));
        lines.Add($"##PLAN-BL-SHA## {hash}");
        // Force visible output by failing intentionally (unskip when capturing)
        lines.Count.Should().Be(-1, string.Join("\n", lines)); // preserved for future regeneration when unskipped
    }
}