using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Phases;

namespace Veggerby.Boards.Core.States
{
    public class GameState
    {
        private readonly IEnumerable<IArtifactState> _childStates;
        private readonly GameState _previousState;

        public Game Game { get; }
        public IEnumerable<IArtifactState> ChildStates => _childStates.ToList().AsReadOnly();
        public bool IsInitialState => _previousState == null;

        private GameState(Game game, IEnumerable<IArtifactState> childStates = null, GameState previousState = null)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            Game = game;
            _childStates = (childStates ?? Enumerable.Empty<IArtifactState>()).ToList();
            _previousState = previousState;
        }

        public T GetState<T>(Artifact artifact) where T : IArtifactState
        {
            return _childStates
                .OfType<T>()
                .SingleOrDefault(x => x.Artifact.Equals(artifact));
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
                return ChildStates.Aggregate(hashCode, (seed, state) => (397 * seed) ^ state.GetHashCode());
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

        public static GameState New(Game game, IEnumerable<IArtifactState> initialStates)
        {
            return new GameState(game, initialStates, null);
        }
    }
}