using System;

namespace Veggerby.Boards.Core.States;

public class ArtifactStateChange
{
    public IArtifactState From { get; }
    public IArtifactState To { get; }

    public ArtifactStateChange(IArtifactState from, IArtifactState to)
    {
        if (from is null && to is null)
        {
            throw new ArgumentNullException(nameof(to), "Both From and To cannot be null");
        }

        if (from is not null && to is not null && !from.Artifact.Equals(to.Artifact))
        {
            throw new ArgumentException("To and From need to reference the same artifact", nameof(to));
        }

        From = from;
        To = to;
    }
}