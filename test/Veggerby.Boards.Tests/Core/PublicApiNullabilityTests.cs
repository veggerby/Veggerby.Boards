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
    public void PublicSurface_Should_Not_Expose_Nullable_Collections_Or_Strings()
    {
        // arrange
        var asm = typeof(Game).Assembly;
        var ctx = new NullabilityInfoContext();
        var whitelist = new HashSet<string>
        {
            // Explicitly modeled absence cases
            typeof(ConditionResponse).FullName + ".Reason",
            typeof(EventResultStruct).FullName + ".Message",
            // Simulation trace removed from whitelist after normalization to non-null
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
            }
        }

        // assert
        offenders.Should().BeEmpty("no public string?/IEnumerable<>? should be exposed in API:\n" + string.Join("\n", offenders));
    }
}