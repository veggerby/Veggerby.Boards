using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;

namespace Veggerby.Boards.Core.States
{
    public class GameState
    {
        private readonly IEnumerable<IState> _childStates;

        public Game Game { get; }
        public IEnumerable<IState> ChildStates => _childStates.ToList().AsReadOnly();

        private GameState(Game game, IEnumerable<IState> childStates = null)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            Game = game;
            _childStates = (childStates ?? Enumerable.Empty<IState>()).ToList();
        }

        public IEnumerable<State<T>> GetStates<T>() where T : Artifact
        {
            return _childStates
                .OfType<State<T>>()
                .ToList();
        }

        public TState GetState<T, TState>(T artifact) where T : Artifact where TState : State<T>
        {
            return GetState<TState>(artifact as Artifact) as TState;
        }

        public TState GetState<TState>(Artifact artifact) where TState : IArtifactState
        {
            return _childStates
                .OfType<TState>()
                .SingleOrDefault(x => x.Artifact.Equals(artifact));
        }

        public TurnState GetActiveTurn()
        {
            return _childStates
                .OfType<TurnState>()
                .OrderBy(x => x.Round.Number)
                .ThenBy(x => x.Turn.Number)
                .LastOrDefault();
        }

        public GameState Update<TState>(TState state) where TState : IArtifactState
        {
            var states = _childStates
                .Where(x => !((x as IArtifactState)?.Artifact.Equals(state.Artifact) ?? false)) // remove current state of base entities
                .Append(state) // add ned states
                .ToList();

            return new GameState(Game, states);
        }

        public GameState Update(IState state)
        {
            if (state is IArtifactState)
            {
                return Update(state as IArtifactState);
            }

            return new GameState(Game, _childStates.Append(state));
        }

        public GameState Remove<TState>(Artifact artifact) where TState : IArtifactState
        {
            var states = _childStates
                .Except(GetState<TState>(artifact)) // remove current state of base entities
                .ToList();

            return new GameState(Game, states);
        }

        public GameState OnBeforeEvent(IGameEvent @event)
        {
            var state = this;

            foreach (var childState in _childStates)
            {
                var newState = childState.OnBeforeEvent(@event);
                if (newState != null)
                {
                    state = state.Update(newState);
                }
            }

            return state;
        }

        public GameState OnAfterEvent(IGameEvent @event)
        {
            var state = this;

            foreach (var childState in _childStates)
            {
                var newState = childState.OnAfterEvent(@event);
                if (newState != null)
                {
                    state = state.Update(newState);
                }
            }

            return state;
        }

        public static GameState New(Game game, IEnumerable<IState> childStates = null)
        {
            return new GameState(game, childStates);
        }
    }
}