using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Random;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable snapshot of all artifact states at a point in game execution.
/// </summary>
/// <remarks>
/// <see cref="GameState"/> instances form a singly linked chain (via the private previous reference) enabling
/// time-travel style inspection or diffing. Mutations are represented by producing a successor instance through
/// <see cref="Next"/>.
/// </remarks>
public class GameState
{
    private readonly IDictionary<Artifact, IArtifactState> _childStates;
    private readonly GameState? _previousState;

    /// <summary>
    /// Gets the collection of artifact states contained in this snapshot.
    /// </summary>
    public IEnumerable<IArtifactState> ChildStates => _childStates.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets the deterministic state hash (when hashing feature flag enabled); otherwise <c>null</c>.
    /// </summary>
    public ulong? Hash
    {
        get;
    }

    /// <summary>
    /// Gets the 128-bit state hash (when hashing feature flag enabled); otherwise <c>null</c>.
    /// </summary>
    public (ulong Low, ulong High)? Hash128
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether this is the initial state (no prior state).
    /// </summary>
    public bool IsInitialState => _previousState is null;

    private GameState(IEnumerable<IArtifactState>? childStates = null, GameState? previousState = null, IRandomSource? random = null)
    {
        _childStates = (childStates ?? Enumerable.Empty<IArtifactState>()).ToDictionary(x => x.Artifact, x => x);
        _previousState = previousState;
        Random = random;

        // Always compute hashes (graduated feature)
        Hash = ComputeHash();
        Hash128 = ComputeHash128();
    }

    /// <summary>
    /// Retrieves the state of a specific artifact if present.
    /// </summary>
    /// <typeparam name="T">The artifact state type expected.</typeparam>
    /// <param name="artifact">The artifact.</param>
    /// <returns>The state instance or default when absent or different type.</returns>
    public T? GetState<T>(Artifact artifact) where T : class, IArtifactState
    {
        return _childStates.TryGetValue(artifact, out var state) && state is T t ? t : null;
    }

    /// <summary>
    /// Gets all artifact states of the specified type.
    /// </summary>
    /// <typeparam name="T">The artifact state type.</typeparam>
    /// <returns>Enumeration of states.</returns>
    public IEnumerable<T> GetStates<T>() where T : IArtifactState
    {
        return [.. _childStates.Values.OfType<T>()];
    }

