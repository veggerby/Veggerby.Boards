using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Events;

/// <summary>
/// Expands a multi-distance move event into a sequence of single-step move events, validating each via a step condition.
/// </summary>
public class SingleStepMovePieceGameEventPreProcessor : IGameEventPreProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleStepMovePieceGameEventPreProcessor"/> class.
    /// </summary>
    /// <param name="stepMoveCondition">Condition applied per single-step.</param>
    /// <param name="dice">Dice whose values drive step distances.</param>
    public SingleStepMovePieceGameEventPreProcessor(IGameEventCondition<MovePieceGameEvent> stepMoveCondition, IEnumerable<Dice> dice)
    {
        ArgumentNullException.ThrowIfNull(stepMoveCondition);

        ArgumentNullException.ThrowIfNull(dice);

        StepMoveCondition = stepMoveCondition;
        Dice = dice;
    }

    /// <summary>
    /// Gets the per-step validation condition.
    /// </summary>
    public IGameEventCondition<MovePieceGameEvent> StepMoveCondition { get; }
    /// <summary>
    /// Gets the dice considered for step construction.
    /// </summary>
    public IEnumerable<Dice> Dice { get; }

    /// <inheritdoc />
    public IEnumerable<IGameEvent> ProcessEvent(GameProgress progress, IGameEvent @event)
    {
        if (!(@event is MovePieceGameEvent))
        {
            return [@event];
        }

        var e = (MovePieceGameEvent)@event;

        var pathFinder = new SingleStepPathFinder(StepMoveCondition, [.. Dice]);
        var paths = pathFinder.GetPaths(progress.Engine, progress.State, e.Piece, e.From, e.To);

        if (!paths.Any())
        {
            return Enumerable.Empty<IGameEvent>();
        }

        return paths
            .First()
            .Paths
            .Select(path => new MovePieceGameEvent(e.Piece, path))
            .ToList();
    }
}