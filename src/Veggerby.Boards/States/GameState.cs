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
    private readonly GameState _previousState;
    private readonly IRandomSource _random;

    /// <summary>
    /// Gets the collection of artifact states contained in this snapshot.
    /// </summary>
    public IEnumerable<IArtifactState> ChildStates => _childStates.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether this is the initial state (no prior state).
    /// </summary>
    public bool IsInitialState => _previousState is null;

    private GameState(IEnumerable<IArtifactState> childStates = null, GameState previousState = null, IRandomSource random = null)
    {
        _childStates = (childStates ?? Enumerable.Empty<IArtifactState>()).ToDictionary(x => x.Artifact, x => x);
        _previousState = previousState;
        _random = random;
    }

    /// <summary>
    /// Retrieves the state of a specific artifact if present.
    /// </summary>
    /// <typeparam name="T">The artifact state type expected.</typeparam>
    /// <param name="artifact">The artifact.</param>
    /// <returns>The state instance or default when absent or different type.</returns>
    public T GetState<T>(Artifact artifact) where T : IArtifactState
    {
        return _childStates.ContainsKey(artifact) && _childStates[artifact] is T ? (T)_childStates[artifact] : default(T);
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
    public bool Equals(GameState other)
    {
        if (other is null)
        {
            return false;
        }

        if (IsInitialState != other.IsInitialState)
        {
            return false;
        }

        if (ChildStates.Count() != other.ChildStates.Count())
        {
            return false;
        }

        return !ChildStates.Except(other.ChildStates).Any();
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
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
    public GameState Next(IEnumerable<IArtifactState> newStates)
    {
        return new GameState(
            ChildStates
                .Except(newStates ?? Enumerable.Empty<IArtifactState>(), new ArtifactStateEqualityComparer())
                .Concat(newStates ?? Enumerable.Empty<IArtifactState>()),
            this,
            _random?.Clone());
    }

    /// <summary>
    /// Computes artifact state changes relative to an earlier state.
    /// </summary>
    /// <param name="state">The earlier state to compare against.</param>
    /// <returns>Enumeration of changes (additions and modifications).</returns>
    public IEnumerable<ArtifactStateChange> CompareTo(GameState state)
    {
        if (Equals(state))
        {
            return Enumerable.Empty<ArtifactStateChange>();
        }

        var additions = ChildStates
            .Except(state.ChildStates, new ArtifactStateEqualityComparer())
            .Select(to => new ArtifactStateChange(null, to));

        var changes = state.ChildStates
            .Join(ChildStates, x => x.Artifact, x => x.Artifact, (from, to) => new ArtifactStateChange(from, to))
            .Where(x => !x.From.Equals(x.To));

        return [.. changes, .. additions];
    }

    /// <summary>
    /// Creates a new initial <see cref="GameState"/>.
    /// </summary>
    /// <param name="initialStates">The initial artifact states.</param>
    /// <param name="random">Optional deterministic random source snapshot.</param>
    /// <returns>The new initial game state.</returns>
    public static GameState New(IEnumerable<IArtifactState> initialStates, IRandomSource random = null)
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
    public IRandomSource Random => _random;
}