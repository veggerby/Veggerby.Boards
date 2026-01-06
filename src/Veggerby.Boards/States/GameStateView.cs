using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Represents a filtered, player-specific view of a <see cref="GameState"/>.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="GameStateView"/> is a lightweight projection that filters artifact states based on
/// a <see cref="IVisibilityPolicy"/>. It provides the same query interface as <see cref="GameState"/>
/// but returns only states visible to the specified viewer (player or observer role).
/// </para>
/// <para>
/// Views are immutable and deterministic: the same underlying state + policy + viewer always produces
/// the same projection. Hidden states are either redacted (placeholder returned) or omitted entirely
/// based on the policy.
/// </para>
/// </remarks>
public sealed class GameStateView
{
    private readonly GameState _underlyingState;
    private readonly IVisibilityPolicy _policy;
    private readonly Player? _viewer;
    private readonly Lazy<IReadOnlyList<IArtifactState>> _visibleStates;

    /// <summary>
    /// Initializes a new <see cref="GameStateView"/> for the specified viewer.
    /// </summary>
    /// <param name="underlyingState">The full game state to project.</param>
    /// <param name="policy">The visibility policy to apply.</param>
    /// <param name="viewer">The player viewing this projection (may be null for observer roles).</param>
    internal GameStateView(GameState underlyingState, IVisibilityPolicy policy, Player? viewer)
    {
        ArgumentNullException.ThrowIfNull(underlyingState);
        ArgumentNullException.ThrowIfNull(policy);

        _underlyingState = underlyingState;
        _policy = policy;
        _viewer = viewer;
        _visibleStates = new Lazy<IReadOnlyList<IArtifactState>>(ComputeVisibleStates);
    }

    /// <summary>
    /// Gets the collection of artifact states visible to this viewer.
    /// </summary>
    /// <remarks>
    /// Hidden states are either redacted (if the policy returns a placeholder) or omitted entirely.
    /// The result is cached; repeated access returns the same collection instance.
    /// </remarks>
    public IEnumerable<IArtifactState> VisibleStates => _visibleStates.Value;

    /// <summary>
    /// Retrieves the visible state of a specific artifact if present and visible.
    /// </summary>
    /// <typeparam name="T">The artifact state type expected.</typeparam>
    /// <param name="artifact">The artifact.</param>
    /// <returns>
    /// The state instance if visible to this viewer, redacted placeholder if hidden but policy returns
    /// a redaction, or <c>null</c> if the state is absent or fully hidden.
    /// </returns>
    public T? GetState<T>(Artifact artifact) where T : class, IArtifactState
    {
        var fullState = _underlyingState.GetState<IArtifactState>(artifact);
        if (fullState is null)
        {
            return null;
        }

        if (_policy.CanSee(_viewer, fullState))
        {
            return fullState as T;
        }

        var redacted = _policy.Redact(_viewer, fullState);
        return redacted as T;
    }

    /// <summary>
    /// Gets all visible artifact states of the specified type.
    /// </summary>
    /// <typeparam name="T">The artifact state type.</typeparam>
    /// <returns>Enumeration of visible states (redacted where applicable).</returns>
    public IEnumerable<T> GetStates<T>() where T : IArtifactState
    {
        return [.. _visibleStates.Value.OfType<T>()];
    }

    /// <summary>
    /// Provides access to the underlying full game state (internal use only).
    /// </summary>
    /// <remarks>
    /// This property bypasses visibility policies and exposes all state. It is intended for
    /// internal engine use where full state access is required (e.g., rule evaluation, mutators).
    /// Consumer code (UI, AI) should never access this property directly.
    /// </remarks>
    internal GameState UnsafeFullState => _underlyingState;

    private IReadOnlyList<IArtifactState> ComputeVisibleStates()
    {
        var result = new List<IArtifactState>();

        foreach (var state in _underlyingState.ChildStates)
        {
            if (_policy.CanSee(_viewer, state))
            {
                result.Add(state);
            }
            else
            {
                var redacted = _policy.Redact(_viewer, state);
                if (redacted is not null)
                {
                    result.Add(redacted);
                }
            }
        }

        return result.AsReadOnly();
    }
}
