using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using AwesomeAssertions;

using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Events; // IGameEvent
using Veggerby.Boards.Flows.Rules;

using Xunit;

namespace Veggerby.Boards.Tests.DeckBuilding;

/// <summary>
/// Verifies the deck-building DecisionPlan has a deterministic ordered signature of phase/event mappings.
/// Guards against accidental reordering or insertion/removal of core events without an accompanying invariant update.
/// <remarks>
/// Regenerating the baseline:
/// 1. Temporarily un-skip the test in DecisionPlanBaselineCaptureTests.CapturePlan and run the test suite.
/// 2. Copy the emitted lines beginning with '##PLAN-BL##' (ordered) and the '##PLAN-BL-SHA##' value.
/// 3. Replace Entries + Signature in DecisionPlanBaseline.cs keeping ordering stable.
/// 4. Re-skip the capture test and re-run this test; it should pass with zero diff.
/// 5. Update CHANGELOG (Unreleased > Added/Changed) noting the intentional plan modification.
///</remarks>
/// </summary>
public class DecisionPlanSignatureTests
{
    private static string ComputeSignature(DecisionPlan plan)
    {
        // Flatten (PhaseLabel, EventTypeName) respecting entry ordering and composite expansion ordering.
        var parts = plan.Entries
            .SelectMany(e => ExtractEventTypes(e.Rule).Select(t => (Phase: e.Phase?.Label ?? "<null>", Event: t.Name)))
            .Select(p => p.Phase + ":" + p.Event)
            .ToArray();

        var joined = string.Join("|", parts);
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexString(bytes);
    }

    private static System.Collections.Generic.IEnumerable<Type> ExtractEventTypes(IGameEventRule rule)
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

    [Fact]
    public void DecisionPlan_Matches_Locked_Baseline()
    {
        using var guard = Veggerby.Boards.Tests.Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var progress = new DeckBuildingGameBuilder().Compile();
        var plan = progress.Engine.DecisionPlan;

        var currentEntries = plan.Entries
            .SelectMany(e => ExtractEventTypes(e.Rule).Select(t => (Phase: e.Phase?.Label ?? "<null>", Event: t.Name)))
            .Select(p => p.Phase + ":" + p.Event)
            .ToArray();
        var currentSignature = ComputeSignature(plan);

        // Baseline length guard (quick signal for accidental insertion/removal)
        currentEntries.Length.Should().Be(DecisionPlanBaseline.Entries.Length, "baseline entry count mismatch (possible insertion/removal). If intentional, regenerate baseline.");

        if (!string.Equals(currentSignature, DecisionPlanBaseline.Signature, StringComparison.Ordinal))
        {
            var diff = BuildDiff(DecisionPlanBaseline.Entries, currentEntries);
            currentSignature.Should().Be(DecisionPlanBaseline.Signature, "signature mismatch\n" + diff);
        }

        if (!currentEntries.SequenceEqual(DecisionPlanBaseline.Entries, StringComparer.Ordinal))
        {
            var diff = BuildDiff(DecisionPlanBaseline.Entries, currentEntries);
            currentEntries.Should().Equal(DecisionPlanBaseline.Entries, diff);
        }

        if (!string.Equals(currentSignature, DecisionPlanBaseline.Signature, StringComparison.Ordinal))
        {
            var diff = BuildDiff(DecisionPlanBaseline.Entries, currentEntries);
            currentSignature.Should().Be(DecisionPlanBaseline.Signature, "signature mismatch\n" + diff);
        }

        if (!currentEntries.SequenceEqual(DecisionPlanBaseline.Entries, StringComparer.Ordinal))
        {
            var diff = BuildDiff(DecisionPlanBaseline.Entries, currentEntries);
            currentEntries.Should().Equal(DecisionPlanBaseline.Entries, diff);
        }
    }

    private static string BuildDiff(string[] expected, string[] actual, int context = 3)
    {
        var sb = new StringBuilder();
        sb.AppendLine("DecisionPlan diff (expected vs actual)");
        int max = Math.Max(expected.Length, actual.Length);
        int firstMismatch = -1;
        for (int i = 0; i < max; i++)
        {
            var e = i < expected.Length ? expected[i] : "<missing>";
            var a = i < actual.Length ? actual[i] : "<missing>";
            if (!string.Equals(e, a, StringComparison.Ordinal)) { firstMismatch = i; break; }
        }
        if (firstMismatch == -1)
        {
            sb.AppendLine("(Signatures differ but entries appear identical; ensure same flattening logic.)");
            return sb.ToString();
        }
        int start = Math.Max(0, firstMismatch - context);
        int end = Math.Min(max - 1, firstMismatch + context);
        for (int i = start; i <= end; i++)
        {
            var e = i < expected.Length ? expected[i] : "<missing>";
            var a = i < actual.Length ? actual[i] : "<missing>";
            if (string.Equals(e, a, StringComparison.Ordinal))
            {
                sb.AppendLine($"  {i,3}:   {a}");
            }
            else
            {
                sb.AppendLine($"- {i,3}:   {e}");
                sb.AppendLine($"+ {i,3}:   {a}");
            }
        }
        return sb.ToString();
    }
}