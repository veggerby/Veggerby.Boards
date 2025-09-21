using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts;

/// <summary>
/// Artifact composed of multiple child artifacts (e.g. dice sets, composite pieces).
/// </summary>
public class CompositeArtifact<T> : Artifact, IEquatable<CompositeArtifact<T>>
    where T : Artifact
{
    /// <summary>
    /// Gets the child artifacts.
    /// </summary>
    public IEnumerable<T> ChildArtifacts { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeArtifact{T}"/> class.
    /// </summary>
    public CompositeArtifact(string id, IEnumerable<T> childArtifacts) : base(id)
    {
        ArgumentNullException.ThrowIfNull(childArtifacts);

        if (!childArtifacts.Any())
        {
            throw new ArgumentException("Empty child artifacts list", nameof(childArtifacts));
        }

        ChildArtifacts = childArtifacts.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public bool Equals(CompositeArtifact<T> other) => base.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as CompositeArtifact<T>);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(Id);

        foreach (var childArtifact in ChildArtifacts)
        {
            code.Add(childArtifact);
        }

        return code.ToHashCode();
    }
}