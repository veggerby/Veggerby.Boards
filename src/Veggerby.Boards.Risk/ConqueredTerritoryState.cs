using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Temporary marker state indicating a territory has been conquered (defender eliminated)
/// and awaits the attacker's army movement via ConquerTerritoryGameEvent.
/// </summary>
/// <remarks>
/// This state replaces the TerritoryState after combat when defender armies reach zero.
/// It is replaced by a new TerritoryState when the attacker completes the conquest.
/// </remarks>
public sealed class ConqueredTerritoryState : IArtifactState
{
    private readonly Tile _territory;
    private readonly Player _previousOwner;

    /// <summary>
    /// Gets the territory that was conquered.
    /// </summary>
    public Tile Territory => _territory;

    /// <summary>
    /// Gets the previous owner (defender who was eliminated).
    /// </summary>
    public Player PreviousOwner => _previousOwner;

    /// <inheritdoc />
    public Artifact Artifact => _territory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConqueredTerritoryState"/> class.
    /// </summary>
    /// <param name="territory">The conquered territory.</param>
    /// <param name="previousOwner">The previous owner.</param>
    public ConqueredTerritoryState(Tile territory, Player previousOwner)
    {
        _territory = territory ?? throw new ArgumentNullException(nameof(territory));
        _previousOwner = previousOwner ?? throw new ArgumentNullException(nameof(previousOwner));
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is ConqueredTerritoryState cts &&
               Equals(cts._territory, _territory) &&
               Equals(cts._previousOwner, _previousOwner);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is IArtifactState state && Equals(state);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_territory, _previousOwner);
    }
}
