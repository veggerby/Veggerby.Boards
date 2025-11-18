using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Builder.Extensions;

/// <summary>
/// Helper methods for GameBuilder subclasses to simplify common board construction patterns.
/// </summary>
/// <remarks>
/// These helpers are designed to be called from within a GameBuilder.Build() method
/// where AddTile and AddPiece are accessible.
/// </remarks>
public static class BoardBuilderHelpers
{
    /// <summary>
    /// Generates a sequence of tile/piece identifiers using a factory function.
    /// </summary>
    /// <param name="count">Number of identifiers to generate.</param>
    /// <param name="factory">Function that creates an identifier from an index (0 to count-1).</param>
    /// <returns>An enumerable of identifiers.</returns>
    public static IEnumerable<string> GenerateIds(int count, Func<int, string> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        if (count <= 0)
        {
            throw new ArgumentException("Count must be positive", nameof(count));
        }

        for (int i = 0; i < count; i++)
        {
            yield return factory(i);
        }
    }

    /// <summary>
    /// Generates a 2D grid of tile identifiers using a factory function.
    /// </summary>
    /// <param name="width">Width of the grid (columns).</param>
    /// <param name="height">Height of the grid (rows).</param>
    /// <param name="factory">Function that creates an identifier from (x, y) coordinates.</param>
    /// <returns>An enumerable of (x, y, id) tuples.</returns>
    public static IEnumerable<(int x, int y, string id)> GenerateGridIds(
        int width,
        int height,
        Func<int, int, string> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        if (width <= 0)
        {
            throw new ArgumentException("Width must be positive", nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentException("Height must be positive", nameof(height));
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                yield return (x, y, factory(x, y));
            }
        }
    }

    /// <summary>
    /// Generates a ring of tile identifiers.
    /// </summary>
    /// <param name="count">Number of tiles in the ring.</param>
    /// <param name="factory">Function that creates an identifier from position (0 to count-1).</param>
    /// <returns>An enumerable of (position, id) tuples.</returns>
    public static IEnumerable<(int position, string id)> GenerateRingIds(
        int count,
        Func<int, string> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        if (count <= 0)
        {
            throw new ArgumentException("Count must be positive", nameof(count));
        }

        for (int i = 0; i < count; i++)
        {
            yield return (i, factory(i));
        }
    }

    /// <summary>
    /// Calculates the next position in a ring (wrapping around).
    /// </summary>
    /// <param name="current">Current position.</param>
    /// <param name="ringSize">Total size of the ring.</param>
    /// <returns>Next position (wraps to 0 if at end).</returns>
    public static int NextInRing(int current, int ringSize)
    {
        if (ringSize <= 0)
        {
            throw new ArgumentException("Ring size must be positive", nameof(ringSize));
        }

        return (current + 1) % ringSize;
    }

    /// <summary>
    /// Calculates the previous position in a ring (wrapping around).
    /// </summary>
    /// <param name="current">Current position.</param>
    /// <param name="ringSize">Total size of the ring.</param>
    /// <returns>Previous position (wraps to ringSize-1 if at start).</returns>
    public static int PreviousInRing(int current, int ringSize)
    {
        if (ringSize <= 0)
        {
            throw new ArgumentException("Ring size must be positive", nameof(ringSize));
        }

        return (current - 1 + ringSize) % ringSize;
    }
}
