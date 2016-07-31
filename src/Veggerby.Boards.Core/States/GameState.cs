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

        private GameState(Game game, IEnumerable<IState> childStates)
        {
            Game = game;
            _childStates = (childStates ?? Enumerable.Empty<IState>()).ToList();
        }

        public IEnumerable<State<T>> GetStates<T>() where T : Artifact
        {
            return _childStates
                .OfType<State<T>>()
                .ToList();
        }

        public State<T> GetState<T>(T artifact) where T : Artifact
        {
            return GetState(artifact as Artifact) as State<T>;
        }

        public IState GetState(Artifact artifact)
        {
            return _childStates
                .OfType<IArtifactState>()
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

        public GameState Update(IEnumerable<IState> newStates)
        {
            var states = _childStates
                .Except(newStates, new StateBaseEntityEqualityComparer()) // remove current state of base entities
                .Concat(newStates) // add ned states
                .ToList();

            return new GameState(Game, states);
        }

        public GameState Remove(Artifact artifact)
        {
            var states = _childStates
                .Except(GetState(artifact)) // remove current state of base entities
                .ToList();

            return new GameState(Game, states);
        }

        public GameState OnBeforeEvent(IGameEvent @event)
        {
            var states = _childStates
                .Select(x => x.OnBeforeEvent(@event))
                .Where(x => x != null)
                .ToList();

            if (!states.Any())
            {
                return this;
            }
            
            return Update(states);
        }

        public GameState OnAfterEvent(IGameEvent @event)
        {
             var states = _childStates
                .Select(x => x.OnAfterEvent(@event))
                .Where(x => x != null)
                .ToList();
            
            if (!states.Any())
            {
                return this;
            }
            
            return Update(states);
        }

        public static GameState New(Game game, IEnumerable<IState> childStates = null)
        {
            return new GameState(game, childStates);
        }
    }
}