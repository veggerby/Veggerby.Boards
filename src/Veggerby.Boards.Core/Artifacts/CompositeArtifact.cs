using System.Collections.Generic;

namespace Veggerby.Boards.Core.Artifacts 
{
    public class CompositeArtifact<T> : Artifact
        where T : Artifact
    {
        public IEnumerable<T> ChildArtifacts { get; }

        public CompositeArtifact(string id, IEnumerable<T> childArtifacts) : base(id)
        {
            ChildArtifacts = childArtifacts;
        }
    }
}