using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Builder.Artifacts;

/// <summary>
/// Fluent definition for a generic artifact allowing custom factory configuration.
/// </summary>
public class ArtifactDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    /// <summary>
    /// Gets the configured artifact identifier.
    /// </summary>
    public string ArtifactId { get; private set; } = null!; // LIFECYCLE: set by WithId() before Build()

    /// <summary>
    /// Gets the factory used to create the runtime artifact instance.
    /// </summary>
    public Func<string, Artifact> Factory { get; private set; } = null!; // LIFECYCLE: set by OfType() or WithFactory() before Build()

    /// <summary>
    /// Sets the artifact identifier.
    /// </summary>
    /// <param name="id">Unique artifact identifier.</param>
    /// <returns>The same definition for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(ArtifactId))]
    public ArtifactDefinition WithId(string id)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        ArtifactId = id;
        return this;
    }

    /// <summary>
    /// Uses a parameterless constructor to create the artifact.
    /// </summary>
    /// <typeparam name="T">Concrete artifact type.</typeparam>
    /// <returns>The same definition for chaining.</returns>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(Factory))]
    public ArtifactDefinition OfType<T>() where T : Artifact, new()
    {
        Factory = id => new T();
        return this;
    }

    /// <summary>
    /// Provides a custom factory for artifact instantiation.
    /// </summary>
    /// <typeparam name="T">Concrete artifact type.</typeparam>
    /// <param name="factory">Factory function producing the artifact instance.</param>
    /// <returns>The same definition for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(Factory))]
    public ArtifactDefinition WithFactory<T>(Func<string, T> factory) where T : Artifact
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        Factory = factory;
        return this;
    }
}