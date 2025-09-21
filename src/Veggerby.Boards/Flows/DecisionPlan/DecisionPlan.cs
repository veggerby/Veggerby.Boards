using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Flows.DecisionPlan;

/// <summary>
/// Immutable precompiled representation of phase and rule evaluation order.
/// </summary>
/// <remarks>
/// V1 keeps a simple linear ordering of leaf phases discovered via depth-first traversal
/// of the <see cref="GamePhase"/> tree. Each entry binds a phase condition and its rule.
/// No advanced optimizations (masking/short-circuit indexing) are attempted yet; the goal
/// is parity with existing dynamic traversal while establishing a stable index space for
/// future observer instrumentation and performance improvements.
/// </remarks>
/// <summary>
/// Precompiled representation of phase/rule ordering. Exposed publicly only for transparency; subject to change.
/// </summary>
public sealed class DecisionPlan
{
    /// <summary>
    /// Gets the ordered list of compiled entries (leaf phases) considered during evaluation.
    /// </summary>
    public IReadOnlyList<DecisionPlanEntry> Entries { get; }

    /// <summary>
    /// Gets the optional list of compiled groups (when grouping feature enabled at compile time).
    /// Groups provide gate predicates evaluated once prior to scanning member entries.
    /// </summary>
    internal IReadOnlyList<DecisionPlanGroup> Groups { get; }

    /// <summary>
    /// Gets the supported event kinds per entry (index aligned with <see cref="Entries"/>). Internal experimental.
    /// </summary>
    internal EventKind[] SupportedKinds { get; }

    private DecisionPlan(IEnumerable<DecisionPlanEntry> entries, IEnumerable<DecisionPlanGroup> groups, EventKind[] supportedKinds)
    {
        Entries = entries.ToArray();
        Groups = groups?.ToArray() ?? Array.Empty<DecisionPlanGroup>();
        SupportedKinds = supportedKinds ?? Array.Empty<EventKind>();
    }

    /// <summary>
    /// Compiles a decision plan from the provided root phase.
    /// </summary>
    public static DecisionPlan Compile(GamePhase root)
    {
        ArgumentNullException.ThrowIfNull(root);

    var entries = new List<DecisionPlanEntry>();
    var kinds = new List<EventKind>();
    Traverse(root, entries, kinds);

        // Build grouping metadata (contiguous identical condition references) regardless of runtime flag.
        // This avoids having to recompile the plan when flag toggles and keeps plan deterministic.
        var groups = BuildGroups(entries);
        return new DecisionPlan(entries, groups, kinds.ToArray());
    }

    private static void Traverse(GamePhase phase, IList<DecisionPlanEntry> entries, IList<EventKind> kinds)
    {
        if (phase is CompositeGamePhase composite)
        {
            foreach (var child in composite.ChildPhases)
            {
                Traverse(child, entries, kinds);
            }
        }
        else
        {
            // Leaf phase
            bool alwaysValid = false;
            if (phase.Condition is States.Conditions.NullGameStateCondition nullCondition)
            {
                // Evaluate once with a minimal dummy state (NullGameStateCondition ignores the state argument anyway).
                // We pass null intentionally; implementation does not dereference.
                var result = nullCondition.Evaluate(null);
                alwaysValid = ReferenceEquals(result, ConditionResponse.Valid);
            }
            entries.Add(new DecisionPlanEntry(phase.Number, phase.Condition, phase.Rule, phase, alwaysValid));
            kinds.Add(EventKindClassifier.ClassifyRule(phase.Rule));
        }
    }

    /// <summary>
    /// Attempts to resolve a phase by its phase number via depth-first traversal of the original tree.
    /// </summary>
    /// <remarks>
    /// This helper performs a linear search over entries first (fast path) and falls back to null when not found.
    /// Future optimized versions may maintain a dictionary if profiling shows material overhead.
    /// </remarks>
    /// <param name="root">Root phase for traversal.</param>
    /// <param name="phaseNumber">Phase number to locate.</param>
    /// <returns>The matching <see cref="GamePhase"/> or null.</returns>
    internal static GamePhase ResolvePhase(GamePhase root, int phaseNumber)
    {
        if (root is null)
        {
            return null;
        }

        if (root.Number == phaseNumber)
        {
            return root;
        }

        if (root is CompositeGamePhase composite)
        {
            foreach (var child in composite.ChildPhases)
            {
                var match = ResolvePhase(child, phaseNumber);
                if (match is not null)
                {
                    return match;
                }
            }
        }
        return null;
    }

    private static IEnumerable<DecisionPlanGroup> BuildGroups(IReadOnlyList<DecisionPlanEntry> entries)
    {
        if (entries.Count == 0)
        {
            yield break;
        }

        var start = 0;
        var currentCondition = entries[0].Condition;
        for (var i = 1; i < entries.Count; i++)
        {
            if (!ReferenceEquals(entries[i].Condition, currentCondition))
            {
                // close current group
                yield return new DecisionPlanGroup(start, i - start, currentConditionIsGate: true);
                start = i;
                currentCondition = entries[i].Condition;
            }
        }
        // final group
        yield return new DecisionPlanGroup(start, entries.Count - start, currentConditionIsGate: true);
    }
}

/// <summary>
/// Immutable value representing a leaf phase condition + rule pair within a compiled plan.
/// </summary>
/// <summary>
/// Represents a leaf phase entry within a compiled decision plan.
/// </summary>
public readonly record struct DecisionPlanEntry(int PhaseNumber, IGameStateCondition Condition, IGameEventRule Rule, GamePhase Phase, bool ConditionIsAlwaysValid);

/// <summary>
/// Represents a contiguous group of entries sharing the exact same condition reference.
/// The first entry's predicate acts as a gate for all entries when grouping is enabled.
/// </summary>
internal readonly record struct DecisionPlanGroup(int StartIndex, int Length, bool currentConditionIsGate);