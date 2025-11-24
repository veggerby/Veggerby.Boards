using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding.States;

/// <summary>
/// Passive state wrapper for a <see cref="CardDefinition"/> enabling deterministic lookup during scoring.
/// </summary>
public sealed class CardDefinitionState : ArtifactState<CardDefinition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CardDefinitionState"/> class.
    /// </summary>
    public CardDefinitionState(CardDefinition definition) : base(definition)
    {
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as CardDefinitionState);

    /// <inheritdoc />
    public override bool Equals(IArtifactState other) => Equals(other as CardDefinitionState);

    private bool Equals(CardDefinitionState? other)
    {
        if (other is null)
        {
            return false;
        }
        return Artifact.Equals(other.Artifact);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Artifact);
    }
}