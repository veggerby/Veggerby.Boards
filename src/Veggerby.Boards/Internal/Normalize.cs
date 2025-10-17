using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Utility class for normalizing potentially null values at API boundaries.
/// </summary>
internal static class Normalize
{
    /// <summary>
    /// Converts a potentially null string to a non-null string (empty string for null).
    /// </summary>
    /// <param name="s">The string to normalize.</param>
    /// <returns>The original string if non-null, otherwise <see cref="string.Empty"/>.</returns>
    public static string Text(string? s) => s ?? string.Empty;

    /// <summary>
    /// Converts a potentially null enumerable to a non-null read-only list.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="xs">The enumerable to normalize.</param>
    /// <returns>A read-only list containing the elements, or an empty array if null.</returns>
    public static IReadOnlyList<T> List<T>(IEnumerable<T>? xs) => xs?.ToArray() ?? Array.Empty<T>();
}
