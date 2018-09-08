using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Phases;

namespace Veggerby.Boards.Core.States
{
    public class GameState
    {
        private readonly IDictionary<Artifact, IArtifactState> _childStates;
        private readonly GameState _previousState;

        public Game Game { get; }
        public IEnumerable<IArtifactState> ChildStates => _childStates.Values.ToList().AsReadOnly();
        public bool IsInitialState => _previousState == null;

        private GameState(Game game, IEnumerable<IArtifactState> childStates = null, GameState previousState = null)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            Game = game;
            _childStates = (childStates ?? Enumerable.Empty<IArtifactState>()).ToDictionary(x => x.Artifact, x => x);
            _previousState = previousState;
        }

        public T GetState<T>(Artifact artifact) where T : IArtifactState
        {
            return _childStates.ContainsKey(artifact) && _childStates[artifact] is T ? (T)_childStates[artifact] : default(T);
        }

        public IEnumerable<T> GetStates<T>() where T : IArtifactState
        {
            return _childStates.Values.OfType<T>().ToList();
        }

        protected bool Equals(GameState other)
        {
            if (!Game.Equals(other.Game) || IsInitialState != other.IsInitialState)
            {
                return false;
            }

            if (ChildStates.Count() != other.ChildStates.Count())
            {
                return false;
            }

            return !ChildStates.Except(other.ChildStates).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.GetType().GetHashCode();
                hashCode = (hashCode*397) ^ Game.GetHashCode();
                hashCode = (hashCode*397) ^ IsInitialState.GetHashCode();
                return ChildStates.Aggregate(hashCode, (seed, state) => seed ^ state.GetHashCode());
            }
        }

        public GameState Next(IEnumerable<IArtifactState> newStates)
        {
            return new GameState(
                Game,
                ChildStates
                    .Except(newStates ?? Enumerable.Empty<IArtifactState>(), new ArtifactStateEqualityComparer())
                    .Concat(newStates ?? Enumerable.Empty<IArtifactState>()),
                this);
        }

        public IEnumerable<ArtifactStateChange> CompareTo(GameState state)
        {
            if (!Game.Equals(state.Game))
            {
                throw new ArgumentException("Must compare GameState for the same game", nameof(state));
            }

            if (Equals(state))
            {
                return Enumerable.Empty<ArtifactStateChange>();
            }

            var additions = ChildStates
                .Except(state.ChildStates, new ArtifactStateEqualityComparer())
                .Select(to => new ArtifactStateChange(null, to));

            // states cannot be removed
            /*var removals = state.ChildStates
                .Except(ChildStates, new ArtifactStateEqualityComparer())
                .Select(from => new ArtifactStateChange(from, null));*/

            var changes = state.ChildStates
                .Join(ChildStates, x => x.Artifact, x => x.Artifact, (from, to) => new ArtifactStateChange(from, to))
                .Where(x => !x.From.Equals(x.To));

            return changes.Concat(additions).ToList();
        }

        public static GameState New(Game game, IEnumerable<IArtifactState> initialStates)
        {
            return new GameState(game, initialStates, null);
        }
    }
}