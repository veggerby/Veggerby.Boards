using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;

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

    private DecisionPlan(IEnumerable<DecisionPlanEntry> entries)
    {
        Entries = entries.ToArray();
    }

    /// <summary>
    /// Compiles a decision plan from the provided root phase.
    /// </summary>
    public static DecisionPlan Compile(GamePhase root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var entries = new List<DecisionPlanEntry>();
        Traverse(root, entries);
        return new DecisionPlan(entries);
    }

    private static void Traverse(GamePhase phase, IList<DecisionPlanEntry> entries)
    {
        if (phase is CompositeGamePhase composite)
        {
            foreach (var child in composite.ChildPhases)
            {
                Traverse(child, entries);
            }
        }
        else
        {
            // Leaf phase
            entries.Add(new DecisionPlanEntry(phase.Number, phase.Condition, phase.Rule));
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
}

/// <summary>
/// Immutable value representing a leaf phase condition + rule pair within a compiled plan.
/// </summary>
/// <summary>
/// Represents a leaf phase entry within a compiled decision plan.
/// </summary>
public readonly record struct DecisionPlanEntry(int PhaseNumber, IGameStateCondition Condition, IGameEventRule Rule);