    /// <summary>
    /// Structural equality comparison with another state.
    /// </summary>
    /// <param name="other">The other state.</param>
    /// <returns><c>true</c> when equal; otherwise <c>false</c>.</returns>
    public bool Equals(GameState? other)
    {
        if (other is null)
        {
            return false;
        }

        if (IsInitialState != other.IsInitialState)
        {
            return false;
        }

        // Use Count property directly on the dictionary for O(1) access instead of Count() LINQ extension
        if (_childStates.Count != other._childStates.Count)
        {
            return false;
        }

        return !ChildStates.Except(other.ChildStates).Any();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((GameState)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(GetType());
        code.Add(IsInitialState);
        foreach (var state in ChildStates)
        {
            code.Add(state);
        }

        return code.ToHashCode();
    }

    /// <summary>
    /// Produces a successor state with the specified artifact states replacing any existing entries.
    /// </summary>
    /// <param name="newStates">States to merge.</param>
    /// <returns>The new state.</returns>
    public GameState Next(IEnumerable<IArtifactState>? newStates)
    {
        return new GameState(
            ChildStates
                .Except(newStates ?? Enumerable.Empty<IArtifactState>(), new ArtifactStateEqualityComparer())
                .Concat(newStates ?? Enumerable.Empty<IArtifactState>()),
            this,
            Random?.Clone());
    }

    /// <summary>
    /// Computes artifact state changes relative to an earlier state.
    /// </summary>
    /// <param name="state">The earlier state to compare against.</param>
    /// <returns>Enumeration of changes (additions and modifications).</returns>
    public IEnumerable<ArtifactStateChange> CompareTo(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (Equals(state))
        {
            return Enumerable.Empty<ArtifactStateChange>();
        }

        var additions = ChildStates
            .Except(state.ChildStates, new ArtifactStateEqualityComparer())
            .Select(to => new ArtifactStateChange(null, to));

        var changes = state.ChildStates
            .Join(ChildStates, x => x.Artifact, x => x.Artifact, (from, to) => new ArtifactStateChange(from, to))
            .Where(x => x.From is not null && !x.From.Equals(x.To));

        return [.. changes, .. additions];
    }

    /// <summary>
    /// Creates a new initial <see cref="GameState"/>.
    /// </summary>
    /// <param name="initialStates">The initial artifact states.</param>
    /// <param name="random">Optional deterministic random source snapshot.</param>
    /// <returns>The new initial game state.</returns>
    public static GameState New(IEnumerable<IArtifactState> initialStates, IRandomSource? random = null)
    {
        return new GameState(initialStates, null, random);
    }

    /// <summary>
    /// Creates a new <see cref="GameState"/> with the provided random source replacing the existing one (if any).
    /// </summary>
    public GameState WithRandom(IRandomSource random)
    {
        return new GameState(ChildStates, _previousState, random);
    }

    /// <summary>
    /// Gets the random source snapshot associated with this state (may be null if none assigned).
    /// </summary>
    public IRandomSource? Random
    {
        get;
    }

    /// <summary>
    /// Computes a 64-bit FNV-1a style hash over artifact states (id + type + serialized state) and RNG snapshot.
    /// Stable ordering: artifact id ascending.
    /// </summary>
    private ulong ComputeHash()
    {
        const ulong offset = 1469598103934665603UL; // FNV offset basis
        ulong h = offset;

        // order by artifact id for canonical ordering; use canonical serializer for deterministic representation
        foreach (var kvp in _childStates.OrderBy(x => x.Key.Id, StringComparer.Ordinal))
        {
            h = HashBytes(h, (ReadOnlySpan<byte>)System.Text.Encoding.UTF8.GetBytes(kvp.Key.Id));
            var keyTypeName = kvp.Key.GetType().FullName ?? kvp.Key.GetType().Name;
            h = HashBytes(h, (ReadOnlySpan<byte>)System.Text.Encoding.UTF8.GetBytes(keyTypeName));
            var state = kvp.Value;
            var stateTypeName = state.GetType().FullName ?? state.GetType().Name;
            h = HashBytes(h, (ReadOnlySpan<byte>)System.Text.Encoding.UTF8.GetBytes(stateTypeName));
            var writer = new Internal.IncrementalHashWriter(h);
            Internal.CanonicalStateSerializer.WriteObject(state, ref writer);
            h = writer.Hash;
        }

        if (Random is not null)
        {
            // Serialize RNG seed plus a small deterministic peek (does not mutate original due to clone).
            var clone = Random.Clone();
            Span<byte> seedBytes = stackalloc byte[8];
            BitConverter.TryWriteBytes(seedBytes, clone.Seed);
            h = HashBytes(h, seedBytes);
            // Peek 2 uints as additional state fingerprint (8 bytes)
            Span<byte> peek = stackalloc byte[8];
            var u1 = clone.NextUInt();
            var u2 = clone.NextUInt();
            BitConverter.TryWriteBytes(peek[..4], u1);
            BitConverter.TryWriteBytes(peek[4..], u2);
            h = HashBytes(h, peek);
        }

        return h;
    }

    /// <summary>
    /// Computes a 128-bit xxHash of the canonical serialized state (artifacts + RNG fingerprint).
    /// </summary>
    private (ulong Low, ulong High) ComputeHash128()
    {
        // Accumulate canonical bytes into a pooled buffer (bounded by typical small state footprint).
        var buffer = new System.Buffers.ArrayBufferWriter<byte>(1024);
        void Write(ReadOnlySpan<byte> span)
        {
            var dest = buffer.GetSpan(span.Length);
            span.CopyTo(dest);
            buffer.Advance(span.Length);
        }

        foreach (var kvp in _childStates.OrderBy(x => x.Key.Id, StringComparer.Ordinal))
        {
            var idBytes = System.Text.Encoding.UTF8.GetBytes(kvp.Key.Id);
            Write(idBytes);
            var atBytes = System.Text.Encoding.UTF8.GetBytes(kvp.Key.GetType().FullName ?? kvp.Key.GetType().Name);
            Write(atBytes);
            var state = kvp.Value;
            var stBytes = System.Text.Encoding.UTF8.GetBytes(state.GetType().FullName ?? state.GetType().Name);
            Write(stBytes);
            var writer = new Internal.IncrementalHashWriter(0); // reuse canonical serializer into temp incremental hash then emit final bytes of 64-bit snapshot
            Internal.CanonicalStateSerializer.WriteObject(state, ref writer);
            var interim = BitConverter.GetBytes(writer.Hash);
            Write(interim);
        }
        if (Random is not null)
        {
            var clone = Random.Clone();
            Span<byte> seedBytes = stackalloc byte[8];
            BitConverter.TryWriteBytes(seedBytes, clone.Seed);
            Write(seedBytes);
            var u1 = clone.NextUInt();
            var u2 = clone.NextUInt();
            Span<byte> peek = stackalloc byte[8];
            BitConverter.TryWriteBytes(peek[..4], u1);
            BitConverter.TryWriteBytes(peek[4..], u2);
            Write(peek);
        }
        var (low, high) = Internal.Hash.XXHash128.Compute(buffer.WrittenSpan);
        return (low, high);
    }

    private static ulong HashBytes(ulong seed, IEnumerable<byte> bytes)
    {
        ulong h = seed;
        foreach (var b in bytes)
        {
            h ^= b;
            h *= 1099511628211UL;
        }
        return h;
    }

    private static ulong HashBytes(ulong seed, ReadOnlySpan<byte> bytes)
    {
        ulong h = seed;
        for (int i = 0; i < bytes.Length; i++)
        {
            h ^= bytes[i];
            h *= 1099511628211UL;
        }
        return h;
    }
}