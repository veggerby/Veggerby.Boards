using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class TestArtifact : Artifact, IEquatable<TestArtifact>
    {
        public TestArtifact(string id) : base(id)
        {
        }

        public bool Equals(TestArtifact other) => base.Equals(other);
    }
}