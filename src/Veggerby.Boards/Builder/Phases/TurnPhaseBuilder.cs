using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Builder.Phases;

/// <summary>
/// Builder helper for creating common turn phase structures.
/// </summary>
/// <remarks>
/// Provides fluent API for defining phase sequences commonly used across board games:
/// - Sequential phases (Reinforce → Attack → Fortify in Risk)
/// - Optional phases (can be skipped)
/// - Repeated phases (attack until you stop)
/// - Player rotation after phases complete
/// 
/// Example usage:
/// <code>
/// var phases = TurnPhaseBuilder.Create()
///     .AddPhase("reinforce")
///     .AddPhase("attack", optional: true)
///     .AddPhase("fortify", optional: true)
///     .WithPlayerRotation()
///     .Build();
/// </code>
/// </remarks>
public sealed class TurnPhaseBuilder
{
    private readonly List<PhaseConfig> _phases = [];
    private bool _rotatePlayerAfterTurn = true;
    private string? _turnLabel;

    private TurnPhaseBuilder()
    {
    }

    /// <summary>
    /// Creates a new turn phase builder.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static TurnPhaseBuilder Create()
    {
        return new TurnPhaseBuilder();
    }

    /// <summary>
    /// Creates a builder with predefined Risk-style phases.
    /// </summary>
    /// <returns>A builder configured with Reinforce → Attack → Fortify phases.</returns>
    public static TurnPhaseBuilder Risk()
    {
        return Create()
            .WithTurnLabel("risk-turn")
            .AddPhase("reinforce")
            .AddPhase("attack", optional: true)
            .AddPhase("fortify", optional: true)
            .WithPlayerRotation();
    }

    /// <summary>
    /// Creates a builder with predefined Monopoly-style phases.
    /// </summary>
    /// <returns>A builder configured with Roll → Move → Action phases.</returns>
    public static TurnPhaseBuilder Monopoly()
    {
        return Create()
            .WithTurnLabel("monopoly-turn")
            .AddPhase("roll")
            .AddPhase("move")
            .AddPhase("action", optional: true)
            .WithPlayerRotation();
    }

    /// <summary>
    /// Creates a builder with predefined Chess-style phases (simple alternating turns).
    /// </summary>
    /// <returns>A builder configured with a single Move phase.</returns>
    public static TurnPhaseBuilder Chess()
    {
        return Create()
            .WithTurnLabel("chess-turn")
            .AddPhase("move")
            .WithPlayerRotation();
    }

    /// <summary>
    /// Sets the label for the turn structure.
    /// </summary>
    /// <param name="label">Human-readable label.</param>
    /// <returns>This builder for fluent chaining.</returns>
    public TurnPhaseBuilder WithTurnLabel(string label)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(label);

        _turnLabel = label;

        return this;
    }

    /// <summary>
    /// Adds a phase to the turn sequence.
    /// </summary>
    /// <param name="name">Phase name (used as identifier).</param>
    /// <param name="optional">Whether this phase can be skipped.</param>
    /// <param name="repeatable">Whether this phase can be repeated within a turn.</param>
    /// <returns>This builder for fluent chaining.</returns>
    public TurnPhaseBuilder AddPhase(string name, bool optional = false, bool repeatable = false)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);

        _phases.Add(new PhaseConfig
        {
            Name = name,
            Optional = optional,
            Repeatable = repeatable
        });

        return this;
    }

    /// <summary>
    /// Enables player rotation after the turn completes.
    /// </summary>
    /// <returns>This builder for fluent chaining.</returns>
    public TurnPhaseBuilder WithPlayerRotation()
    {
        _rotatePlayerAfterTurn = true;

        return this;
    }

    /// <summary>
    /// Disables player rotation after the turn (same player continues).
    /// </summary>
    /// <returns>This builder for fluent chaining.</returns>
    public TurnPhaseBuilder WithoutPlayerRotation()
    {
        _rotatePlayerAfterTurn = false;

        return this;
    }

    /// <summary>
    /// Gets the configured phases.
    /// </summary>
    /// <returns>Read-only list of phase configurations.</returns>
    public IReadOnlyList<PhaseConfig> GetPhases()
    {
        return _phases.AsReadOnly();
    }

    /// <summary>
    /// Gets whether player rotation is enabled.
    /// </summary>
    public bool RotatePlayerAfterTurn => _rotatePlayerAfterTurn;

    /// <summary>
    /// Gets the turn label.
    /// </summary>
    public string TurnLabel => _turnLabel ?? "turn";

    /// <summary>
    /// Builds the phase configuration.
    /// </summary>
    /// <returns>The completed turn phase configuration.</returns>
    public TurnPhaseConfiguration Build()
    {
        if (_phases.Count == 0)
        {
            throw new InvalidOperationException("At least one phase must be defined.");
        }

        return new TurnPhaseConfiguration(_turnLabel ?? "turn", [.. _phases], _rotatePlayerAfterTurn);
    }

    /// <summary>
    /// Configuration for a single phase within a turn.
    /// </summary>
    public sealed class PhaseConfig
    {
        /// <summary>
        /// Gets or sets the phase name.
        /// </summary>
        public required string Name
        {
            get; init;
        }

        /// <summary>
        /// Gets or sets whether this phase is optional (can be skipped).
        /// </summary>
        public bool Optional
        {
            get; init;
        }

        /// <summary>
        /// Gets or sets whether this phase can be repeated.
        /// </summary>
        public bool Repeatable
        {
            get; init;
        }
    }
}

/// <summary>
/// Completed turn phase configuration ready for use.
/// </summary>
/// <param name="TurnLabel">Label for the turn structure.</param>
/// <param name="Phases">Ordered list of phase configurations.</param>
/// <param name="RotatePlayerAfterTurn">Whether to rotate player after turn completes.</param>
public sealed record TurnPhaseConfiguration(
    string TurnLabel,
    IReadOnlyList<TurnPhaseBuilder.PhaseConfig> Phases,
    bool RotatePlayerAfterTurn)
{
    /// <summary>
    /// Gets the index of a phase by name.
    /// </summary>
    /// <param name="name">Phase name to find.</param>
    /// <returns>Index of the phase, or -1 if not found.</returns>
    public int GetPhaseIndex(string name)
    {
        for (int i = 0; i < Phases.Count; i++)
        {
            if (string.Equals(Phases[i].Name, name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Gets the next phase after the specified phase.
    /// </summary>
    /// <param name="currentPhaseName">Current phase name.</param>
    /// <returns>Next phase name, or null if at end of turn.</returns>
    public string? GetNextPhase(string currentPhaseName)
    {
        var index = GetPhaseIndex(currentPhaseName);

        if (index < 0 || index >= Phases.Count - 1)
        {
            return null;
        }

        return Phases[index + 1].Name;
    }

    /// <summary>
    /// Gets whether a specific phase is optional.
    /// </summary>
    /// <param name="phaseName">Phase name to check.</param>
    /// <returns>True if optional; false otherwise.</returns>
    public bool IsOptional(string phaseName)
    {
        var index = GetPhaseIndex(phaseName);

        return index >= 0 && Phases[index].Optional;
    }

    /// <summary>
    /// Gets whether a specific phase is repeatable.
    /// </summary>
    /// <param name="phaseName">Phase name to check.</param>
    /// <returns>True if repeatable; false otherwise.</returns>
    public bool IsRepeatable(string phaseName)
    {
        var index = GetPhaseIndex(phaseName);

        return index >= 0 && Phases[index].Repeatable;
    }
}
