using System;
using System.Diagnostics.CodeAnalysis;

namespace Veggerby.Boards.Core;

/// <summary>
/// Exception thrown when a required property or configuration element is missing during <see cref="GameBuilder"/> compilation.
/// </summary>
/// <remarks>
/// Used to surface validation failures for incomplete game definitions (e.g. missing board id before calling <c>Compile</c>).
/// </remarks>
[ExcludeFromCodeCoverage]
public class GameEngineBuilderException(string property, string message) : Exception($"{property}: {message}")
{
    /// <summary>
    /// Initializes a new instance indicating the specified property has no value.
    /// </summary>
    /// <param name="property">The missing property name.</param>
    public GameEngineBuilderException(string property) : this(property, "Missing value") { }
}