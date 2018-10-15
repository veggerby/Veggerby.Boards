using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class HasValidPathWithDiceStateGameEventCondition : IGameEventCondition<MovePieceGameEvent>
    {
        public HasValidPathWithDiceStateGameEventCondition(IGameEventCondition<MovePieceGameEvent> stepMoveCondition, params Dice[] dice)
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

        private class SingleStepPath
        {
            public SingleStepPath(GameState state, DiceState<int> diceState, TilePath path, SingleStepPath previous = null)
            {
                if (state == null)
                {
                    throw new ArgumentNullException(nameof(state));
                }

                if (diceState == null)
                {
                    throw new ArgumentNullException(nameof(diceState));
                }

                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                NewState = state;
                DiceState = diceState;
                Path = path;
                Previous = previous;
            }

            public GameState NewState { get; }
            public DiceState<int> DiceState { get; }
            public TilePath Path { get; }
            public SingleStepPath Previous { get; }
        }

        private SingleStepPath GetSingleStep(GameEngine engine, PieceState pieceState, IPattern pattern, GameState state, DiceState<int> diceState, Tile to, SingleStepPath previousStep = null)
        {
            var visitor = new ResolveTilePathDistanceVisitor(engine.Game.Board, pieceState.CurrentTile, to, diceState.CurrentValue, false);
            pattern.Accept(visitor);

            if (visitor.ResultPath == null)
            {
                return null;
            }

            var step = StepMoveCondition.Evaluate(engine, state, new MovePieceGameEvent(pieceState.Artifact, pieceState.CurrentTile, visitor.ResultPath.To));

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

        public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
        {
            var pieceState = state.GetState<PieceState>(@event.Piece);

            if (!pieceState.CurrentTile.Equals(@event.From))
            {
                return ConditionResponse.Invalid;
            }

            var steps = FindSingleSteps(engine, @event.Piece, state, @event.To, null);
            while (steps.Any() && !steps.Any(x => x.Path.To.Equals(@event.To)))
            {
                steps = steps.SelectMany(step => ContinueStep(engine, @event.Piece, @event.To, step)).ToList();
            }

            return steps.Any(x => x.Path.To.Equals(@event.To))
                ? ConditionResponse.Valid
                : ConditionResponse.Fail("No valid steps found to destination tile");
        }
    }
}