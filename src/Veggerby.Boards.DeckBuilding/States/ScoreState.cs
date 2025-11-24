using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding.States;

/// <summary>
/// Immutable score snapshot for a player produced exactly once at scoring time.
/// </summary>
public sealed class ScoreState : ArtifactState<Player>
{
    /// <summary>Gets the victory point total.</summary>
    public int VictoryPoints
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScoreState"/> class.
    /// </summary>
    public ScoreState(Player player, int victoryPoints) : base(player)
    {
        VictoryPoints = victoryPoints;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScoreState);

    /// <inheritdoc />
    public override bool Equals(IArtifactState other) => Equals(other as ScoreState);

    private bool Equals(ScoreState? other)
    {
        if (other is null)
        {
            return false;
        }
        return Artifact.Equals(other.Artifact) && VictoryPoints == other.VictoryPoints;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, VictoryPoints);
}