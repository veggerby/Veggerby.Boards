using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts
{
    public class CompositeArtifact<T> : Artifact
        where T : Artifact
    {
        public IEnumerable<T> ChildArtifacts { get; }

        public CompositeArtifact(string id, IEnumerable<T> childArtifacts) : base(id)
        {
            if (childArtifacts == null)
            {
                throw new ArgumentNullException(nameof(childArtifacts));
            }

            if (!childArtifacts.Any())
            {
                throw new ArgumentException("Empty child artifacts list", nameof(childArtifacts));
            }

            ChildArtifacts = childArtifacts;
        }
    }
}