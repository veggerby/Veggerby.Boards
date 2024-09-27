using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Builder.Artifacts
{
    public class ArtifactDefinition : DefinitionBase
    {
        public ArtifactDefinition(GameBuilder builder) : base(builder)
        {
        }

        public string ArtifactId { get; private set; }

        public Func<string, Artifact> Factory { get; private set; }

        public ArtifactDefinition WithId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(id));
            }

            ArtifactId = id;
            return this;
        }

        public ArtifactDefinition OfType<T>() where T : Artifact, new()
        {
            Factory = id => new T();
            return this;
        }

        public ArtifactDefinition WithFactory<T>(Func<string, T> factory) where T : Artifact
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            return this;
        }
    }
}
