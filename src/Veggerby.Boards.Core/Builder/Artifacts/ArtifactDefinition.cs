using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Builder.Artifacts;

/// <summary>
/// Fluent definition for a generic artifact allowing custom factory configuration.
/// </summary>
public class ArtifactDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    /// <summary>
    /// Gets the configured artifact identifier.
    /// </summary>
    public string ArtifactId { get; private set; }

    /// <summary>
    /// Gets the factory used to create the runtime artifact instance.
    /// </summary>
    public Func<string, Artifact> Factory { get; private set; }

    /// <summary>
    /// Sets the artifact identifier.
    /// </summary>
    /// <param name="id">Unique artifact identifier.</param>
    /// <returns>The same definition for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public ArtifactDefinition WithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        ArtifactId = id;
        return this;
    }

    /// <summary>
    /// Uses a parameterless constructor to create the artifact.
    /// </summary>
    /// <typeparam name="T">Concrete artifact type.</typeparam>
    /// <returns>The same definition for chaining.</returns>
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
    public ArtifactDefinition WithFactory<T>(Func<string, T> factory) where T : Artifact
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }
}