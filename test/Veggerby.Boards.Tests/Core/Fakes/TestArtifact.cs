using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Tests.Core.Fakes;

public class TestArtifact(string id) : Artifact(id), IEquatable<TestArtifact>
{
    public bool Equals(TestArtifact other) => base.Equals(other);

    public override bool Equals(object obj)
    {
        return Equals(obj as TestArtifact);
    }

    public override int GetHashCode() => base.GetHashCode();
}