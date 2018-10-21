using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Events
{
    public class SingleStepMovePieceGameEventPreProcessor : IGameEventPreProcessor
    {
        public SingleStepMovePieceGameEventPreProcessor(IGameEventCondition<MovePieceGameEvent> stepMoveCondition, IEnumerable<Dice> dice)
        {
            if (stepMoveCondition == null)
            {
                throw new ArgumentNullException(nameof(stepMoveCondition));
            }

            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            StepMoveCondition = stepMoveCondition;
            Dice = dice;
        }

        public IGameEventCondition<MovePieceGameEvent> StepMoveCondition { get; }
        public IEnumerable<Dice> Dice { get; }

        public IEnumerable<IGameEvent> ProcessEvent(GameProgress progress, IGameEvent @event)
        {
            if (!(@event is MovePieceGameEvent))
            {
                return new [] { @event };
            }

            var e = (MovePieceGameEvent)@event;

            var pathFinder = new SingleStepPathFinder(StepMoveCondition, Dice.ToArray());
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
}