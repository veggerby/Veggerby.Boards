using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public abstract class ArtifactState : IArtifactState
    {
        public Artifact Artifact { get; }

        public ArtifactState(Artifact artifact)
        {
            if (artifact == null)
            {
                throw new ArgumentNullException(nameof(artifact));
            }

            Artifact = artifact;
        }
    }
}