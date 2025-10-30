using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AwesomeAssertions;

using Xunit;

namespace Veggerby.Boards.Tests.Documentation;

public class EqualityHashingPatternsTests
{
    [Fact]
    public void GivenAssemblyTypes_WhenScanning_ThenOnlyExpectedTypesOverrideEqualityOrHashCode()
    {
        // arrange
        var assembly = typeof(Veggerby.Boards.Artifacts.Game).Assembly;
        // Expected core set (minimum required overrides). Additional overriding types must be explicitly justified or added here.
        var expected = new HashSet<string>
        {
            // Core concrete artifacts (abstract base 'Artifact' excluded intentionally)
            "Veggerby.Boards.Artifacts.Board",
            "Veggerby.Boards.Artifacts.Dice",
            "Veggerby.Boards.Artifacts.Piece",
            "Veggerby.Boards.Artifacts.Player",
            "Veggerby.Boards.Artifacts.Tile",
            "Veggerby.Boards.Artifacts.CompositeArtifact`1", // generic arity included
            // Patterns
            "Veggerby.Boards.Artifacts.Patterns.AnyPattern",
            "Veggerby.Boards.Artifacts.Patterns.DirectionPattern",
            "Veggerby.Boards.Artifacts.Patterns.FixedPattern",
            // Value types / records
            "Veggerby.Boards.Internal.Bitboards.Bitboard64",
            "Veggerby.Boards.ConditionResponse",
            // States
            "Veggerby.Boards.States.GameState",
            "Veggerby.Boards.States.PieceState",
            "Veggerby.Boards.States.ActivePlayerState",
            "Veggerby.Boards.States.CapturedPieceState",
            "Veggerby.Boards.States.DiceState`1",
            "Veggerby.Boards.States.NullDiceState",
            "Veggerby.Boards.States.TurnState"
        };

        bool Overrides(MethodInfo? m, Type t)
        {
            if (m is null)
            {
                return false;
            }

            return m.DeclaringType == t && m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        var overridingTypes = new List<Type>();
        foreach (var t in assembly.GetTypes())
        {
            if (!t.IsPublic && !t.IsNestedPublic)
            {
                continue; // focus on public surface
            }
            if (t.IsAbstract)
            {
                continue;
            }

            var equalsObj = t.GetMethod("Equals", new[] { typeof(object) });
            var hashCode = t.GetMethod("GetHashCode", Type.EmptyTypes);
            var overridesEquals = Overrides(equalsObj, t);
            var overridesHash = Overrides(hashCode, t);
            if (overridesEquals || overridesHash)
            {
                overridingTypes.Add(t);
                if (overridesEquals)
                {
                    // assert hash code also overridden when equals is overridden (identity contract)
                    overridesHash.Should().BeTrue($"Type '{t.FullName}' overrides Equals(object) but not GetHashCode()");
                }
            }
        }

        var actualSet = overridingTypes.Select(x => x.FullName).ToHashSet();
        // Assert expected subset is present and capture any missing for diagnostic clarity
        var missing = expected.Where(e => !actualSet.Contains(e)).ToList();
        actualSet.IsSupersetOf(expected).Should().BeTrue("Expected core override set must be present. Missing: {0}", string.Join(",", missing));
        // Identify unexpected overrides (that are not in expected and not obviously generic pattern variants)
        var allowedAdditionalPrefixes = new[]
        {
        "Veggerby.Boards.Artifacts.Patterns.", // pattern variants acceptable
        "Veggerby.Boards.Flows.", // decision/events value records
        "Veggerby.Boards.Simulation.", // simulation result records
        "Veggerby.Boards.Artifacts.Relations." // relation identity helpers (Direction, etc.)
    };
        var unexpected = actualSet.Where(x => x is not null && !expected.Contains(x) && !allowedAdditionalPrefixes.Any(pref => x.StartsWith(pref, StringComparison.Ordinal))).ToList();
        unexpected.Should().BeEmpty("No unexpected equality overrides should appear without explicit allowance: {0}", string.Join(",", unexpected));

        // act
        // (no action beyond reflection scanning)

        // assert
        // Subset & unexpected checks already performed.
    }
}
