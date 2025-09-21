using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Flows.Phases;

/// <summary>
/// Represents a logical stage of play gated by a <see cref="IGameStateCondition"/> and governed by a single <see cref="IGameEventRule"/>.
/// </summary>
public class GamePhase
{
    /// <summary>
    /// Gets the sequential number identifying this phase.
    /// </summary>
    public int Number { get; }

    /// <summary>
    /// Gets the optional human readable label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the parent composite phase (nullable).
    /// </summary>
    public CompositeGamePhase Parent { get; }

    /// <summary>
    /// Gets the event pre-processors applied before rule evaluation.
    /// </summary>
    public IEnumerable<IGameEventPreProcessor> PreProcessors { get; }

    /// <summary>
    /// Gets the activation condition for the phase.
    /// </summary>
    public IGameStateCondition Condition { get; }

    /// <summary>
    /// Gets the rule executed when the phase is active.
    /// </summary>
    public IGameEventRule Rule { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GamePhase"/> class.
    /// </summary>
    /// <param name="number">Sequential phase number (must be positive).</param>
    /// <param name="label">Optional label.</param>
    /// <param name="condition">Activation condition.</param>
    /// <param name="rule">Event rule applied when active.</param>
    /// <param name="parent">Optional parent composite phase.</param>
    /// <param name="preProcessors">Optional pre-processors.</param>
    protected GamePhase(int number, string label, IGameStateCondition condition, IGameEventRule rule, CompositeGamePhase parent, IEnumerable<IGameEventPreProcessor> preProcessors)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Round number must be positive and non-zero");
        }

        ArgumentNullException.ThrowIfNull(condition);

        ArgumentNullException.ThrowIfNull(rule);

        Number = number;
        Label = label;
        Condition = condition;
        Rule = rule;
        PreProcessors = preProcessors;

        Parent = parent;
        parent?.Add(this);
    }

    /// <summary>
    /// Returns this phase if its condition is satisfied; otherwise null.
    /// </summary>
    /// <param name="gameState">Current state.</param>
    /// <returns>The active phase or null.</returns>
    public virtual GamePhase GetActiveGamePhase(GameState gameState)
    {
        return Condition.Evaluate(gameState).Equals(ConditionResponse.Valid) ? this : null;
    }

    /// <summary>
    /// Applies pre-processors to an incoming event yielding zero or more derived events.
    /// </summary>
    /// <param name="progress">Current progress snapshot.</param>
    /// <param name="event">Original event.</param>
    /// <returns>Enumeration of events (original if no processors).</returns>
    public IEnumerable<IGameEvent> PreProcessEvent(GameProgress progress, IGameEvent @event)
    {
        if (!PreProcessors.Any())
        {
            return [@event];
        }

        var preProcessedEvents = PreProcessors
            .SelectMany(x => x.ProcessEvent(progress, @event))
            .Where(x => x is not null)
            .ToList();

        return preProcessedEvents;
    }

    /// <summary>
    /// Factory for a leaf phase.
    /// </summary>
    /// <param name="number">Phase number.</param>
    /// <param name="label">Label.</param>
    /// <param name="condition">Activation condition.</param>
    /// <param name="rule">Rule governing events.</param>
    /// <param name="parent">Optional parent composite.</param>
    /// <param name="preProcessors">Optional pre-processors.</param>
    /// <returns>New phase.</returns>
    public static GamePhase New(int number, string label, IGameStateCondition condition, IGameEventRule rule, CompositeGamePhase parent = null, IEnumerable<IGameEventPreProcessor> preProcessors = null)
    {
        return new GamePhase(number, label, condition, rule, parent, preProcessors);
    }

    /// <summary>
    /// Factory for a composite phase container.
    /// </summary>
    /// <param name="number">Phase number.</param>
    /// <param name="label">Label.</param>
    /// <param name="condition">Activation condition (defaults to null condition).</param>
    /// <param name="parent">Higher level composite parent.</param>
    /// <param name="preProcessors">Optional pre-processors.</param>
    /// <returns>Composite phase.</returns>
    public static CompositeGamePhase NewParent(int number, string label = "n/a", IGameStateCondition condition = null, CompositeGamePhase parent = null, IEnumerable<IGameEventPreProcessor> preProcessors = null)
    {
        return new CompositeGamePhase(number, label, condition ?? new NullGameStateCondition(), parent, preProcessors ?? Enumerable.Empty<IGameEventPreProcessor>());
    }
}