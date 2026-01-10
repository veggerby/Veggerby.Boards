using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Marker artifact for standard game outcome.
/// </summary>
internal sealed class StandardGameOutcomeMarker : Artifact
{
    public StandardGameOutcomeMarker() : base("standard-game-outcome-marker")
    {
    }
}

/// <summary>
/// Standard implementation of <see cref="IGameOutcome"/> for reusable outcome representation.
/// </summary>
/// <remarks>
/// This provides a general-purpose outcome state that can be used by any game module.
/// Game modules can either use this directly or create their own module-specific implementations.
/// </remarks>
public sealed class StandardGameOutcome : IArtifactState, IGameOutcome
{
    private static readonly StandardGameOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the terminal condition that ended the game.
    /// </summary>
    public required string TerminalCondition
    {
        get; init;
    }

    /// <summary>
    /// Gets the ordered player results.
    /// </summary>
    public required IReadOnlyList<PlayerResult> PlayerResults
    {
        get; init;
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState? other)
    {
        if (other is not StandardGameOutcome sgo)
        {
            return false;
        }

        if (TerminalCondition != sgo.TerminalCondition)
        {
            return false;
        }

        if (PlayerResults.Count != sgo.PlayerResults.Count)
        {
            return false;
        }

        for (var i = 0; i < PlayerResults.Count; i++)
        {
            if (!PlayerResults[i].Equals(sgo.PlayerResults[i]))
            {
                return false;
            }
        }

        return true;
    }
}
