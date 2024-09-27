using System;

namespace Veggerby.Boards.Core.Artifacts.Relations;

public abstract class ArtifactRelation<TFrom, TTo>
    where TFrom : Artifact
    where TTo : Artifact
{
    public TFrom From { get; }
    public TTo To { get; }

    public ArtifactRelation(TFrom from, TTo to)
    {
        ArgumentNullException.ThrowIfNull(from);

        ArgumentNullException.ThrowIfNull(to);

        From = from;
        To = to;
    }
}