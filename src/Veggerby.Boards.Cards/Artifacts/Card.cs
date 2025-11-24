using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Cards.Artifacts;

/// <summary>
/// Immutable card artifact representing a single card identity.
/// </summary>
public sealed class Card : Artifact
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class.
    /// </summary>
    /// <param name="id">Card identifier.</param>
    public Card(string id) : base(id)
    {
    }
}