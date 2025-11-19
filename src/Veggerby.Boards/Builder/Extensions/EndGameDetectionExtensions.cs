using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Phases;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Builder.Extensions;

/// <summary>
/// Extension methods for common endgame detection patterns.
/// </summary>
/// <remarks>
/// These helpers provide convenient fluent methods for common win conditions.
/// Game-specific endgame logic should still be implemented in module-specific conditions and mutators.
/// </remarks>
public static class EndGameDetectionExtensions
{
    /// <summary>
    /// Configures custom endgame detection with a fluent interface.
    /// </summary>
    /// <param name="phaseDefinition">The phase definition.</param>
    /// <param name="conditionFactory">Factory producing the endgame condition.</param>
    /// <param name="mutatorFactory">Factory producing the mutator that adds terminal states.</param>
    /// <returns>The phase definition for fluent chaining.</returns>
    /// <remarks>
    /// This is a convenience wrapper around the existing WithEndGameDetection method
    /// that maintains consistency with the new fluent API naming conventions.
    /// </remarks>
    public static IGamePhaseDefinition WithEndGame(
        this IGamePhaseDefinition phaseDefinition,
        GameStateConditionFactory conditionFactory,
        Func<Game, IStateMutator<IGameEvent>> mutatorFactory)
    {
        ArgumentNullException.ThrowIfNull(phaseDefinition, nameof(phaseDefinition));
        ArgumentNullException.ThrowIfNull(conditionFactory, nameof(conditionFactory));
        ArgumentNullException.ThrowIfNull(mutatorFactory, nameof(mutatorFactory));

        return phaseDefinition.WithEndGameDetection(conditionFactory, mutatorFactory);
    }
}
