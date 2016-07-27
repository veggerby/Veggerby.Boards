using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Phases;

namespace Veggerby.Boards.Core.States
{
    public class GameState : State<Game>
    {
        private readonly IEnumerable<IState> _childStates;
        private readonly IEnumerable<TurnState> _turns;

        public IEnumerable<IState> ChildStates => _childStates.ToList().AsReadOnly();
        public IEnumerable<Round> Rounds => _turns.Select(x => x.Round).Distinct().OrderBy(x => x.Number).ToList();
        public IEnumerable<Turn> Turns => _turns.Select(x => x.Turn).ToList();
        public TurnState ActiveTurn => _turns.LastOrDefault();

        public GameState(Game game, IEnumerable<IState> childStates, IEnumerable<TurnState> turns) : base(game)
        {
            _childStates = (childStates ?? Enumerable.Empty<IState>()).ToList();
            _turns = (turns ?? Enumerable.Empty<TurnState>()).ToList();
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

        public GameState Update(IEnumerable<IState> newStates)
        {
            var states = _childStates
                .Except(newStates, new StateBaseEntityEqualityComparer()) // remove current state of base entities
                .Concat(newStates) // add ned states
                .ToList();

            return new GameState(Artifact, states, _turns);
        }

        public new GameState OnBeforeEvent(IGameEvent @event)
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

        public new GameState OnAfterEvent(IGameEvent @event)
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

        public GameState NextTurn(TurnState turnState)
        {
            var turnStates = new List<TurnState>(_turns);
            turnStates.Add(turnState);
            return new GameState(Artifact, _childStates, turnStates);
        }
    }
}