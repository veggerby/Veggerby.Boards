using System;

namespace Veggerby.Boards.Random;

/// <summary>
/// Deterministic random source with explicit seed/state snapshot support.
/// </summary>
public interface IRandomSource
{
    /// <summary>
    /// Gets the 64-bit seed used to initialize the generator (for replay/reference).
    /// </summary>
    ulong Seed
    {
        get;
    }

    /// <summary>
    /// Fills a span with pseudo-random bytes deterministically.
    /// </summary>
    void NextBytes(Span<byte> buffer);

    /// <summary>
    /// Produces the next 32-bit unsigned integer value.
    /// </summary>
    uint NextUInt();

    /// <summary>
    /// Clones the generator producing a new instance at identical state (pure snapshot).
    /// </summary>
    IRandomSource Clone();
}