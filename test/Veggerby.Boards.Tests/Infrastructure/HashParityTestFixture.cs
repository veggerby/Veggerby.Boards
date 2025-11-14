using System;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Infrastructure;

/// <summary>
/// Base class for hash parity tests that validate deterministic state hashing across execution paths.
/// </summary>
/// <remarks>
/// Provides helper methods to assert hash equality between reference and candidate game progressions,
/// ensuring that different acceleration paths (compiled patterns, bitboards, etc.) produce identical
/// state hashes when processing the same event sequences.
/// </remarks>
public abstract class HashParityTestFixture
{

    /// <summary>
    /// Asserts that two <see cref="GameProgress"/> instances have identical state hashes (both 64-bit and 128-bit).
    /// </summary>
    /// <param name="reference">The reference game progress (baseline).</param>
    /// <param name="candidate">The candidate game progress (under test).</param>
    /// <param name="context">Optional context string for error messages.</param>
    protected static void AssertHashParity(
        GameProgress reference,
        GameProgress candidate,
        string? context = null)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(candidate);

        var refHash = reference.State.Hash;
        var candHash = candidate.State.Hash;

        var message = context is null ? "Hash mismatch" : $"Hash mismatch: {context}";

        refHash.Should().Be(candHash, message);

        // Also verify 128-bit hash for additional collision resistance
        var refHash128 = reference.State.Hash128;
        var candHash128 = candidate.State.Hash128;

        refHash128.Should().Be(candHash128, message + " (128-bit)");
    }

    /// <summary>
    /// Asserts that two <see cref="GameState"/> instances have identical hashes (both 64-bit and 128-bit).
    /// </summary>
    /// <param name="reference">The reference game state (baseline).</param>
    /// <param name="candidate">The candidate game state (under test).</param>
    /// <param name="context">Optional context string for error messages.</param>
    protected static void AssertHashParity(
        GameState reference,
        GameState candidate,
        string? context = null)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(candidate);

        var refHash = reference.Hash;
        var candHash = candidate.Hash;

        var message = context is null ? "Hash mismatch" : $"Hash mismatch: {context}";

        refHash.Should().Be(candHash, message);

        // Also verify 128-bit hash for additional collision resistance
        var refHash128 = reference.Hash128;
        var candHash128 = candidate.Hash128;

        refHash128.Should().Be(candHash128, message + " (128-bit)");
    }
}
