using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Generic wrapper turning an arbitrary extras record into an <see cref="IArtifactState"/> so it can live inside <see cref="GameState"/>.
/// </summary>
/// <typeparam name="T">Extras type.</typeparam>
internal sealed class ExtrasState<T> : IArtifactState
{
    public ExtrasState(Artifact artifact, T value)
    {
        Artifact = artifact;
        Value = value;
    }

    public Artifact Artifact { get; }

    public T Value { get; }

    public override bool Equals(object obj)
    {
        return obj is ExtrasState<T> other && Artifact.Equals(other.Artifact) && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Artifact, Value);
    }
}