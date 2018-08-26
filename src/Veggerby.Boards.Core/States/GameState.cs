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

        public IArtifactState GetState(Artifact artifact)
        {
            return _childStates
                .SingleOrDefault(x => x.Artifact.Equals(artifact));
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