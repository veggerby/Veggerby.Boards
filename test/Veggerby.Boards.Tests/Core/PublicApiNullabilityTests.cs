using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

using EventResultStruct = Veggerby.Boards.Flows.Events.EventResult;

namespace Veggerby.Boards.Tests.Core;

/// <summary>
/// Enforces nullability policy for public API surface.
/// </summary>
public class PublicApiNullabilityTests
{
    [Fact]
    public void PublicSurface_Should_Not_Expose_Nullable_Collections_Or_Strings_Or_Mutable_Collections()
    {
        // arrange

        // act

        // assert

        var asm = typeof(Game).Assembly;
        var ctx = new NullabilityInfoContext();
        var whitelist = new HashSet<string>
        {
            // Explicitly modeled absence cases
            typeof(ConditionResponse).FullName + ".Reason",
            typeof(EventResultStruct).FullName + ".Message",
            "Veggerby.Boards.States.PlayerResult.Metrics", // Optional game-specific metrics
            // Simulation trace removed from whitelist after normalization to non-null
            // Serialization types - nullable strings/collections are intentional for optional metadata
            "Veggerby.Boards.Serialization.EventRecord.ResultHash128", // Optional 128-bit hash
            "Veggerby.Boards.Serialization.GameStateSnapshot.Hash128", // Optional 128-bit hash
            "Veggerby.Boards.Serialization.ReplayMetadata.Title", // Optional title
            "Veggerby.Boards.Serialization.ReplayMetadata.Tags", // Optional tags collection
            "Veggerby.Boards.Serialization.ReplayMetadata.CustomMetadata", // Optional custom metadata
            "Veggerby.Boards.Serialization.HashMismatch.EventType", // Optional event type description
            "Veggerby.Boards.Serialization.TurnStateData.Segment", // Optional turn segment
            // Rule priority and conflict resolution diagnostics - nullable is intentional for optional diagnostic data
            "Veggerby.Boards.RuleDecision.RejectedRules", // Optional list of rejected rules (null when no conflict)
            "Veggerby.Boards.RuleDecision.SelectionReason", // Optional explanation of selection
            "Veggerby.Boards.RuleMetadata.StrategyIdentifier", // Optional strategy grouping hint
            "Veggerby.Boards.Flows.DecisionPlan.DecisionPlanEntry.StrategyIdentifier", // Optional strategy identifier
        };
        var offenders = new List<string>();

        // act
        foreach (var t in asm.GetExportedTypes())
        {
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var id = t.FullName + "." + p.Name;
                if (whitelist.Contains(id))
                {
                    continue;
                }

                // Skip inherited Exception properties (HelpLink, Source, etc.)
                if (typeof(Exception).IsAssignableFrom(t) && p.DeclaringType == typeof(Exception))
                {
                    continue;
                }

                var n = ctx.Create(p);

                // Ban nullable string
                if (p.PropertyType == typeof(string) && n.WriteState == NullabilityState.Nullable)
                {
                    offenders.Add(id + " (nullable string)");
                    continue;
                }

                // Ban nullable enumerable (IEnumerable / IReadOnlyCollection / IReadOnlyList / arrays)
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) &&
                    p.PropertyType != typeof(string) &&
                    n.WriteState == NullabilityState.Nullable)
                {
                    offenders.Add(id + " (nullable enumerable)");
                }

                // Ban exposing mutable collections even if non-null (List<>, Dictionary<,>)
                if (p.PropertyType.IsGenericType)
                {
                    var gen = p.PropertyType.GetGenericTypeDefinition();
                    if (gen == typeof(List<>) || gen == typeof(Dictionary<,>))
                    {
                        offenders.Add(id + " (mutable collection)");
                    }
                }
            }
        }

        // assert
        offenders.Should().BeEmpty("no public nullable strings/enumerables or mutable List<>/Dictionary<,> should be exposed in API:\n" + string.Join("\n", offenders));
    }
}
