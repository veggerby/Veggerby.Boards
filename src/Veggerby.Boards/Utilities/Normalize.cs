using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Veggerby.Boards.Utilities;

/// <summary>
/// Normalization helpers converting nullable external inputs into internal non-null canonical representations.
/// </summary>
/// <remarks>
/// Policy: collections and strings stored internally are never null; absence is expressed via empties.
/// </remarks>
public static class Normalize
{
    /// <summary>
    /// Normalizes a possibly null string to a non-null value (empty when source is null).
    /// </summary>
    /// <param name="value">Source string.</param>
    /// <returns>Original string or <see cref="string.Empty"/> when null.</returns>
    public static string Text(string? value) => value ?? string.Empty;

    /// <summary>
    /// Normalizes a possibly null enumerable to a non-null read-only list (empty when source is null).
    /// Materializes lazily into an array only when enumeration is required.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Source enumerable.</param>
    /// <returns>Non-null read-only list (empty when source is null).</returns>
    public static IReadOnlyList<T> List<T>(IEnumerable<T>? source)
    {
        if (source is null)
        {
            return Array.Empty<T>();
        }

        return source as IReadOnlyList<T> ?? source.ToArray();
    }

    /// <summary>
    /// Ensures a reference is non-null throwing <see cref="ArgumentNullException"/> otherwise.
    /// </summary>
    /// <typeparam name="T">Reference type.</typeparam>
    /// <param name="value">Value to validate.</param>
    /// <param name="paramName">Parameter name for exception context.</param>
    /// <returns>The non-null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static T Ref<T>([NotNull] T? value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }
}