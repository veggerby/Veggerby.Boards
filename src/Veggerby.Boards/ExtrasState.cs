using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Non-generic extras wrapper turning an arbitrary auxiliary record/class into an <see cref="IArtifactState"/> so it can live inside <see cref="GameState"/>.
/// </summary>
/// <remarks>
/// Replaces previous generic <c>ExtrasState&lt;T&gt;</c> implementation to avoid reflection during initial game compilation and extras retrieval.
/// </remarks>
internal sealed class ExtrasState : IArtifactState
{
    public Artifact Artifact
    {
        get;
    }

    public object Value
    {
        get;
    }

    public Type ExtrasType
    {
        get;
    }

    public ExtrasState(Artifact artifact, object value, Type extrasType)
    {
        ArgumentNullException.ThrowIfNull(artifact, nameof(artifact));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        ArgumentNullException.ThrowIfNull(extrasType, nameof(extrasType));

        Artifact = artifact;
        Value = value;
        ExtrasType = extrasType;
    }

    public override bool Equals(object? obj)
    {
        return obj is ExtrasState other
            && Artifact.Equals(other.Artifact)
            && ExtrasType.Equals(other.ExtrasType)
            && EqualityComparer<object>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Artifact, ExtrasType, Value);
    }
}