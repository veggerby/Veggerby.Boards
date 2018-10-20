using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows
{
    public class SingleStepPathFinder
    {
        public SingleStepPathFinder(IGameEventCondition<MovePieceGameEvent> stepMoveCondition, params Dice[] dice)
        {
            if (stepMoveCondition == null)
            {
                throw new ArgumentNullException(nameof(stepMoveCondition));
            }

            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            if (!dice.Any())
            {
                throw new ArgumentException(nameof(dice));
            }

            StepMoveCondition = stepMoveCondition;
            Dice = dice;
        }

        public IGameEventCondition<MovePieceGameEvent> StepMoveCondition { get; }
        public Dice[] Dice { get; }

        private SingleStepPath GetSingleStep(GameEngine engine, PieceState pieceState, IPattern pattern, GameState state, DiceState<int> diceState, Tile to, SingleStepPath previousStep = null)
        {
            var visitor = new ResolveTilePathDistanceVisitor(engine.Game.Board, pieceState.CurrentTile, to, diceState.CurrentValue, false);
            pattern.Accept(visitor);

            if (visitor.ResultPath == null)
            {
                return null;
            }

            var step = StepMoveCondition.Evaluate(engine, state, new MovePieceGameEvent(pieceState.Artifact, visitor.ResultPath));

            if (step.Result != ConditionResult.Valid)
            {
                return null;
            }

            var newState = state.Next(new IArtifactState[] {
                new PieceState(pieceState.Artifact, visitor.ResultPath.To),
                new NullDiceState(diceState.Artifact)
            });

            return new SingleStepPath(newState, diceState, visitor.ResultPath, previousStep);
        }

        private IEnumerable<SingleStepPath> FindSingleSteps(GameEngine engine, Piece piece, GameState state, Tile to, SingleStepPath previousStep)
        {
            var pieceState = state.GetState<PieceState>(piece);

            if (pieceState == null)
            {
                return Enumerable.Empty<SingleStepPath>();
            }

            var diceStates = Dice.Select(x => state.GetState<DiceState<int>>(x)).Where(x => x != null);

            if (!diceStates.Any())
            {
                return Enumerable.Empty<SingleStepPath>();
            }

            return piece
                .Patterns
                .SelectMany(pattern => diceStates.Select(diceState => GetSingleStep(engine, pieceState, pattern, state, diceState, to, previousStep)))
                .Where(x => x != null)
                .ToList();
        }

        private IEnumerable<SingleStepPath> ContinueStep(GameEngine engine, Piece piece, Tile to, SingleStepPath step)
        {
            return FindSingleSteps(engine, piece, step.NewState, to, step);
        }

        public IEnumerable<SingleStepPath> GetPaths(GameEngine engine, GameState state, Piece piece, Tile from, Tile to)
        {
            var pieceState = state.GetState<PieceState>(piece);

            if (!pieceState.CurrentTile.Equals(from))
            {
                return Enumerable.Empty<SingleStepPath>();
            }

            var steps = FindSingleSteps(engine, piece, state, to, null);
            while (steps.Any() && !steps.Any(x => x.Path.To.Equals(to)))
            {
                steps = steps.SelectMany(step => ContinueStep(engine, piece, to, step)).ToList();
            }

            return steps.Where(x => x.Path.To.Equals(to)).ToList();
        }
    }
}