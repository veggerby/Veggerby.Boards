using System;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Base class for relations connecting two artifacts (typically tiles) with direction or additional semantics supplied by subclasses.
/// </summary>
/// <typeparam name="TFrom">Origin artifact type.</typeparam>
/// <typeparam name="TTo">Destination artifact type.</typeparam>
public abstract class ArtifactRelation<TFrom, TTo>
    where TFrom : Artifact
    where TTo : Artifact
{
    /// <summary>
    /// Gets the origin artifact.
    /// </summary>
    public TFrom From
    {
        get;
    }
    /// <summary>
    /// Gets the destination artifact.
    /// </summary>
    public TTo To
    {
        get;
    }

    /// <summary>
    /// Initializes a new relation instance.
    /// </summary>
    /// <param name="from">Origin artifact.</param>
    /// <param name="to">Destination artifact.</param>
    public ArtifactRelation(TFrom from, TTo to)
    {
        ArgumentNullException.ThrowIfNull(from);

        ArgumentNullException.ThrowIfNull(to);

        From = from;
        To = to;
    }
}